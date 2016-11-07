using UnityEngine;
using UnityEngine.UI;
using Tango;
using System.Collections.Generic;
using System;
using System.Collections;

public class ColorPoint
{
    /// <summary>
    /// The world point of this pixel.
    /// </summary>
    public Vector3 XYZ;
    public Vector3 RGB;
    public float Luma;
    public int X;
    public int Y;
    public ColorPoint() {
        X = 0;
        Y = 0;
        Luma = 0;
        XYZ = Vector3.zero;
        RGB = Vector3.zero;
    }
    public ColorPoint(int x, int y, float luma, Vector3 rgb, Vector3 xyz)
    {
        X = x;
        Y = y;
        Luma = luma;
        RGB = rgb;
        XYZ = xyz;
    }
    public ColorPoint(ColorPoint p)
    {
        X = p.X;
        Y = p.Y;
        Luma = p.Luma;
        RGB = p.RGB;
        XYZ = p.XYZ;
    }
}

public class FindLight : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle, ITangoDepth
{
    private TangoApplication _tangoApplication;

    public static FindLight _LightDetector;

    public Camera _MainCam;
    public TangoPointCloud _PointCloud;
    public RenderTexture _InTexture;
    public Texture2D _OutTexture = null;

    public Transform _LightPlaceholder;
    public Button _ButtonCreateLight;
    public Slider _SliderThreshold;
    public Slider _SliderSpotSize;

    /// <summary>
    /// If true, bright spots are updated when ever new image is available, otherwise,
    /// bright spots updated manually in the on click event of _ButtonFindLights.
    /// </summary>
    private bool _Continuous = true;
    private float _Threshold = 0.65f;
    private int _SpotSize = 10;
    private TangoUnityImageData _lastImageBuffer = null;
    private bool _isOn = false;
    private List<ColorPoint> _foundLights = new List<ColorPoint>();
    private List<GameObject> _foundLightsPlaceholder = new List<GameObject>();
    /// <summary>
    /// If set, then the depth camera is on and we are waiting for the next depth update.
    /// </summary>
    private bool m_findPlaneWaitingForDepth;

    void Awake() {
        if (!_LightDetector)
            _LightDetector = this;
    }

    void Start () {

        

        _tangoApplication = FindObjectOfType<TangoApplication>();
        //if (_tangoApplication != null)
        //{
        //    _tangoApplication.Register(_LightDetector);
        //}

        if(_ButtonCreateLight)
            _ButtonCreateLight.onClick.AddListener(onClickCreateLight);

        _SliderThreshold.value = _Threshold;
        _SliderSpotSize.value = _SpotSize / 10;
        _SliderThreshold.onValueChanged.AddListener(onValueChangedThreshold);
        _SliderSpotSize.onValueChanged.AddListener(onValueChangedSpotSize);
        

        _OutTexture = new Texture2D(1280 / 8, 720 / 8);

        transform.gameObject.SetActive(false);
        //TurnOff();
    }

    void Update()
    {
        if (Input.touchCount == 1) {
            Touch t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Began ||
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(t.fingerId))
                return;
            CreateLight();
        }

