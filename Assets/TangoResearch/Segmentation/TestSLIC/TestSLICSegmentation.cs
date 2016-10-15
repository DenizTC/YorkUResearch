using UnityEngine;
using System.Collections;
using Tango;
using System.Collections.Generic;

public class TestSLICSegmentation : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    private TangoApplication _tangoApplication;

    public Texture2D _OutTexture;

    private TangoUnityImageData _lastImageBuffer = null;

    public SLICSegmentation _SLIC;

    public int _ResDiv = 16;

    public int _ClusterCount = 32;

    public int _Compactness = 10;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();
    
    public Material _ResultMat;

    public void Start () {
        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }
        //_OutTexture = new Texture2D(1280 / 8, 720 / 8);

        //_OutTexture.filterMode = FilterMode.Point;
        //_OutTexture.anisoLevel = 0;

        for (int i = 0; i < _ClusterCount; i++)
        {
            _regionColors.Add(i, RandomColor());
        }

        tempTex = new Texture2D(1280 / _ResDiv, 720 / _ResDiv);
        tempTex.filterMode = FilterMode.Point;
        tempTex.mipMapBias = 0;
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

        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(_lastImageBuffer, 2, _ResDiv, _ClusterCount);



        int count = 0;
        float weight = 0f;
        foreach (CIELABXYCenter c in clusterCenters)
        {

            //CIELABXYCenter ave = SLICSegmentation.GetAverage(c.Region);
            foreach (CIELABXY cR in c.Region)
            {
                if (!_regionColors.ContainsKey(count))
                {
                    _regionColors.Add(count, RandomColor());
                }


                _OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(cR.RGB.x, cR.RGB.y, cR.RGB.z));
                tempTex.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(_regionColors[count].r, _regionColors[count].g, _regionColors[count].b));
                //_OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv,
                //    new Color(cR.L * weight + _regionColors[count].r * (1 - weight),
                //              cR.A * weight + _regionColors[count].g * (1 - weight),
                //              cR.B * weight + _regionColors[count].b * (1 - weight)));



                //_OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv,
                //    new Color(_regionColors[count].r * (1 - weight),
                //    _regionColors[count].g * (1 - weight),
                //    _regionColors[count].b * (1 - weight)));
                //_OutTexture.SetPixel(cR.X / 8, _OutTexture.height - cR.Y / 8, new Color(cR.L, cR.A, cR.B));


            }
            //_OutTexture.SetPixel(c.X/ _ResDiv, _OutTexture.height - c.Y/ _ResDiv, new Color(1,0,0));
            count++;
        }

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
