using UnityEngine;
using Tango;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public struct Int2
{
    public int x;
    public int y;

    public Int2(int x, int y) {
        this.x = x;
        this.y = y;
    }
}

    /// <summary>
    /// This class generates a depth map using the point cloud data.
    /// </summary>
    public class PointCloudToDepthMap : MonoBehaviour, ITangoLifecycle, ITangoDepth
{

    public TangoARPoseController _ARCam;
    public TangoPointCloud _PointCloud;
    public Renderer _DepthMapQuad;

    private TangoApplication _tangoApplication;
    private TangoCameraIntrinsics _ccIntrinsics = new TangoCameraIntrinsics();

    public int _depthMapWidth = 8;
    public int _depthMapHeight = 8;
    public Texture2D _depthTexture;
    public int _DirtyRadius = 4;
    public int _ScaleRadiusSize = 2;
    public int _MaxEdgeIterations = 0;
    public Enums.FillHoleMode _FillMode = Enums.FillHoleMode.NOFILL;
    public Enums.DepthMapMode _DepthMapMode = Enums.DepthMapMode.FULL;

    private float[,] _depthMap;
    private bool[,] _depthMapDirty;
    private float[,] _lastDepthMap;
    private float[,] _clipDepthMap = new float[81,46];
    private bool[,] _clipDepthMapDirty = new bool[81, 46];
    private int[,] _depthMapOrder;

    private float[] _sortedPoints = new float[9];
    private int padding = 6;

    private List<Int2> _edgePixels = new List<Int2>();
    private Queue<Int2> _edgesQ = new Queue<Int2>();

    public RenderTexture RT;
    private Vector2 _depthTexureToScreen;

