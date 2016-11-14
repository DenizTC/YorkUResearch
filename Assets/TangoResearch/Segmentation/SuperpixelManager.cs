using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;
using System.Collections.Generic;
using System.Threading;
using System;

public class SuperpixelManager : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    public enum SuperpixelMethod { SLIC, CWATERSHED };
    public enum DisplayClusterMode { BORDER, MOSAIC, SUPERPIXEL_MEAN };
    public SuperpixelMethod _CurSuperpixelMethod = SuperpixelMethod.SLIC;
    public DisplayClusterMode _CurDisplayClusterMode = DisplayClusterMode.BORDER;

    public Transform _PanelOptionsSLIC;
    public Transform _PanelOptionsWatershed;

    public Toggle _ToggleRealtime;
    public Toggle _ToggleSLIC;
    public Toggle _ToggleCWatershed;
    public Toggle _ToggleDMBorder;
    public Toggle _ToggleDMMosaic;
    public Toggle _ToggleDMSuperpixelMean;
    public Toggle _ToggleMerger;
    public Toggle _ToggleDebugLights;

    public Slider _SliderClusterCount;
    public Slider _SliderResDiv;
    public Slider _SliderMaxIterations;
    public Slider _SliderBorderThreshold;
    public Slider _SliderCompactness;
    public Slider _SliderTd;
    public Slider _SliderTc;
    public Slider _SliderDebugLightX;
    public Slider _SliderDebugLightY;
    public Slider _SliderDebugLightZ;

    private TangoApplication _tangoApplication;

    public RenderTexture _InTexture;
    public Texture2D _IoTexture; // Original intensity map. Used for visually comparing Io and Ir (reconstructed intensity) map.
    public Texture2D _OutTexture;
    private Texture2D tempTex;

    private TangoUnityImageData _lastImageBuffer = null;

    public WatershedSegmentation _Watershed;
    public SLICSegmentation _SLIC;
    private SuperpixelMerger _Merger;

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
    private bool _mergeSuperpixels = false;
    private float _Td = 64;
    private float _Tc = 32;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();

    public Material _ResultMat;
    public Material _IoMat;
    public Transform _DebugPointLight;
    public Transform _DebugLightReceiver;

    public void Start()
    {
        _ToggleRealtime.onValueChanged.AddListener(value => onValueChangedRealtime(value));
        _ToggleSLIC.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.SLIC));
        _ToggleCWatershed.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.CWATERSHED));
        _ToggleDMBorder.onValueChanged.AddListener(value => onValueChangedDisplayClusterMode(value, DisplayClusterMode.BORDER));
        _ToggleDMMosaic.onValueChanged.AddListener(value => onValueChangedDisplayClusterMode(value, DisplayClusterMode.MOSAIC));
        _ToggleDMSuperpixelMean.onValueChanged.AddListener(value => onValueChangedDisplayClusterMode(value, DisplayClusterMode.SUPERPIXEL_MEAN));
        _ToggleMerger.onValueChanged.AddListener(value => onValueChangedMerger(value));
        _ToggleDebugLights.onValueChanged.AddListener(value => onValueChangedDebugLights(value));

        _SliderResDiv.onValueChanged.AddListener(onValueChangedResDiv);
        _SliderClusterCount.onValueChanged.AddListener(onValueChangedClusterCount);
        _SliderMaxIterations.onValueChanged.AddListener(onValueChangedMaxIterations);
        _SliderBorderThreshold.onValueChanged.AddListener(onValueChangedBorderThreshold);
        _SliderCompactness.onValueChanged.AddListener(onValueChangedCompactness);
        _SliderTd.onValueChanged.AddListener(onValueChangedTd);
        _SliderTc.onValueChanged.AddListener(onValueChangedTc);
        _SliderDebugLightX.onValueChanged.AddListener(onValueChangedDebugLightX);
        _SliderDebugLightY.onValueChanged.AddListener(onValueChangedDebugLightY);
        _SliderDebugLightZ.onValueChanged.AddListener(onValueChangedDebugLightZ);

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

        _Merger = GetComponent<SuperpixelMerger>();
        setupLightErrorGrid();

        _SliderResDiv.maxValue = _sliderResLevelMax;
        _SliderClusterCount.maxValue = _sliderClusterCountMax;
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

    private void onValueChangedTc(float value)
    {
        _Tc = value;
        _Merger.Tc = _Tc;
    }

    private void onValueChangedTd(float value)
    {
        _Td = value;
        _Merger.Td = _Td;
    }

    private void onValueChangedMerger(bool value)
    {
        _mergeSuperpixels = value;
        _SliderTd.interactable = _mergeSuperpixels;
        _SliderTc.interactable = _mergeSuperpixels;
    }

    private void onValueChangedSuperpixelMethod(bool value, SuperpixelMethod superpixelMethod)
    {
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

    private void onValueChangedDisplayClusterMode(bool value, DisplayClusterMode displayClusterMode)
    {
        _CurDisplayClusterMode = displayClusterMode;
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


    public float[,,] _lightErrorGrid;

    private void setupLightErrorGrid(int size = 5)
    {
        _lightErrorGrid = new float[size, size, size];
    }

    private Vector3 lightEstimation(ref List<Superpixel> superpixels)
    {
        VectorInt3 minError = new VectorInt3(0, 0, 0);
        float[] Io = new float[superpixels.Count];

        for (int i = 0; i < superpixels.Count; i++)
        {
            Io[i] = superpixels[i].Intensity/255f;
        }

        for (int x = 0; x < _lightErrorGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _lightErrorGrid.GetLength(1); y++)
            {
                for (int z = 0; z < _lightErrorGrid.GetLength(2); z++)
                {
                    
                    Vector3 lightPos = Camera.main.transform.position + 
                        new Vector3(x - _lightErrorGrid.GetLength(0)/2f, 
                        y - _lightErrorGrid.GetLength(1) / 2f, 
                        z - _lightErrorGrid.GetLength(2) / 2f);
                    
                    float error = IoIrL2Norm(ref superpixels, Io, lightPos);
                    _lightErrorGrid[x, y, z] = error;

                    if (error < _lightErrorGrid[minError.X, minError.Y, minError.Z])
                    {
                        minError = new VectorInt3(x, y, z);
                    }

                    
                }
            }
        }

        //Debug.Log("MinError " + minError.X + "x"+ minError.Y + "x" + minError.Z + ": " + _lightErrorGrid[minError.X, minError.Y, minError.Z]);

        Vector3 estimatedLightPos = Camera.main.transform.position + 
            new Vector3(minError.X - _lightErrorGrid.GetLength(0) / 2f,
            minError.Y - _lightErrorGrid.GetLength(1) / 2f,
            minError.Z - _lightErrorGrid.GetLength(2) / 2f);

        Debug.Log("LightPos: " + estimatedLightPos + " error: " + _lightErrorGrid[minError.X, minError.Y, minError.Z]);
        return estimatedLightPos;
    }

    private float IoIrL2Norm(ref List<Superpixel> superpixels, float[] Io, Vector3 lightPos)
    {
        float[] Ir = new float[superpixels.Count];

        float dist = 0;
        for (int i = 0; i < superpixels.Count; i++)
        {
            if (superpixels[i].Normal.magnitude <= 0 || superpixels[i].Pixels.Count < (_OutTexture.width*_OutTexture.height/(float)_ClusterCount/2f))
            {
                continue;
            }

            float albedo = 0;
            float ir = 0;
            Vector3 lightDir = ImageProcessing.LightDirection(lightPos, superpixels[i].WorldPoint);
            
            if (ImageProcessing.ComputeAlbedo(Io[i], superpixels[i].Normal, lightDir, out albedo))
            {
                ImageProcessing.ComputeImageIntensity(albedo, superpixels[i].Normal, lightDir, out ir);
            }
            else
            {
                if (!superpixels[i].GetMedianSynthesizedIr(_OutTexture.width, _OutTexture.height, lightPos, out albedo, out ir))
                {
                    ir = 0;
                }
            }
            

            Ir[i] = ir;
            dist += Mathf.Pow(Io[i] - Ir[i], 2);
        }
        dist = Mathf.Pow(dist, 0.5f);

        if (_debuggingLightPos)
        {
            Debug.Log("IoIr: " + dist);
        }
        
        return dist;
    }

    private void drawSuperPixels(ref List<Superpixel> superpixels)
    {
        foreach (Superpixel s in superpixels)
        {

            if (!_regionColors.ContainsKey(s.Label))
            {
                _regionColors.Add(s.Label, ImageProcessing.RandomColor());
            }
            if (_CurDisplayClusterMode == DisplayClusterMode.BORDER)
            {
                foreach (RegionPixel p in s.Pixels)
                {
                    _OutTexture.SetPixel(p.X, _OutTexture.height - p.Y, new Color(p.R / 255f, p.G / 255f, p.B / 255f));
                    tempTex.SetPixel(p.X, _OutTexture.height - p.Y, _regionColors[s.Label]);
                }
            }
            else if (_CurDisplayClusterMode == DisplayClusterMode.MOSAIC)
            {
                bool marker = true;
                float albedo = 0;
                float Ir = 0;
                Vector3 lightDir = ImageProcessing.LightDirection(_estimatedLightPos, s.WorldPoint);
                
                if (ImageProcessing.ComputeAlbedo(s.Intensity / 255f, s.Normal, lightDir, out albedo))
                {
                    ImageProcessing.ComputeImageIntensity(albedo, s.Normal, lightDir, out Ir);
                }
                else
                {
                    marker = false;
                    if (!s.GetMedianSynthesizedIr(_OutTexture.width, _OutTexture.height, _estimatedLightPos, out albedo, out Ir))
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
            else // Superpixel Mean
            {
                foreach (RegionPixel p in s.Pixels)
                {
                    _OutTexture.SetPixel(p.X, _OutTexture.height - p.Y, new Color(s.R / 255f, s.G / 255f, s.B / 255f));
                }
            }
        }

        if (_CurDisplayClusterMode == DisplayClusterMode.BORDER)
        {
            for (int i = 0; i < tempTex.width; i++)
            {
                for (int j = 0; j < tempTex.height; j++)
                {
                    if (tempTex.GetPixel(i, j) != tempTex.GetPixel(i - 1, j) ||
                        tempTex.GetPixel(i, j) != tempTex.GetPixel(i, j - 1))
                    {
                        _OutTexture.SetPixel(i, j, Color.red);
                    }
                }
            }
        }

        _OutTexture.Apply();
        _IoTexture.Apply();
    }

    private void doCompactWatershed()
    {
        Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        //Vector3[,] pixels = ImageProcessing.RenderTextureToRGBArray(_InTexture);

        pixels = ImageProcessing.MedianFilter3x3(ref pixels);
        List<Superpixel> superpixels;
        int[,] S = _Watershed.Run(pixels, out superpixels);

        if (_mergeSuperpixels)
        {
            superpixels = _Merger.MergeSuperpixels(superpixels);
        }

        foreach (Superpixel s in superpixels)
        {
            s.ComputeImageIntensity();
            s.ComputeSurfaceNormal(_OutTexture.width, _OutTexture.height);
        }

        if (_debuggingLightPos)
        {
            Vector3 lightPos = Camera.main.transform.position + 
                new Vector3(_debugLightPos.X - _lightErrorGrid.GetLength(0) / 2f,
                _debugLightPos.Y - _lightErrorGrid.GetLength(1) / 2f,
                _debugLightPos.Z - _lightErrorGrid.GetLength(2) / 2f);
            _estimatedLightPos = lightPos;

            float[] Io = new float[superpixels.Count];
            for (int i = 0; i < superpixels.Count; i++)
            {
                Io[i] = superpixels[i].Intensity;
            }

            float error = IoIrL2Norm(ref superpixels, Io, lightPos);
            Debug.Log("LightPos: " + lightPos + " error: " + error);
        }
        else
        {
            _estimatedLightPos = lightEstimation(ref superpixels);
        }
        

        //Debug.DrawRay(_estimatedLightPos, Camera.main.transform.position - _estimatedLightPos, Color.magenta);
        _DebugPointLight.transform.position = _estimatedLightPos;
        _DebugPointLight.transform.LookAt(_DebugLightReceiver);

        drawSuperPixels(ref superpixels);
        _ResultMat.mainTexture = _OutTexture;
        _IoMat.mainTexture = _IoTexture;
    }

    private Vector3 _estimatedLightPos = Vector3.zero;
    private void doSLIC()
    {
        Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        //Vector3[,] pixels = ImageProcessing.RenderTextureToRGBArray(_InTexture);

        pixels = ImageProcessing.MedianFilter3x3(ref pixels);
        List<Superpixel> superpixels;
        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(pixels, out superpixels);

        if (_mergeSuperpixels)
        {
            superpixels = _Merger.MergeSuperpixels(superpixels);
        }

        foreach (Superpixel s in superpixels)
        {
            s.ComputeImageIntensity();
            s.ComputeSurfaceNormal(_OutTexture.width, _OutTexture.height);
        }

        if (_debuggingLightPos)
        {
            Vector3 lightPos = Camera.main.transform.position +
                new Vector3(_debugLightPos.X - _lightErrorGrid.GetLength(0) / 2f,
                _debugLightPos.Y - _lightErrorGrid.GetLength(1) / 2f,
                _debugLightPos.Z - _lightErrorGrid.GetLength(2) / 2f);
            _estimatedLightPos = lightPos;

            float[] Io = new float[superpixels.Count];
            for (int i = 0; i < superpixels.Count; i++)
            {
                Io[i] = superpixels[i].Intensity/255f;
            }

            float error = IoIrL2Norm(ref superpixels, Io, lightPos);
            Debug.Log("LightPos: " + lightPos + " error: " + error);
        }
        else
        {
            _estimatedLightPos = lightEstimation(ref superpixels);
        }
        //Debug.DrawRay(_estimatedLightPos, Camera.main.transform.position - _estimatedLightPos, Color.magenta);
        _DebugPointLight.transform.position = _estimatedLightPos;
        _DebugPointLight.transform.LookAt(_DebugLightReceiver);

        drawSuperPixels(ref superpixels);
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
