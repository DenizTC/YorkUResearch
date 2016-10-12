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

    public int _ResDiv = 32;

    public int _ClusterCount = 100;

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
    }

    public static Color RandomColor()
    {
        Color col = new Color((float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble());
        return col;
    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        _lastImageBuffer = imageBuffer;

        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(_lastImageBuffer, 2, _ResDiv, 100);

        int count = 0;
        float weight = 0.6f;
        foreach (CIELABXYCenter c in clusterCenters)
        {

            //CIELABXYCenter ave = SLICSegmentation.GetAverage(c.Region);
            foreach (CIELABXY cR in c.Region)
            {
                if (!_regionColors.ContainsKey(count))
                {
                    _regionColors.Add(count, RandomColor());
                }



                _OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv,
                    new Color(cR.L * weight + _regionColors[count].r * (1 - weight), 
                    cR.A * weight + _regionColors[count].g * (1 - weight), 
                    cR.B * weight + _regionColors[count].b * (1 - weight)));
                //_OutTexture.SetPixel(cR.X / 8, _OutTexture.height - cR.Y / 8, new Color(cR.L, cR.A, cR.B));
                //_OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(c.L, c.A, c.B));


            }
            //_OutTexture.SetPixel(c.X/ _ResDiv, _OutTexture.height - c.Y/ _ResDiv, new Color(1,0,0));
            count++;
        }

        //float residualError = _SLIC.ComputeNewClusterCenters(ref clusterCenters);
        ////Debug.Log("ResidualError: " + residualError);
        //foreach (CIELABXYCenter c in clusterCenters)
        //{
        //    _OutTexture.SetPixel(c.X / 8, _OutTexture.height - c.Y / 8, new Color(0, 1, 0));
        //}

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
    }

    public void OnTangoServiceConnected()
    {
        _tangoApplication.SetDepthCameraRate(TangoEnums.TangoDepthCameraRate.DISABLED);
    }

    public void OnTangoServiceDisconnected()
    {
    }
}
