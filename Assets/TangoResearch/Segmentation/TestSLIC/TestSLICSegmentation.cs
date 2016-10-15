using UnityEngine;
using System.Collections;
using Tango;
using System.Collections.Generic;

public class VectorInt2
{
    public int X;
    public int Y;

    public VectorInt2()
    {
    }

    public VectorInt2(int x, int y)
    {
        X = x;
        Y = y;
    }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        VectorInt2 p = (VectorInt2)obj;
        return (X == p.X) && (Y == p.Y);
    }

    public override int GetHashCode()
    {
        return X ^ Y;
        //unchecked // Overflow is fine, just wrap
        //{
        //    int hash = 17;
        //    // Suitable nullity checks etc, of course :)
        //    hash = hash * 23 + X.GetHashCode();
        //    hash = hash * 23 + Y.GetHashCode();
        //    return hash;
        //}
    }
}

public class TestSLICSegmentation : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle
{

    private TangoApplication _tangoApplication;

    public Texture2D _OutTexture;

    private TangoUnityImageData _lastImageBuffer = null;

    public SLICSegmentation _SLIC;

    public int _ResDiv = 16;

    public int _ClusterCount = 32;

    public int _Compactness = 10;

    public float _ErrorThreshold = 0.001f;

    public int _MaxIterations = 10;

    private Dictionary<int, Color> _regionColors = new Dictionary<int, Color>();
    
    public Material _ResultMat;

    public void Start () {
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

        _SLIC = new SLICSegmentation();
        _SLIC.MaxIterations = _MaxIterations;
        _SLIC.ResidualErrorThreshold = _ErrorThreshold;
        _SLIC.ResDiv = _ResDiv;
        _SLIC.Compactness = _Compactness;
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

        List<CIELABXYCenter> clusterCenters = _SLIC.RunSLICSegmentation(_lastImageBuffer, _ClusterCount);

        int count = 0;
        foreach (CIELABXYCenter c in clusterCenters)
        {
            foreach (CIELABXY cR in c.Region)
            {
                if (!_regionColors.ContainsKey(count))
                {
                    _regionColors.Add(count, RandomColor());
                }
                _OutTexture.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(cR.RGB.x, cR.RGB.y, cR.RGB.z));
                tempTex.SetPixel(cR.X / _ResDiv, _OutTexture.height - cR.Y / _ResDiv, new Color(_regionColors[count].r, _regionColors[count].g, _regionColors[count].b));
            }
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
