using UnityEngine;
using Tango;


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

    private float[,] _depthMap;
    private bool[,] _depthMapDirty;

    public void Start()
    {
        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        _depthMap = new float[_depthMapWidth + 1, _depthMapHeight + 1];
        _depthMapDirty = new bool[_depthMapWidth + 1, _depthMapHeight + 1];
        if(_depthTexture == null)
            _depthTexture = new Texture2D(_depthMapWidth, _depthMapHeight, TextureFormat.ARGB32, false);
        _depthTexture.filterMode = FilterMode.Point;

    }

    public void OnDestroy()
    {
        if (_tangoApplication != null)
        {
            _tangoApplication.Unregister(this);
        }
    }

    private void GenerateDepthMap(ref TangoUnityDepth tangoUnityDepth)
    {
        float dW = _depthMapWidth / 1280f;
        float dH = _depthMapHeight / 720f;

        for (int i = 0; i < tangoUnityDepth.m_pointCount; i++)
        {
            float Z = 1 - tangoUnityDepth.m_points[i * 3 + 2] / 4.5f;
            //float Z = 1 - Mathf.Clamp01(tangoUnityDepth.m_points[i * 3 + 2] / 4.5f);

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
            _depthMap[x, y] = Z;
            _depthMapDirty[x, y] = true;
        }

        for (int i = 0; i < _depthMapWidth; i++)
        {
            for (int j = 0; j < _depthMapHeight; j++)
            {
                if (_depthMapDirty[i, j])
                {
                    _depthTexture.SetPixel(i, _depthMapHeight - j,
                        new Color(_depthMap[i, j], _depthMap[i, j], _depthMap[i, j]));
                }
                else
                {
                    _depthTexture.SetPixel(i, _depthMapHeight - j, Color.black);
                }
                _depthMapDirty[i, j] = false;
            }
        }

        _depthTexture.Apply();
        _DepthMapQuad.sharedMaterial.mainTexture = _depthTexture;

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

    /// <summary>
    /// Fix this! Doesn't output correct values. Input matrix probably not correct.
    /// Generates the depth map using nearest neighbor upsampling.
    /// </summary>
    private void GenerateDepthMap_NearestNeighbor(ref TangoUnityDepth tangoUnityDepth)
    {
        TangoPoseData poseData = new TangoPoseData();
        TangoCoordinateFramePair pair;
        pair.baseFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_START_OF_SERVICE;
        pair.targetFrame = TangoEnums.TangoCoordinateFrameType.TANGO_COORDINATE_FRAME_DEVICE;
        PoseProvider.GetPoseAtTime(poseData, tangoUnityDepth.m_timestamp, pair);
        if (poseData.status_code != TangoEnums.TangoPoseStatusType.TANGO_POSE_VALID)
        {
            return;
        }
        Vector3 position;
        Quaternion rotation;
        TangoSupport.TangoPoseToWorldTransform(poseData, out position, out rotation);

        Matrix4x4 ccWorld = Matrix4x4.TRS(position, rotation, Vector3.one);
        bool isValid = false;
        Vector3 colorCameraPoint = new Vector3();
        for (int i = 0; i < _depthMapWidth; i++)
        {
            for (int j = 0; j < _depthMapHeight; j++)
            {
                if (TangoSupport.ScreenCoordinateToWorldNearestNeighbor(
                    _PointCloud.m_points, _PointCloud.m_pointsCount,
                    tangoUnityDepth.m_timestamp,
                    _ccIntrinsics,
                    ref ccWorld,
                    new Vector2(i / (float)_depthMapWidth, j / (float)_depthMapHeight),
                    out colorCameraPoint, out isValid) == Common.ErrorType.TANGO_INVALID)
                {
                    _depthTexture.SetPixel(i, j, Color.red);
                    continue;
                }

                if (isValid)
                {
                    float c = 1 - colorCameraPoint.z / 4.5f;
                    _depthTexture.SetPixel(i, j, new Color(c, c, c));
                }
                else
                {
                    _depthTexture.SetPixel(i, j, Color.black);
                }
            }
        }
        _depthTexture.Apply();
        _DepthMapQuad.sharedMaterial.mainTexture = _depthTexture;

        //_debugMessage = "DepthAvailable: " + _waitingForDepth.ToString() + "\n" +
        //    " points: " + _PointCloud.m_pointsCount + "\n" +
        //    " timestamp: " + tangoUnityDepth.m_timestamp.ToString("0.00") + "\n" +
        //    " XYZ:" + colorCameraPoint.ToString();
    }

    public void OnTangoDepthAvailable(TangoUnityDepth tangoDepth)
    {
        GenerateDepthMap(ref tangoDepth);
        //GenerateDepthMap_NearestNeighbor(ref tangoDepth);
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