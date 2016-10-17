using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;
using System.Collections.Generic;

public class SuperpixelManager : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    public enum SuperpixelMethod { SLIC, CWATERSHED };
    public enum DisplayClusterMode { BORDER, MOSAIC };
    public SuperpixelMethod _CurSuperpixelMethod = SuperpixelMethod.SLIC;
    public DisplayClusterMode _CurDisplayClusterMode = DisplayClusterMode.BORDER;

    public Transform _PanelOptionsSLIC;
    public Transform _PanelOptionsWatershed;

    public Toggle _ToggleSLIC;
    public Toggle _ToggleCWatershed;
    public Toggle _ToggleDMBorder;
    public Toggle _ToggleDMMosaic;

    public Slider _SliderClusterCount;
    public Slider _SliderResDiv;
    public Slider _SliderBorderThreshold;
    public Slider _SliderCompactness;

    private TangoApplication _tangoApplication;

    public Texture2D _OutTexture;
    private Texture2D tempTex;

    private TangoUnityImageData _lastImageBuffer = null;

    public WatershedSegmentation _Watershed;
    public SLICSegmentation _SLIC;

    public int _ClusterCount = 32;
    public int _MaxIterations = 1;
    public int _ResDiv = 16;
    public float _ErrorThreshold = 0.001f;
    public int _BorderThreshold = 6;
    public int _Compactness = 10;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();

    public Material _ResultMat;

    public void Start()
    {
        _ToggleSLIC.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.SLIC));
        _ToggleCWatershed.onValueChanged.AddListener(value => onValueChangedSuperpixelMethod(value, SuperpixelMethod.CWATERSHED));
        _ToggleDMBorder.onValueChanged.AddListener(value => onValueChangedDisplayClusterMode(value, DisplayClusterMode.BORDER));
        _ToggleDMMosaic.onValueChanged.AddListener(value => onValueChangedDisplayClusterMode(value, DisplayClusterMode.MOSAIC));


        _SliderResDiv.onValueChanged.AddListener(onValueChangedResDiv);
        _SliderClusterCount.onValueChanged.AddListener(onValueChangedClusterCount);
        _SliderBorderThreshold.onValueChanged.AddListener(onValueChangedBorderThreshold);
        _SliderCompactness.onValueChanged.AddListener(onValueChangedCompactness);

        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        for (int i = 0; i < _ClusterCount; i++)
        {
            _regionColors.Add(i, RandomColor());
        }

        tempTex = new Texture2D(1280 / _ResDiv, 720 / _ResDiv);
        tempTex.filterMode = FilterMode.Point;
        tempTex.mipMapBias = 0;

        _Watershed = new WatershedSegmentation();
        _Watershed._ClusterCount = _ClusterCount;
        _Watershed._ResDiv = (uint)_ResDiv;
        _Watershed._BorderThreshold = _BorderThreshold;

        _SLIC = new SLICSegmentation();
        _SLIC.MaxIterations = _MaxIterations;
        _SLIC.ResidualErrorThreshold = _ErrorThreshold;
        _SLIC.ResDiv = _ResDiv;
        _SLIC.Compactness = _Compactness;

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
        _SLIC.ResDiv = _ResDiv;
        _Watershed._ResDiv = (uint)_ResDiv;

        TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
        VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
        _OutTexture = new Texture2D((int)(intrinsics.width / _ResDiv), (int)(intrinsics.height / _ResDiv), TextureFormat.RGBA32, false);
        _OutTexture.filterMode = FilterMode.Point;
        _OutTexture.anisoLevel = 0;

        tempTex = new Texture2D(1280 / _ResDiv, 720 / _ResDiv);
        tempTex.filterMode = FilterMode.Point;
        tempTex.mipMapBias = 0;
    }

    private void onValueChangedClusterCount(float value)
    {
        _ClusterCount = (int)value;
        _SLIC._ClusterCount = _ClusterCount;
        _Watershed._ClusterCount = _ClusterCount;
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

    public static Color RandomColor()
    {
        Color col = new Color((float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble());
        return col;
    }

    private void doCompactWatershed()
    {
        Vector3[,] pixels = TangoHelpers.ImageBufferToArray(_lastImageBuffer, (uint)_ResDiv, true);
        int[,] S = _Watershed.Run(pixels);

        for (int i = 0; i < _OutTexture.width; i++)
        {
            for (int j = 0; j < _OutTexture.height; j++)
            {
                if (!_regionColors.ContainsKey(-S[i, j]))
                {
                    _regionColors.Add(-S[i, j], RandomColor());
                }

                if (_CurDisplayClusterMode == DisplayClusterMode.BORDER)
                {
                    if (S[i, j] == -1)
                    {
                        _OutTexture.SetPixel(i, _OutTexture.height - j, Color.red);
                    }
                    else
                    {
                        _OutTexture.SetPixel(i, _OutTexture.height - j, TangoHelpers.Vector3ToColor(pixels[i, j]) / 255f);
                    }
                }
                else
                {
                    if (S[i, j] < -1)
                    {
                        _OutTexture.SetPixel(i, _OutTexture.height - j, _regionColors[-S[i, j]]);
                    }
                    else
                    {
                        _OutTexture.SetPixel(i, _OutTexture.height - j, TangoHelpers.Vector3ToColor(pixels[i, j]) / 255f);
                    }
                }

            }
        }

        _OutTexture.Apply();
        _ResultMat.mainTexture = _OutTexture;
    }

    private void doSLIC()
    {
        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(_lastImageBuffer);

        int count = 0;
        foreach (CIELABXYCenter c in clusterCenters)
        {
            foreach (CIELABXY cR in c.Region)
            {
                if (!_regionColors.ContainsKey(count))
                {
                    _regionColors.Add(count, RandomColor());
                }

                if (_CurDisplayClusterMode == DisplayClusterMode.MOSAIC)
                {
                    _OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, _regionColors[count]);
                }
                else
                {
                    _OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(cR.RGB.x, cR.RGB.y, cR.RGB.z));
                    tempTex.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, _regionColors[count]);
                }
            }
            count++;
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
        _ResultMat.mainTexture = _OutTexture;
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _lastImageBuffer = imageBuffer;

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
        _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    public void OnTangoServiceDisconnected()
    {
    }
}
