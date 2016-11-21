using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;
using System.Collections.Generic;
using System.Threading;
using System;

public class FindLightAdvManager : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    public enum SuperpixelMethod { SLIC, CWATERSHED, NONE };
    public SuperpixelMethod _CurSuperpixelMethod = SuperpixelMethod.SLIC;

    public Transform _PanelOptionsSLIC;
    public Transform _PanelOptionsWatershed;

    public Toggle _ToggleRealtime;
    public Toggle _ToggleSLIC;
    public Toggle _ToggleCWatershed;
    public Toggle _ToggleDebugLights;

    public Slider _SliderClusterCount;
    public Slider _SliderResDiv;
    public Slider _SliderMaxIterations;
    public Slider _SliderBorderThreshold;
    public Slider _SliderCompactness;
    public Slider _SliderDebugLightX;
    public Slider _SliderDebugLightY;
    public Slider _SliderDebugLightZ;
    public Slider _SliderDebugLightXo;
    public Slider _SliderDebugLightYo;
    public Slider _SliderDebugLightZo;

    private TangoApplication _tangoApplication;

    public RenderTexture _InTexture;
    public Texture2D _IoTexture; // Original intensity map. Used for visually comparing Io and Ir (reconstructed intensity) map.
    public Texture2D _OutTexture;
    private Texture2D tempTex;

    private TangoUnityImageData _lastImageBuffer = null;

    public WatershedSegmentation _Watershed;
    public SLICSegmentation _SLIC;
    private FindLightAdvanced _FLA;

    public int _ClusterCount = 32;
    private int _maxIterations = 1;
    public int _ResDiv = 16;
    public float _ErrorThreshold = 0.001f;
    public int _BorderThreshold = 6;
    public int _Compactness = 10;

    private static int _rSliderClusterCountMax = 128;
    private static int _sliderClusterCountMax = 400;
    private static int _rSliderResLevelMax = 9;
    private static int _sliderResLevelMax = 11;
    private VectorInt3 _debugLightPos = new VectorInt3(0, 0, 0);

    private bool _debuggingLightPos = false;
    private bool _realtime = false;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();

    public Material _ResultMat;
    public Material _IoMat;
    public Transform _DebugLightOriginal;
    public Transform _DebugPointLight;
    public Transform _DebugLightReceiver;

    public void Start()
    {
        _ToggleRealtime.onValueChanged.AddListener(value => onValueChangedRealtime(value));
        _ToggleSLIC.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.SLIC));
        _ToggleCWatershed.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.CWATERSHED));
        _ToggleDebugLights.onValueChanged.AddListener(value => onValueChangedDebugLights(value));

        _SliderResDiv.onValueChanged.AddListener(onValueChangedResDiv);
        _SliderClusterCount.onValueChanged.AddListener(onValueChangedClusterCount);
        _SliderMaxIterations.onValueChanged.AddListener(onValueChangedMaxIterations);
        _SliderBorderThreshold.onValueChanged.AddListener(onValueChangedBorderThreshold);
        _SliderCompactness.onValueChanged.AddListener(onValueChangedCompactness);
        _SliderDebugLightX.onValueChanged.AddListener(onValueChangedDebugLightX);
        _SliderDebugLightY.onValueChanged.AddListener(onValueChangedDebugLightY);
        _SliderDebugLightZ.onValueChanged.AddListener(onValueChangedDebugLightZ);
        _SliderDebugLightXo.onValueChanged.AddListener(onValueChangedDebugLightXo);
        _SliderDebugLightYo.onValueChanged.AddListener(onValueChangedDebugLightYo);
        _SliderDebugLightZo.onValueChanged.AddListener(onValueChangedDebugLightZo);

        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        for (int i = 0; i < _ClusterCount; i++)
        {
            _regionColors.Add(i, ImageProcessing.RandomColor());
        }

        tempTex = new Texture2D(1280 / _ResDiv, 720 / _ResDiv);
        tempTex.filterMode = FilterMode.Point;
        tempTex.mipMapBias = 0;

        _IoTexture = new Texture2D(1280 / _ResDiv, 720 / _ResDiv);
        _IoTexture.filterMode = FilterMode.Point;
        _IoTexture.mipMapBias = 0;

        _Watershed = new WatershedSegmentation();
        _Watershed._ClusterCount = _ClusterCount;
        _Watershed._BorderThreshold = _BorderThreshold;

        _SLIC = new SLICSegmentation();
        _SLIC.MaxIterations = _maxIterations;
        _SLIC.ResidualErrorThreshold = _ErrorThreshold;
        _SLIC.Compactness = _Compactness;

        _FLA = GetComponent<FindLightAdvanced>();
        _FLA.SetupLightErrorGrid();

        _SliderResDiv.maxValue = _sliderResLevelMax;
        _SliderClusterCount.maxValue = _sliderClusterCountMax;
    }

    private void onValueChangedDebugLightXo(float value)
    {
        //float x = _DebugLightOriginal.position.x;
        float y = _DebugLightOriginal.position.y;
        float z = _DebugLightOriginal.position.z;
        _DebugLightOriginal.position = new Vector3(value - _FLA._LightErrorGrid.GetLength(0)/2f,y,z);
    }

    private void onValueChangedDebugLightYo(float value)
    {
        float x = _DebugLightOriginal.position.x;
        //float y = _DebugLightOriginal.position.y;
        float z = _DebugLightOriginal.position.z;
        _DebugLightOriginal.position = new Vector3(x, value - _FLA._LightErrorGrid.GetLength(1) / 2f, z);
    }

    private void onValueChangedDebugLightZo(float value)
    {
        float x = _DebugLightOriginal.position.x;
        float y = _DebugLightOriginal.position.y;
        //float z = _DebugLightOriginal.position.z;
        _DebugLightOriginal.position = new Vector3(x,y,value - _FLA._LightErrorGrid.GetLength(1) / 2f);
    }


    private void onValueChangedDebugLightX(float value)
    {
        _debugLightPos.X = (int)value;
    }

    private void onValueChangedDebugLightY(float value)
    {
        _debugLightPos.Y = (int)value;
    }

    private void onValueChangedDebugLightZ(float value)
    {
        _debugLightPos.Z = (int)value;
    }

    private void onValueChangedDebugLights(bool value)
    {
        _debuggingLightPos = value;
    }

    private void Update()
    {
        if (Input.touchCount == 1)
        {
            Touch t = Input.GetTouch(0);
            if (t.phase != TouchPhase.Began || UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(t.fingerId))
            {
                return;
            }
            if (!_realtime)
            {
                StartCoroutine(superpixelSegmentationRoutine());
            }
        }
        else if (Input.GetMouseButtonDown(1))
        {
            if (!_realtime)
            {
                StartCoroutine(superpixelSegmentationRoutine());

            }
        }

    }

    #region UI Events

    private void onValueChangedRealtime(bool value)
    {
        _realtime = value;
        if (!_realtime)
        {
            MessageManager._MessageManager.PushMessage("Tap screen to render superpixels.", 3);
            _SliderResDiv.maxValue = _sliderResLevelMax;
            _SliderClusterCount.maxValue = _sliderClusterCountMax;
        }
        else
        {
            _maxIterations = 1;
            _SliderResDiv.maxValue = _rSliderResLevelMax;
            _SliderClusterCount.maxValue = _rSliderClusterCountMax;
        }
    }

    private void onValueChangedSuperpixelMethod(bool value, SuperpixelMethod superpixelMethod)
    {
        if (!value)
        {
            _CurSuperpixelMethod = SuperpixelMethod.NONE;
            _PanelOptionsSLIC.gameObject.SetActive(false);
            _PanelOptionsWatershed.gameObject.SetActive(false);
            return;
        }

        _CurSuperpixelMethod = superpixelMethod;

        switch (superpixelMethod)
        {
            case SuperpixelMethod.SLIC:
                _PanelOptionsSLIC.gameObject.SetActive(true);
                _PanelOptionsWatershed.gameObject.SetActive(false);
                break;
            case SuperpixelMethod.CWATERSHED:
                _PanelOptionsSLIC.gameObject.SetActive(false);
                _PanelOptionsWatershed.gameObject.SetActive(true);
                break;
            default:
                break;
        }
    }

    private void onValueChangedResDiv(float value)
    {
        _ResDiv = (int)(8 - value) + 8;

        TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
        VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
        _OutTexture = new Texture2D((int)(intrinsics.width / (float)_ResDiv), (int)(intrinsics.height / (float)_ResDiv), TextureFormat.RGBA32, false);
        _OutTexture.filterMode = FilterMode.Point;
        _OutTexture.anisoLevel = 0;

        tempTex = new Texture2D((int)(1280 / (float)_ResDiv), (int)(720 / (float)_ResDiv));
        tempTex.filterMode = FilterMode.Point;
        tempTex.mipMapBias = 0;

        _IoTexture = new Texture2D((int)(1280 / (float)_ResDiv), (int)(720 / (float)_ResDiv));
        _IoTexture.filterMode = FilterMode.Point;
        _IoTexture.mipMapBias = 0;
    }

    private void onValueChangedClusterCount(float value)
    {
        _ClusterCount = (int)value;
        _SLIC._ClusterCount = _ClusterCount;
        _Watershed._ClusterCount = _ClusterCount;
    }

    private void onValueChangedMaxIterations(float value)
    {
        _maxIterations = (int)value;
        _SLIC.MaxIterations = _maxIterations;
    }

    private void onValueChangedBorderThreshold(float value)
    {
        _BorderThreshold = (int)value;
        _Watershed._BorderThreshold = _BorderThreshold;
    }

    private void onValueChangedCompactness(float value)
    {
        _Compactness = (int)value;
        _SLIC.Compactness = _Compactness;
    }

    #endregion

    private void drawSuperPixels(ref List<Superpixel> superpixels)
    {
        foreach (Superpixel s in superpixels)
        {

            if (!_regionColors.ContainsKey(s.Label))
            {
                _regionColors.Add(s.Label, ImageProcessing.RandomColor());
            }

            float ns = 0;
            float albedo = 0;
            float Ir = 0;
            Vector3 lightDir = ImageProcessing.LightDirection(_FLA._EstimatedLightPos, s.WorldPoint);

            ImageProcessing.ComputeAlbedo(s.Intensity / 255f, s.Normal, lightDir, out ns, out albedo);
            if (ns > 0)
            {
                ImageProcessing.ComputeImageIntensity(albedo, s.Normal, lightDir, out Ir);
            }
            else
            {
                if (!s.GetMedianSynthesizedIr(_OutTexture.width, _OutTexture.height, _FLA._EstimatedLightPos, out albedo, out Ir))
                {
                    Ir = 0;
                }
            }

            foreach (RegionPixel p in s.Pixels)
            {
                //_OutTexture.SetPixel(p.X, _OutTexture.height - p.Y, _regionColors[s.Label]);
                _OutTexture.SetPixel(p.X, _OutTexture.height - p.Y, new Color(Ir, Ir, Ir));

                _IoTexture.SetPixel(p.X, _OutTexture.height - p.Y, new Color(s.Intensity / 255f, s.Intensity / 255f, s.Intensity / 255f));
            }

        }

        _OutTexture.Apply();
        _IoTexture.Apply();
    }

    private void drawRegionPixels(ref List<RegionPixel> rpixels)
    {
        foreach (RegionPixel r in rpixels)
        {
            Color oC = new Color();
            Color iC = new Color();

            float ns = 0;
            float albedo = 0;
            float Ir = 0;
            Vector3 lightDir = ImageProcessing.LightDirection(_FLA._EstimatedLightPos, r.WorldPoint);

            ImageProcessing.ComputeAlbedo(r.Intensity / 255f, r.Normal, lightDir, out ns, out albedo);
            if (ns > 0)
            {
                if (albedo > 2.5)
                {
                    oC = Color.red;
                    iC = Color.red;
                }
                else
                {
                    ImageProcessing.ComputeImageIntensity(albedo, r.Normal, lightDir, out Ir);
                    oC = new Color(Ir, Ir, Ir);
                    iC = new Color(albedo, albedo, albedo);
                }
            }
            else
            {
                oC = new Color(Ir, Ir, Ir);
                iC = Color.cyan;
            }

            _IoTexture.SetPixel(r.X, _OutTexture.height - r.Y, iC);
            _OutTexture.SetPixel(r.X, _OutTexture.height - r.Y, oC);
        }

        _OutTexture.Apply();
        _IoTexture.Apply();
    }


    private void doCompactWatershed()
    {
        //Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        Vector3[,] pixels = ImageProcessing.RenderTextureToRGBArray(_InTexture);

        //pixels = ImageProcessing.MedianFilter3x3(ref pixels);
        List<Superpixel> superpixels;
        int[,] S = _Watershed.Run(pixels, out superpixels);

        foreach (Superpixel s in superpixels)
        {
            s.ComputeImageIntensity();
            s.ComputeSurfaceNormal(_OutTexture.width, _OutTexture.height);
        }

        if (_debuggingLightPos)
        {
            Vector3 lightPos = Camera.main.transform.TransformPoint(
                _debugLightPos.X - _FLA._LightErrorGrid.GetLength(0) / 2f,
                _debugLightPos.Y - _FLA._LightErrorGrid.GetLength(1) / 2f,
                -_debugLightPos.Z);
            _FLA._EstimatedLightPos = lightPos;

            float[] Io = new float[superpixels.Count];
            for (int i = 0; i < superpixels.Count; i++)
            {
                Io[i] = superpixels[i].Intensity;
            }

            float error = FindLightAdvanced.IoIrL2Norm(ref superpixels, Io, lightPos, _OutTexture.width, _OutTexture.height);
            Debug.Log("LightPos: " + lightPos + " error: " + error);
        }
        else
        {
            _FLA._EstimatedLightPos = _FLA.LightEstimation(ref superpixels, _OutTexture.width, _OutTexture.height);
        }


        //Debug.DrawRay(_estimatedLightPos, Camera.main.transform.position - _estimatedLightPos, Color.magenta);
        _DebugPointLight.transform.position = _FLA._EstimatedLightPos;
        _DebugPointLight.transform.LookAt(_DebugLightReceiver);

        drawSuperPixels(ref superpixels);
        _ResultMat.mainTexture = _OutTexture;
        _IoMat.mainTexture = _IoTexture;
    }

    private void doSLIC()
    {
        //Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        Vector3[,] pixels = ImageProcessing.RenderTextureToRGBArray(_InTexture);

        pixels = ImageProcessing.MedianFilter3x3(ref pixels);
        List<Superpixel> superpixels;
        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(pixels, out superpixels);

        foreach (Superpixel s in superpixels)
        {
            s.ComputeImageIntensity();
            s.ComputeSurfaceNormal(_OutTexture.width, _OutTexture.height);
        }

        if (_debuggingLightPos)
        {
            Vector3 lightPos = Camera.main.transform.TransformPoint(
                _debugLightPos.X - _FLA._LightErrorGrid.GetLength(0) / 2f,
                _debugLightPos.Y - _FLA._LightErrorGrid.GetLength(1) / 2f,
                -_debugLightPos.Z);
            _FLA._EstimatedLightPos = lightPos;

            float[] Io = new float[superpixels.Count];
            for (int i = 0; i < superpixels.Count; i++)
            {
                Io[i] = superpixels[i].Intensity / 255f;
            }

            float error = FindLightAdvanced.IoIrL2Norm(ref superpixels, Io, lightPos, _OutTexture.width, _OutTexture.height);
            Debug.Log("LightPos: " + lightPos + " error: " + error);
        }
        else
        {
            _FLA._EstimatedLightPos = _FLA.LightEstimation(ref superpixels, _OutTexture.width, _OutTexture.height);
        }
        //Debug.DrawRay(_estimatedLightPos, Camera.main.transform.position - _estimatedLightPos, Color.magenta);
        _DebugPointLight.transform.position = _FLA._EstimatedLightPos;
        _DebugPointLight.transform.LookAt(_DebugLightReceiver);

        drawSuperPixels(ref superpixels);
        _ResultMat.mainTexture = _OutTexture;
        _IoMat.mainTexture = _IoTexture;
    }

    private void doLightEstimation()
    {
        //Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        Vector3[,] pixels = ImageProcessing.RenderTextureToRGBArray(_InTexture);

        //pixels = ImageProcessing.MedianFilter3x3(ref pixels);
        //float[,] edges = ImageProcessing.SobelFilter3x3(ref pixels, true);

        List<RegionPixel> rpixels = RegionPixel.ToRegionPixels(pixels);
        foreach (RegionPixel r in rpixels)
        {
            r.ComputeImageIntensity();
            r.ComputeSurfaceNormal(_OutTexture.width, _OutTexture.height);
        }

        if (_debuggingLightPos)
        {

            Vector3 lightPos = Camera.main.transform.TransformPoint(
                _debugLightPos.X - _FLA._LightErrorGrid.GetLength(0) / 2f,
                _debugLightPos.Y - _FLA._LightErrorGrid.GetLength(1) / 2f,
                -_debugLightPos.Z);

            _FLA._EstimatedLightPos = lightPos;

            float[] Io = new float[rpixels.Count];
            for (int i = 0; i < rpixels.Count; i++)
            {
                Io[i] = rpixels[i].Intensity / 255f;
            }

            float error = FindLightAdvanced.IoIrL2Norm(ref rpixels, Io, lightPos, _OutTexture.width, _OutTexture.height);
            Debug.Log("LightPos: " + lightPos + " error: " + error);
        }
        else
        {
            _FLA._EstimatedLightPos = _FLA.LightEstimation(ref rpixels, _OutTexture.width, _OutTexture.height);
        }
        //Debug.DrawRay(_estimatedLightPos, Camera.main.transform.position - _estimatedLightPos, Color.magenta);
        _DebugPointLight.transform.position = _FLA._EstimatedLightPos;
        _DebugPointLight.transform.LookAt(_DebugLightReceiver);

        drawRegionPixels(ref rpixels);
        _ResultMat.mainTexture = _OutTexture;
        _IoMat.mainTexture = _IoTexture;
    }

    private void superpixelSegmentation()
    {

        switch (_CurSuperpixelMethod)
        {
            case SuperpixelMethod.SLIC:
                doSLIC();
                break;
            case SuperpixelMethod.CWATERSHED:
                doCompactWatershed();
                break;
            case SuperpixelMethod.NONE:
                doLightEstimation();
                break;
            default:
                break;
        }
    }

    private IEnumerator superpixelSegmentationRoutine()
    {
        MessageManager._MessageManager.PushMessage("Performing Superpixel Segmentation ...");
        yield return null;
        superpixelSegmentation();
    }

    #region Tango Events

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _lastImageBuffer = imageBuffer;

        if (_realtime)
        {
            superpixelSegmentation();
        }

    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
#if UNITY_EDITOR
        _OutTexture = new Texture2D(1280 / _ResDiv, 720 / _ResDiv, TextureFormat.ARGB32, false);
        _OutTexture.filterMode = FilterMode.Point;
        _OutTexture.anisoLevel = 0;
#else
        TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
        VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
        _OutTexture = new Texture2D((int)(intrinsics.width / _ResDiv), (int)(intrinsics.height / _ResDiv), TextureFormat.RGBA32, false);
        _OutTexture.filterMode = FilterMode.Point;
        _OutTexture.anisoLevel = 0;
#endif

        Debug.Log(_OutTexture.width + "x" + _OutTexture.height);
    }

    public void OnTangoServiceConnected()
    {
        //_tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    public void OnTangoServiceDisconnected()
    {
    }

    #endregion

}