        if (Input.GetMouseButtonDown(1)) {
            if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
                return;
            CreateLight();
        }
    }

    #region UIEvents

    private void onClickCreateLight()
    {
        CreateLight();
    }

    private void onValueChangedSpotSize(float value)
    {
        _SpotSize = (int)value * 10;
    }

    private void onValueChangedThreshold(float value)
    {
        _Threshold = value;
    }

    #endregion

    public void TurnOn() {
        if (_isOn) return;
        _isOn = true;
        _tangoApplication.Register(_LightDetector);
        _foundLights = new List<ColorPoint>();
        _foundLightsPlaceholder = new List<GameObject>();
        transform.gameObject.SetActive(true);
        MessageManager._MessageManager.PushMessage("Tap screen to place light source.", 3);
    }

    public List<ColorPoint> TurnOff() {
        if (!_isOn) {
            return new List<ColorPoint>();
        }

        _isOn = false;
        _tangoApplication.Unregister(_LightDetector);
        if (!_tangoApplication.m_enable3DReconstruction)
            _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
        StopAllCoroutines();
        for (int i = 0; i < _foundLightsPlaceholder.Count; i++)
        {
            Destroy(_foundLightsPlaceholder[i]);
        }
        
        transform.gameObject.SetActive(false);
        return _foundLights;
    }

    public bool IsRunning() {
        return _isOn;
    }

    public void CreateLight() {
        ColorPoint p;
        if (FindBrightSpots(_lastImageBuffer, ref _OutTexture, _Threshold, _SpotSize, out p))
        {

            Vector2 pos = new Vector2(p.X * _lastImageBuffer.width / (float)_OutTexture.width,
                p.Y * _lastImageBuffer.height / (float)_OutTexture.height);
            pos = new Vector2(pos.x / _lastImageBuffer.width * Screen.width,
                pos.y / _lastImageBuffer.height * Screen.height);

            //_QuadResult.sharedMaterial.mainTexture = _OutTexture;
            StartCoroutine(_WaitForDepthAndFindPlane(pos, p));
        }
    }

    /// <summary>
    /// Finds the bright spots of the color image.
    /// </summary>
    /// <param name="imageBuffer">The image buffer.</param>
    /// <param name="outTexture">The out texture.</param>
    /// <param name="threshold">The brightness threshold.</param>
    /// <param name="spotSize">Size of the bright spot.</param>
    /// <param name="outPixel">The brightest pixel found (centre point of all the bright spots taken into account).</param>
    /// <returns>True if a pixel was found, false otherwise.</returns>
    public static bool FindBrightSpots(TangoUnityImageData imageBuffer, 
        ref Texture2D outTexture, 
        float threshold, 
        int spotSize,
        out ColorPoint outPixel)
    {

        int width = (int)imageBuffer.width;
        int height = (int)imageBuffer.height;

        // Brightest pixel in the image.
        ColorPoint brightestPixel = new ColorPoint();
        
        // Pixels whose luma is greater than specified threshold.
        List<ColorPoint> brightSpots = new List<ColorPoint>();

        // Stack of the brightest pixels. Brightest towards the head.
        Stack<ColorPoint> brightnessStack = new Stack<ColorPoint>();

        int wS = width / outTexture.width;
        int hS = height / outTexture.height;
        for (int i = 0; i < outTexture.height; ++i)
        {
            for (int j = 0; j < outTexture.width; ++j)
            {
                int iS = i * hS;
                int jS = j * wS;

                Vector3 yuv = TangoHelpers.GetYUV(imageBuffer, jS, iS);
                Vector3 rgb = ImageProcessing.YUVToRGB(yuv);

                if (yuv.x / 255f > threshold)
                {
                    if (yuv.x / 255f >= brightestPixel.Luma - 0.02f)
                    {
                        if (yuv.x / 255f >= brightestPixel.Luma)
                            brightestPixel.Luma = yuv.x / 255f;

                        brightestPixel.X = j;
                        brightestPixel.Y = height / hS - i - 1;
                        brightnessStack.Push(new ColorPoint(brightestPixel));
                    }

                    brightSpots.Add(new ColorPoint(j, height / hS - i - 1, yuv.x / 255f, rgb, Vector3.zero));
                    outTexture.SetPixel(j, height / hS - i - 1, new Color(rgb.x, rgb.y * 0.5f, rgb.z * 0.5f));
                }
                else
                {
                    outTexture.SetPixel(j, height / hS - i - 1, new Color(rgb.x, rgb.y, rgb.z));
                }
            }
        }

        if (brightnessStack.Count <= 0)
        {
            outTexture.Apply();
            outPixel = brightestPixel;
            return false;
        }

        ColorPoint tempBrightest = new ColorPoint(0, 0, brightestPixel.Luma, brightestPixel.RGB, Vector3.zero);
        int bCount = 0;
        for (int i = 0; i < brightnessStack.Count; i++)
        {
            ColorPoint cur = brightnessStack.Pop();
            if (cur.Luma >= brightestPixel.Luma)
            {
                tempBrightest.X += cur.X;
                tempBrightest.Y += cur.Y;
                bCount++;
                //outTexture.SetPixel(cur.X, cur.Y, Color.cyan);
            }
            else
            {
                break;
            }
        }

        tempBrightest.X /= bCount;
        tempBrightest.Y /= bCount;
        brightestPixel.X = tempBrightest.X;
        brightestPixel.Y = tempBrightest.Y;

        ColorPoint ave = new ColorPoint();
        int count = 0;
        foreach (ColorPoint p in brightSpots)
        {
                
            if (p.X < brightestPixel.X + spotSize / 2 &&
                p.X > brightestPixel.X - spotSize / 2 &&
                p.Y < brightestPixel.Y + spotSize / 2 &&
                p.Y > brightestPixel.Y - spotSize / 2) {

                ave.X += p.X;
                ave.Y += p.Y;
                ave.Luma += p.Luma;
                ave.RGB += p.RGB;

                outTexture.SetPixel(p.X, p.Y, new Color(p.Luma*0.5f, p.Luma*0.5f, p.Luma));

                count++;
            }
        }
        if (count > 0)
        {
            ave.X /= count;
            ave.Y /= count;
            ave.Luma /= count;
            ave.RGB /= count;
            int avePRad = 4;
            for (int w = -avePRad; w < avePRad; w++)
            {
                for (int h = -avePRad; h < avePRad; h++)
                {
                    if (w == -avePRad || w == avePRad - 1 || h == -avePRad || h == avePRad - 1)
                    {
                        // Outline the spot in black.
                        outTexture.SetPixel(ave.X + w, ave.Y + h, Color.black);
                    }
                    else
                    {
                        //outTexture.SetPixel(ave.X + w, ave.Y + h, new Color(ave.RGB.x, ave.RGB.y, ave.RGB.z));
                    }
                }
            }
        }

        outTexture.SetPixel(brightestPixel.X, brightestPixel.Y, Color.green);
        outTexture.SetPixel(ave.X, ave.Y, Color.black);
        outTexture.Apply();

        outPixel = ave;
        return count > 0;
    }

    /// <summary>
    /// Code from ARGUIController.
    /// Wait for the next depth update, then find the plane at the touch position.
    /// </summary>
    /// <param name="touchPosition">Touch position to find a plane at.</param>
    private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition, ColorPoint p)
    {
        //MessageManager._MessageManager.PushMessage("Screen: " + Screen.width + " " + Screen.height);
        //MessageManager._MessageManager.PushMessage("w" + _OutTexture.width + " h" + _OutTexture.height);
        //MessageManager._MessageManager.PushMessage("x" + touchPosition.x +
        //    " y" + touchPosition.y);

        m_findPlaneWaitingForDepth = true;
        // Turn on the camera and wait for a single depth update.

        if (!_tangoApplication.m_enable3DReconstruction)
            _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.MAXIMUM);
        while (m_findPlaneWaitingForDepth)
        {
            yield return null;
        }
        if (!_tangoApplication.m_enable3DReconstruction)
            _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
        
        int idx = _PointCloud.FindClosestPoint(_MainCam, touchPosition, 200);
        if (idx == -1)
        {
            MessageManager._MessageManager.PushMessage("Could not create light source. No depth detected.");
            yield break;
        }
        Vector3 point = _PointCloud.m_points[idx];
        GameObject newGO =
            Instantiate(_LightPlaceholder.gameObject, point, Quaternion.identity) as GameObject;
        newGO.transform.GetComponent<Renderer>().material.color = new Color(p.RGB.x, p.RGB.y, p.RGB.z);

        _foundLightsPlaceholder.Add(newGO);
        _foundLights.Add(new ColorPoint(p.X, p.Y, p.Luma, p.RGB, point));

    }

    #region TangoEvents

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _lastImageBuffer = imageBuffer;
        if (_Continuous)
        {
            float aveLuma = TangoImageProcessing.AverageLuminescence(imageBuffer);
            _SliderThreshold.value = aveLuma + (1 - aveLuma) / 1.25f;
            Debug.Log(_Threshold);
            ColorPoint p;
            if (FindBrightSpots(_lastImageBuffer, ref _OutTexture, _Threshold, _SpotSize, out p))
            {
                // Don't do anything.
            }
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
#if UNITY_EDITOR
        _OutTexture = new Texture2D(1280 / 8, 720 / 8, TextureFormat.ARGB32, false);
#else
                TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
                VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
                _OutTexture = new Texture2D((int)(intrinsics.width / 8), (int)(intrinsics.height / 8), TextureFormat.RGBA32, false);
#endif
        _OutTexture.filterMode = FilterMode.Point;
    }

    public void OnTangoServiceConnected()
    {
        _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    public void OnTangoServiceDisconnected()
    {
    }

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        // Don't handle depth here because the PointCloud may not have been updated yet.  Just
        // tell the coroutine it can continue.
        m_findPlaneWaitingForDepth = false;
    }

    #endregion

}
