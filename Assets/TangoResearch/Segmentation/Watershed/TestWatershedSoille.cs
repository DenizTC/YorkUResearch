using UnityEngine;
using System.Collections;
using Tango;
using System.Collections.Generic;
using Segmentation;

public class TestWatershedSoille : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    private TangoApplication _tangoApplication;

    public Texture2D _OutTexture;

    private TangoUnityImageData _lastImageBuffer = null;

    public WatershedGrayscale _Watershed;

    public int _ResDiv = 32;

    public int _ClusterCount = 32;

    public int _BorderThreshold = 128;

    public int _MaxIterations = 10;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();

    public Material _ResultMat;

    public void Start()
    {
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

        _Watershed = new WatershedGrayscale(4);
        //_Watershed._ClusterCount = _ClusterCount;
        //_Watershed._ResDiv = (uint)_ResDiv;
        //_Watershed._BorderThreshold = _BorderThreshold;
    }

    public static Color RandomColor()
    {
        Color col = new Color((float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble());
        return col;
    }

    Texture2D tempTex;
    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _lastImageBuffer = imageBuffer;

        Vector3[,] pixels = TangoHelpers.ImageBufferToArray(imageBuffer, (uint)_ResDiv, true);
        //int[,] S = _Watershed.Run(pixels);

        _Watershed.ProcessFilter(ref pixels);

        for (int i = 0; i < _OutTexture.width; i++)
        {
            for (int j = 0; j < _OutTexture.height; j++)
            {
                _OutTexture.SetPixel(i, _OutTexture.height - j, TangoHelpers.Vector3ToColor(pixels[i, j]) / 255f);
                //_OutTexture.SetPixel(i, _OutTexture.height - j, TangoHelpers.Vector3ToColor(new Vector3(S[i, j], S[i, j], S[i, j])) / 255f);
                //if (S[i, j] == -1)
                //{
                //    _OutTexture.SetPixel(i, _OutTexture.height - j, Color.red);
                //}
                //else
                //{
                //    _OutTexture.SetPixel(i, _OutTexture.height - j, TangoHelpers.Vector3ToColor(pixels[i, j]) / 255f);
                //}
            }
        }

        _OutTexture.Apply();
        _ResultMat.mainTexture = _OutTexture;
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
