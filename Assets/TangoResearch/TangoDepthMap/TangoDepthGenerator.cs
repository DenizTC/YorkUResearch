using UnityEngine;
using Tango;
using System.Collections.Generic;
using System;
using UnityEngine.UI;

public struct Int2
{
    public int x;
    public int y;

    public Int2(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

/// <summary>
/// This class generates a depth map using the point cloud data.
/// </summary>
public class TangoDepthGenerator : MonoBehaviour, ITangoDepth, ITangoPose
{
    private TangoApplication _tangoApplication;
    private TangoPointCloud _tpc;

    public int _depthMapWidth = 8;
    public int _depthMapHeight = 8;
    public Texture2D _depthTexture;
    public int _DirtyRadius = 4;
    public int _ScaleRadiusSize = 2;
    public int _MaxEdgeIterations = 0;
    public Enums.FillHoleMode _FillMode = Enums.FillHoleMode.NOFILL;
    public Enums.DepthMapMode _DepthMapMode = Enums.DepthMapMode.FULL;
    public bool _DepthPrediction = false;

    private float[,] _depthMap;
    private bool[,] _depthMapDirty;
    private int[,] _depthMapOrder;

    private float[] _sortedPoints = new float[9];
    private int padding = 6;

    private Queue<Int2> _edgesQ = new Queue<Int2>();

    private Vector2 _depthTexureToScreen;

    private TangoUnityDepth _lastTangoDepth = new TangoUnityDepth();

    private void Start()
    {
        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        _tpc = FindObjectOfType<TangoPointCloud>();

        _depthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapDirty = new bool[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapOrder = new int[_depthMapWidth + 2 * padding, _depthMapHeight + 2 * padding];
        _depthTexture = new Texture2D(_depthMapWidth, _depthMapHeight, TextureFormat.RGB24, false);
        _depthTexture.filterMode = FilterMode.Point;


        _depthTexureToScreen = new Vector2(Screen.width / (float)_depthMapWidth,
            Screen.height / (float)_depthMapHeight);
    }

    public void OnDestroy()
    {
        if (_tangoApplication != null)
        {
            _tangoApplication.Unregister(this);
        }
    }

    public void ChangeDepthMapResolution(int width, int height)
    {
        _depthMapWidth = width;
        _depthMapHeight = height;

        _depthMap = new float[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapDirty = new bool[_depthMapWidth + padding * 2, _depthMapHeight + padding * 2];
        _depthMapOrder = new int[_depthMapWidth + 2 * padding, _depthMapHeight + 2 * padding];

        _depthTexture = new Texture2D(_depthMapWidth, _depthMapHeight, TextureFormat.RGB24, false);
        _depthTexture.filterMode = FilterMode.Point;
        _depthTexureToScreen = new Vector2(Screen.width / (float)_depthMapWidth,
            Screen.height / (float)_depthMapHeight);

        _edgesQ.Clear();
    }

    private List<Vector3> _pointsToProject = new List<Vector3>();
    private void FindPointsToProject(TangoUnityDepth depthData)
    {
        _pointsToProject.Clear();

        for (int i = 0; i < depthData.m_pointCount; i++)
        {
            Vector3 XYZ = new Vector3(depthData.m_points[i * 3],
                depthData.m_points[i * 3 + 1],
                depthData.m_points[i * 3 + 2]);

            Vector2 pixelCoord = ComputeScreenCoordinate(XYZ);
            if (pixelCoord.x >= 0 && pixelCoord.x <= 1280 && pixelCoord.y >= 0 && pixelCoord.y <= 720)
            {
                _pointsToProject.Add(XYZ);
            }
        }
        
    }

    public Camera _MainCam;
    private void ProjectPointCloud()
    {

        Vector3 posDiff = _poseTrans - _poseTransAtDepth;

        float dW = _depthMapWidth / 1280f;
        float dH = _depthMapHeight / 720f;

        foreach (Vector3 point in _pointsToProject)
        {
            Vector3 XYZ = point;

            if (_poseTimestamp > _depthTimestamp)
            {

                XYZ = _poseMatAtDepth.MultiplyPoint(XYZ);
                XYZ += new Vector3(-posDiff.x, posDiff.y, -posDiff.z);
                XYZ = _poseMat.inverse.MultiplyPoint(XYZ);
            }

            float Z = 1 - XYZ.z / 4.5f;


            Vector2 pixelCoord = ComputeScreenCoordinate(XYZ);
            if (pixelCoord.x < 0 || pixelCoord.x > 1280 || pixelCoord.y < 0 || pixelCoord.y > 720)
            {
                continue;
            }
            int x = (int)(pixelCoord.x * dW);
            int y = (int)(pixelCoord.y * dH);

            _depthMap[x + padding, y + padding] = Z;
            _depthMapDirty[x + padding, y + padding] = true;

            if (_depthMapOrder[x + padding, y + padding] == 0)
            {

                if (_DepthMapMode == Enums.DepthMapMode.MASKED)
                {
                    Ray ray =
                        _MainCam.ScreenPointToRay(
                            new Vector3(x * _depthTexureToScreen.x, (_depthMapHeight - y) * _depthTexureToScreen.y, 0));
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

    private void SetDepthArray()
    {

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
                            _depthTexture.SetPixel(i + w, (_depthMapHeight - j) + h, lastFound);
                            _depthMapDirty[i + padding + w, j + padding + h] = false;
                        }
                    }

                } // scale point

            } // height
        } // row



    }

    private void SetDepthArrayIterative(int iterations)
    {

        int cur = 1;

        if (iterations <= 0)
        {
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

        int max = _edgesQ.Count * iterations * iterations;
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

    private void FillEdges(int curPriority)
    {
        if (_FillMode == Enums.FillHoleMode.NOFILL) return;

        int cur = curPriority;

        int curIteration = 0;

        while (_edgesQ.Count > 0 && curIteration++ < _depthMapWidth * _depthMapHeight * 1)
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

                        _depthMap[e.x + w + padding, e.y + h + padding] = lastColor.r;
                        _depthTexture.SetPixel(e.x + w, _depthMapHeight - e.y - h, lastColor);
                        _edgesQ.Enqueue(new Int2(e.x + w, e.y + h));

                    } // fill pixel
                } // h
            } // w

        } // while holes exist



    }

    private void GenerateDepthMap()
    {
        if (_edgesQ.Count > 0) _edgesQ.Clear();
        _depthMapOrder = new int[_depthMapWidth + 2 * padding, _depthMapHeight + 2 * padding];
        ProjectPointCloud();

        if (_DepthMapMode == Enums.DepthMapMode.FULL)
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

    public float[,] GetDepthMap(TangoUnityDepth depth) {

        FindPointsToProject(depth);
        GenerateDepthMap();
        return _depthMap;
    }

    public static float ProjectToWorldDepth(float depth) {
        return 1 + depth * 4.5f;
    }

    public Text _DepthInfoText;
    public Text _RGBInfoText;
    private Vector3 _poseTransAtDepth = Vector3.zero;
    private Quaternion _poseQuatAtDepth = Quaternion.identity;
    private double _depthTimestamp;
    private Matrix4x4 _poseMatAtDepth = Matrix4x4.identity;
    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {

        //_DepthInfoText.text = tangoDepth.m_timestamp.ToString();
        //return;

        _lastTangoDepth = tangoDepth;
        _depthTimestamp = tangoDepth.m_timestamp;
        FindPointsToProject(_lastTangoDepth);

        if (!_DepthPrediction)
        {
            GenerateDepthMap();
            return;
        }

        TangoCoordinateFramePair pair = new TangoCoordinateFramePair();
        pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;

        TangoPoseData pd = new TangoPoseData();
        PoseProvider.GetPoseAtTime(pd, _depthTimestamp, pair);

        TangoSupport.TangoPoseToWorldTransform(pd, out _poseTransAtDepth, out _poseQuatAtDepth);

        _poseQuatAtDepth.eulerAngles =
            new Vector3(-_poseQuatAtDepth.eulerAngles.x, _poseQuatAtDepth.eulerAngles.y, -_poseQuatAtDepth.eulerAngles.z);

        _poseMatAtDepth = Matrix4x4.TRS(Vector3.zero, _poseQuatAtDepth, Vector3.one);

    }

    private Vector3 _poseTrans = Vector3.zero;
    private Quaternion _poseQuat = Quaternion.identity;
    private double _poseTimestamp;
    private Matrix4x4 _poseMat = Matrix4x4.identity;
    public void OnTangoPoseAvailable(TangoPoseData poseData)
    {

        if (!_DepthPrediction) return;

        _poseTimestamp = poseData.timestamp;
        TangoSupport.TangoPoseToWorldTransform(poseData, out _poseTrans, out _poseQuat);

        _poseQuat.eulerAngles =
            new Vector3(-_poseQuat.eulerAngles.x, _poseQuat.eulerAngles.y, -_poseQuat.eulerAngles.z);

        _poseMat = Matrix4x4.TRS(Vector3.zero, _poseQuat, Vector3.one);

        GenerateDepthMap();
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