    private void Start()
    {
        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        _depthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _lastDepthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapDirty = new bool[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapOrder = new int[_depthMapWidth + 2 * padding, _depthMapHeight + 2 * padding];
        _depthTexture = new Texture2D(_depthMapWidth, _depthMapHeight, TextureFormat.RGB24, false);
        _depthTexture.filterMode = FilterMode.Point;


        RT = new RenderTexture(80, 45, 0);
        _depthTexureToScreen = new Vector2(Screen.width / (float)_depthMapWidth,
            Screen.height / (float)_depthMapHeight);
    }

    //public float temp = 0;
    //private void Update() {
    //    if (Input.GetMouseButtonDown(1)) {
    //        Debug.Log(Input.mousePosition + " screenSize: " + Screen.width + "x" + Screen.height);
    //        Ray r00 = Camera.main.ScreenPointToRay(new Vector3(temp * _depthTexureToScreen.x, temp * _depthTexureToScreen.y, 0));
    //        Ray r01 = Camera.main.ScreenPointToRay(new Vector3(temp * _depthTexureToScreen.x, (_depthMapHeight - temp) * _depthTexureToScreen.y, 0));
    //        Ray r11 = Camera.main.ScreenPointToRay(new Vector3((_depthMapWidth - temp) * _depthTexureToScreen.x, (_depthMapHeight - temp) * _depthTexureToScreen.y, 0));
    //        Ray r10 = Camera.main.ScreenPointToRay(new Vector3((_depthMapWidth - temp) * _depthTexureToScreen.x, temp * _depthTexureToScreen.y, 0));
    //        Debug.DrawRay(r00.origin, r00.direction.normalized * 4.5f, Color.red, 10f);
    //        Debug.DrawRay(r01.origin, r01.direction.normalized * 4.5f, Color.green, 10f);
    //        Debug.DrawRay(r11.origin, r11.direction.normalized * 4.5f, Color.blue, 10f);
    //        Debug.DrawRay(r10.origin, r10.direction.normalized * 4.5f, Color.yellow, 10f);
    //    }
    //}

    public void OnDestroy()
    {
        if (_tangoApplication != null)
        {
            _tangoApplication.Unregister(this);
        }
    }

    public void ChangeDepthMapResolution(int width, int height) {
        _depthMapWidth = width;
        _depthMapHeight = height;

        _depthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _lastDepthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapDirty = new bool[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapOrder = new int[_depthMapWidth + 2 * padding, _depthMapHeight + 2 * padding];

        _depthTexture = new Texture2D(_depthMapWidth, _depthMapHeight, TextureFormat.RGB24, false);
        _depthTexture.filterMode = FilterMode.Point;
        _depthTexureToScreen = new Vector2(Screen.width / (float)_depthMapWidth,
            Screen.height / (float)_depthMapHeight);

        _edgesQ.Clear();
    }

    private void ProjectPointCloud(ref TangoUnityDepth tangoUnityDepth) {   

        float dW = _depthMapWidth / 1280f;
        float dH = _depthMapHeight / 720f;
        float dCW = 80 / 1280f;
        float dCH = 45 / 720f;

        for (int i = 0; i < tangoUnityDepth.m_pointCount; i++)
        {
            float Z = 1 - tangoUnityDepth.m_points[i * 3 + 2] / 4.5f;
            //Z = Mathf.Pow(Z, 0.5f);

            Vector3 XYZ = new Vector3(tangoUnityDepth.m_points[i * 3],
                tangoUnityDepth.m_points[i * 3 + 1],
                tangoUnityDepth.m_points[i * 3 + 2]);
            Vector2 pixelCoord = ComputeScreenCoordinate(XYZ);
            if (pixelCoord.x < 0 || pixelCoord.x > 1280 || pixelCoord.y < 0 || pixelCoord.y > 720)
            {
                continue;
            }
            int x = (int)(pixelCoord.x * dW);
            int y = (int)(pixelCoord.y * dH);

            int xC = (int)(pixelCoord.x * dCW);
            int yC = (int)(pixelCoord.y * dCH);

            _clipDepthMap[xC, yC] = 1;
            _clipDepthMapDirty[xC, yC] = true;

            _depthMap[x + padding, y + padding] = Z;
            _depthMapDirty[x+padding, y+ padding] = true;

            if (_depthMapOrder[x + padding, y + padding] == 0)
            {

                if (_DepthMapMode == Enums.DepthMapMode.MASKED)
                {
                    Ray ray =
                        Camera.main.ScreenPointToRay(new Vector3(x * _depthTexureToScreen.x, y * _depthTexureToScreen.y, 0));
                    if (Physics.Raycast(ray, 10f))
                    {
                        //Debug.DrawRay(ray.origin, ray.direction * 5, Color.blue);
                        _depthMapOrder[x + padding, y + padding] = 1;
                        _edgesQ.Enqueue(new Int2(x, y));
                        _depthMap[x + padding, y + padding] = Z;
                        _depthTexture.SetPixel(x, _depthMapHeight - y, new Color(Z, Z, Z));
                    }
                }
                else
                {
                    _depthMapOrder[x + padding, y + padding] = 1;
                    //_edgesQ.Enqueue(new Int2(x, y));
                    _depthMap[x + padding, y + padding] = Z;
                    _depthTexture.SetPixel(x, _depthMapHeight - y, new Color(Z, Z, Z));
                }

            }
        }
    }

    private void GenerateClipDepthMap() {
        int clipOffset = 4;
        for (int i = 0; i < 80; i++)
        {
            for (int j = 0; j < 45; j++)
            {


                if (!_clipDepthMapDirty[i, j])
                {

                    if (i - clipOffset < 0 || i + clipOffset >= 80 ||
                        j - clipOffset < 0 || j + clipOffset >= 45)
                        continue;

                    if (!_clipDepthMapDirty[i + clipOffset, j] &&
                       !_clipDepthMapDirty[i - clipOffset, j] &&
                       !_clipDepthMapDirty[i, j + clipOffset] &&
                       !_clipDepthMapDirty[i, j - clipOffset])
                        _clipDepthMap[i, j] = 0;
                }
                _clipDepthMapDirty[i, j] = false;
            }
        }
    }

    private bool IsEdgePoint(int x, int y) {
        for (int w = -1; w < 2; w++)
        {
            for (int h = -1; h < 2; h++)
            {
                if (w + h == 0) continue;
                if (_depthMapDirty[x + padding + w, y + padding + h])
                    return true;
            }
        }
        return false;
    }

    private void ProcessEdgePoints() {
        foreach (var uv in _edgePixels)
        {
            _lastDepthMap[uv.x, uv.y] = 1;
            //_depthTexture.SetPixel(uv.x, _depthMapHeight - uv.y, Color.white);
        }
        _edgePixels.Clear();
    }

    private void SetDepthArray() {

        int cur = 1;

        Color lastFound = new Color();
        for (int i = 0; i < _depthMapWidth; i++)
        {
            for (int j = 0; j < _depthMapHeight; j++)
            {

                if (_depthMapDirty[i + padding, j + padding])
                {
                    float d = _depthMap[i + padding, j + padding];
                    lastFound = new Color(d, d, d);
                    for (int w = -1; w <= 1; w++)
                    {
                        for (int h = -1; h <= 1; h++)
                        {
                            _lastDepthMap[i + padding + w, j + padding + h] = lastFound.r;
                            _depthTexture.SetPixel(i + w, (_depthMapHeight - j) + h, lastFound);
                            _depthMapDirty[i + padding + w, j + padding + h] = false;
                        }
                    }

                } // scale point

            } // height
        } // row
        


    }

    private void SetDepthArray(int pointScale) {
        Color lastFound = new Color();
        for (int i = 0; i < _depthMapWidth; i++)
        {
            for (int j = 0; j < _depthMapHeight; j++)
            {

                if (_depthMapDirty[i + padding, j + padding])
                {
                    float d = _depthMap[i + padding, j + padding];
                    lastFound = new Color(d, d, d);
                    _depthTexture.SetPixel(i, _depthMapHeight - j, lastFound);

                    for (int w = -pointScale; w <= pointScale; w++)
                    {
                        for (int h = -pointScale; h <= pointScale; h++)
                        {
                            _lastDepthMap[i + padding + w, j + padding + h] = lastFound.r;
                            _depthTexture.SetPixel(i + w, (_depthMapHeight - j) + h, lastFound);
                        }
                    }

                }
                else
                {
                    float d = _lastDepthMap[i + padding, j + padding];
                    lastFound = new Color(d, d, d);
                    _depthTexture.SetPixel(i, _depthMapHeight - j, lastFound);
                    _lastDepthMap[i + padding, j + padding] = 0;

                }
                _depthMapDirty[i + padding, j + padding] = false;
            }
        }
    }

    private void SetDepthArrayIterative(int iterations)
    {

        int cur = 1;

        if (iterations <= 0) {
            for (int i = 0; i < _depthMapWidth; i++)
            {
                for (int j = 0; j < _depthMapHeight; j++)
                {
                    int priority = _depthMapOrder[i + padding, j + padding];
                    if (priority == 0)
                    {
                        _depthMap[i + padding, j + padding] = 0;
                        _depthTexture.SetPixel(i, _depthMapHeight - j, Color.black);
                    }
                    else
                    {
                        _edgesQ.Enqueue(new Int2(i, j));
                    }
                } // height
            } // row
            //Debug.Log(_edgesQ.Count);
            FillEdges(cur);
            return;
        }

        Color lastFound = new Color();
        while (cur - 1 < iterations)
        {
            for (int i = 0; i < _depthMapWidth; i++)
            {
                for (int j = 0; j < _depthMapHeight; j++)
                {
                    int priority = _depthMapOrder[i + padding, j + padding];
                    if (priority == 0 && cur == iterations)
                    {
                        _depthTexture.SetPixel(i, _depthMapHeight - j, Color.black);
                        _depthMap[i + padding, j + padding] = 0;
                    }
                    else if (priority == cur)
                    {
                        float d = _depthMap[i + padding, j + padding];
                        lastFound = new Color(d, d, d);
                        _depthTexture.SetPixel(i, (_depthMapHeight - j), lastFound);

                        for (int w = -1; w <= 1; w++)
                        {
                            for (int h = -1; h <= 1; h++)
                            {
                                if (_depthMapOrder[i + padding + w, j + padding + h] == 0)
                                {
                                    _depthMap[i + padding + w, j + padding + h] = lastFound.r;
                                    _depthTexture.SetPixel(i + w, _depthMapHeight - (j + h), lastFound);
                                    _depthMapOrder[i + padding + w, j + padding + h] = cur + 1;
                                    if (cur == iterations)
                                        _edgesQ.Enqueue(new Int2(i + w, j + h));
                                }
                            }
                        }

                    } // scale point

                } // height
            } // row

            cur++;

        } // iterations
        //Debug.Log(_edgesQ.Count);
        FillEdges(cur);

    }

    private void SetDepthArrayIterativeExperimental(int iterations)
    {


        Color lastFound = new Color();

        int count = 0;

        int max = _edgesQ.Count * iterations*iterations;
        //int max = _depthMapWidth * _depthMapHeight / (7 - iterations);
        while (_edgesQ.Count > 0 && count < max)
        {
            Int2 e = _edgesQ.Dequeue();

            float d = _depthMap[e.x + padding, e.y + padding];

            lastFound = new Color(d, d, d);
            //_depthTexture.SetPixel(e.x, _depthMapHeight - e.y, lastFound);

            for (int w = -1; w <= 1; w++)
            {
                for (int h = -1; h <= 1; h++)
                {
                    if (_depthMapOrder[e.x + padding + w, e.y + padding + h] == 0)
                    {
                        _depthMapOrder[e.x + padding + w, e.y + padding + h] = 1;
                        if (e.x <= 0 || e.x >= _depthMapWidth ||
                            e.y <= 0 || e.y >= _depthMapHeight)
                            continue;
                        
                        _depthMap[e.x + padding + w, e.y + padding + h] = lastFound.r;
                        _depthTexture.SetPixel(e.x + w, _depthMapHeight - (e.y + h), lastFound);
                        _edgesQ.Enqueue(new Int2(e.x + w, e.y + h));
                    }
                }
            }

            count++;

        } // iterations


        for (int i = 0; i < _depthMapWidth; i++)
        {
            for (int j = 0; j < _depthMapHeight; j++)
            {
                int priority = _depthMapOrder[i + padding, j + padding];
                if (priority == 0)
                {
                    _depthTexture.SetPixel(i, _depthMapHeight - j, Color.black);
                    //_depthMap[i + padding, j + padding] = 0;
                }
            }
        }

    }


    private void Swap(int a, int b)
    {
        float temp = Mathf.Max(_sortedPoints[a], _sortedPoints[b]);
        _sortedPoints[a] = Mathf.Min(_sortedPoints[a], _sortedPoints[b]);
        _sortedPoints[b] = temp;
    }

    private void MedianFilterEdges() {

        foreach (var uv in _edgePixels)
        {
            for (int i = 0; i < 3; i++)
                for (int j = 0; j < 3; j++)
                    _sortedPoints[i * 3 + j] = _depthMap[uv.x + padding + (i-1), uv.y + padding + (j-i)];

            // Sorting network generated from: http://pages.ripco.net/~jgamble/nw.html
            Swap(0, 1); Swap(3, 4); Swap(6, 7);
            Swap(1, 2); Swap(4, 5); Swap(7, 8);
            Swap(0, 1); Swap(3, 4); Swap(6, 7); Swap(0, 3);
            Swap(3, 6); Swap(0, 3); Swap(1, 4);
            Swap(4, 7); Swap(1, 4); Swap(2, 5);
            Swap(5, 8); Swap(2, 5); Swap(1, 3); Swap(5, 7);
            Swap(2, 6); Swap(4, 6);
            Swap(2, 4); Swap(2, 3);
            Swap(5, 6);

            _depthMap[uv.x + padding, uv.y + padding] = _sortedPoints[4];
            _depthTexture.SetPixel(uv.x, _depthMapHeight - uv.y, 
                new Color(_sortedPoints[4], _sortedPoints[4], _sortedPoints[4]));

        }
        _edgePixels.Clear();
    }

    private float MeanPoint(Int2 uv)
    {
        int count = 0;
        float sum = 0;
        //for (int w = -1; w <= 1; w++)
        //{
        //    for (int h = -1; h <= 1; h++)
        //    {

        //        if (_depthMapOrder[uv.x + w + padding, uv.y + h + padding] != 0)
        //        {
        //            count++;
        //            sum += _depthMap[uv.x + w + padding, uv.y + h + padding];
        //        }
        //    }
        //}


        if (_depthMapOrder[uv.x + 0 + padding, uv.y + 1 + padding] != 0)
        {
            count++;
            sum += _depthMap[uv.x + 0 + padding, uv.y + 1 + padding];
        }
        if (_depthMapOrder[uv.x + 0 + padding, uv.y - 1 + padding] != 0)
        {
            count++;
            sum += _depthMap[uv.x + 0 + padding, uv.y - 1 + padding];
        }
        if (_depthMapOrder[uv.x + 1 + padding, uv.y + 0 + padding] != 0)
        {
            count++;
            sum += _depthMap[uv.x + 1 + padding, uv.y + 0 + padding];
        }
        if (_depthMapOrder[uv.x - 1 + padding, uv.y + 0 + padding] != 0)
        {
            count++;
            sum += _depthMap[uv.x - 1 + padding, uv.y + 0 + padding];
        }


        return sum / count;

    }

    private void FillEdges(int curPriority) {
        if (_FillMode == Enums.FillHoleMode.NOFILL) return;

        int cur = curPriority;

        int curIteration = 0; 
        
        while (_edgesQ.Count > 0 && curIteration++ < _depthMapWidth*_depthMapHeight * 1)
        {
            Int2 e = _edgesQ.Dequeue();
            float f = _depthMap[e.x + padding, e.y + padding];

            Color lastColor = new Color(f, f, f);
            for (int w = -1; w <= 1; w++)
            {
                for (int h = -1; h <= 1; h++)
                {
                    if (_depthMapOrder[e.x + w + padding, e.y + h + padding] == 0)
                    {
                        _depthMapOrder[e.x + w + padding, e.y + h + padding] = cur + 1;
                        if (e.x <= 0 || e.x >= _depthMapWidth ||
                            e.y <= 0 || e.y >= _depthMapHeight)
                            continue;

                        if (_FillMode == Enums.FillHoleMode.MEAN) {
                            float mean = MeanPoint(new Int2(e.x, e.y));
                            lastColor = new Color(mean, mean, mean);
                        }
                        
                        _depthMap[e.x + w + padding, e.y + h + padding] = lastColor.r;
                        _depthTexture.SetPixel(e.x + w, _depthMapHeight - e.y - h, lastColor);
                        _edgesQ.Enqueue(new Int2(e.x + w, e.y + h));

                    } // fill pixel


                } // h
            } // w

        } // while holes exist


        
    }

    private void GenerateDepthMap_ScaledPoints(ref TangoUnityDepth tangoUnityDepth)
    {
        if (_edgesQ.Count > 0) _edgesQ.Clear();
        _depthMapOrder = new int[_depthMapWidth + 2*padding, _depthMapHeight + 2*padding];
        ProjectPointCloud(ref tangoUnityDepth);
        //GenerateClipDepthMap();

        //SetDepthArray();
        //SetDepthArray(_ScaleRadiusSize);

        if(_DepthMapMode == Enums.DepthMapMode.FULL)
            SetDepthArrayIterative(_ScaleRadiusSize);
        else
            SetDepthArrayIterativeExperimental(_ScaleRadiusSize);

        _depthTexture.Apply();              
    }

    /// <summary>
    /// Computes the screen coordinate given a point cloud point.
    /// From: http://stackoverflow.com/questions/30640149/how-to-color-point-cloud-from-image-pixels
    /// </summary>
    private Vector2 ComputeScreenCoordinate(Vector3 tangoDepthPoint)
    {
        Vector2 imageCoords = new Vector2(tangoDepthPoint.x / tangoDepthPoint.z, tangoDepthPoint.y / tangoDepthPoint.z);
        float r2 = Vector2.Dot(imageCoords, imageCoords);
        float r4 = r2 * r2;
        float r6 = r2 * r4;
        imageCoords *= 1.0f + 0.228532999753952f * r2 + -0.663019001483917f * r4 + 0.642908990383148f * r6;
        Vector3 ic3 = new Vector3(imageCoords.x, imageCoords.y, 1);

#if USEMATRIXMATH
        Matrix4x4 cameraTransform  = new Matrix4x4();
        cameraTransform.SetRow(0,new Vector4(1042.73999023438f,0,637.273986816406f,0));
        cameraTransform.SetRow(1, new Vector4(0, 1042.96997070313f, 352.928985595703f, 0));
        cameraTransform.SetRow(2, new Vector4(0, 0, 1, 0));
        cameraTransform.SetRow(3, new Vector4(0, 0, 0, 1));
        Vector3 pixelCoords = cameraTransform * ic3;
        return new Vector2(pixelCoords.x, pixelCoords.y);
#else
        //float v1 = 1042.73999023438f * imageCoords.x + 637.273986816406f;
        //float v2 = 1042.96997070313f * imageCoords.y + 352.928985595703f;
        //float v3 = 1;
        return new Vector2(1042.73999023438f * imageCoords.x + 637.273986816406f, 1042.96997070313f * imageCoords.y + 352.928985595703f);
#endif

        //float dx = Math.Abs(v1 - pixelCoords.x);
        //float dy = Math.Abs(v2 - pixelCoords.y);
        //float dz = Math.Abs(v3 - pixelCoords.z);
        //if (dx > float.Epsilon || dy > float.Epsilon || dz > float.Epsilon)
        //    UnityEngine.Debug.Log("Well, that didn't work");
        //return new Vector2(v1, v2);
    }

    public Text _DepthInfoText;
    public Text _RGBInfoText;
    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        
        //Camera.main.targetTexture = RT;

        //Camera.main.Render();
        //Camera.main.targetTexture = null;

        //_DepthInfoText.text = "Depth:" + tangoDepth.m_timestamp;
        //Debug.Log("Depth:" + tangoDepth.m_timestamp);
        GenerateDepthMap_ScaledPoints(ref tangoDepth);
        //TangoRGB_Out.TangoRGBGenerator.SetSyncedRGB();
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        //_RGBInfoText.text = "RGB  :" + imageBuffer.timestamp + " camID: " + cameraId;
        //Debug.Log("RGB  :" + imageBuffer.timestamp + " camID: " + cameraId);
        if (cameraId == TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR) {

        }
    }

    /// <summary>
    /// This is called when the permission granting process is finished.
    /// </summary>
    /// <param name="permissionsGranted"><c>true</c> if permissions were granted, otherwise <c>false</c>.</param>
    public void OnTangoPermissions(bool permissionsGranted)
    {
    }

    /// <summary>
    /// This is called when succesfully connected to the Tango service.
    /// </summary>
    public void OnTangoServiceConnected()
    {
        _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    /// <summary>
    /// This is called when disconnected from the Tango service.
    /// </summary>
    public void OnTangoServiceDisconnected()
    {
    }


}