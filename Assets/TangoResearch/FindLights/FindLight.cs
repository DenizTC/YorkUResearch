using UnityEngine;
using UnityEngine.UI;
using Tango;
using System.Collections.Generic;
using System;
using System.Collections;

class ColorPoint
{
    public Vector3 RGB;
    public float Luma;
    public int X;
    public int Y;
    public ColorPoint(int x, int y, float luma, Vector3 rgb)
    {
        X = x;
        Y = y;
        Luma = luma;
        RGB = rgb;
    }
}

public class FindLight : MonoBehaviour, ITangoVideoOverlay, ITangoLifecycle, ITangoDepth
{
    private TangoApplication _tangoApplication;

    public Camera _MainCam;
    public RenderTexture _InTexture;
    public Texture2D _OutTexture;
    /// <summary>
    /// Display _OutTexture on this quad face.
    /// </summary>
    public Renderer _QuadResult;
    public Material _ScreenMat;

    public Transform _LightObject;
    public Button _ButtonSwitchBG;
    public Button _ButtonFindLights;
    public Button _ButtonCreateLight;
    public Slider _SliderThreshold;
    public Slider _SliderSpotSize;

    /// <summary>
    /// If true, bright spots are updated when ever new image is available, otherwise,
    /// bright spots updated manually in the on click event of _ButtonFindLights.
    /// </summary>
    public bool _Continuous = true;
    private float _Threshold = 0.65f;
    private int _SpotSize = 10;
    private bool _bgIsInTexture = false;
    private TangoUnityImageData _lastImageBuffer = null;


    public TangoPointCloud _PointCloud;

    /// <summary>
    /// If set, then the depth camera is on and we are waiting for the next depth update.
    /// </summary>
    private bool m_findPlaneWaitingForDepth;

    void Start () {
        _tangoApplication = FindObjectOfType<TangoApplication>();
        if (_tangoApplication != null)
        {
            _tangoApplication.Register(this);
        }

        _ButtonSwitchBG.onClick.AddListener(onClickSwitchBG);
        _ButtonFindLights.onClick.AddListener(onClickFindLights);
        _ButtonCreateLight.onClick.AddListener(onClickCreateLight);

        _SliderThreshold.value = _Threshold;
        _SliderSpotSize.value = _SpotSize / 10;
        _SliderThreshold.onValueChanged.AddListener(onValueChangedThreshold);
        _SliderSpotSize.onValueChanged.AddListener(onValueChangedSpotSize);
        

        _OutTexture = new Texture2D(1280 / 8, 720 / 8);
	}

    private void onClickSwitchBG()
    {
        _bgIsInTexture = !_bgIsInTexture;

        if (_bgIsInTexture)
            _ScreenMat.mainTexture = _InTexture;
        else
            _ScreenMat.mainTexture = _OutTexture;
    }

    private void onClickCreateLight()
    {
        

        ColorPoint p = findBrightSpots(_lastImageBuffer, ref _OutTexture, _Threshold, _SpotSize);
        Debug.Log(p.X*8 + " " + p.Y*8);
        StartCoroutine(_WaitForDepthAndFindPlane(new Vector2(p.X * 8, p.Y * 8)));

        
    }

    private void onValueChangedSpotSize(float value)
    {
        _SpotSize = (int)value * 10;
    }

    private void onValueChangedThreshold(float value)
    {
        _Threshold = value;
    }

    private void onClickFindLights()
    {
        if (!_Continuous)
        { 
            findBrightSpots(_lastImageBuffer, ref _OutTexture, _Threshold, _SpotSize);
            _QuadResult.material.mainTexture = _OutTexture;
        }
    }

    /// <summary>
    /// Finds the bright spots of the color image.
    /// </summary>
    /// <param name="imageBuffer">The image buffer.</param>
    /// <param name="outTexture">The out texture.</param>
    /// <param name="threshold">The brightness threshold.</param>
    /// <param name="spotSize">Size of the bright spot.</param>
    /// <returns>The centre point of all the bright spots taken into account.</returns>
    private static ColorPoint findBrightSpots(TangoUnityImageData imageBuffer, 
        ref Texture2D outTexture, 
        float threshold, int spotSize)
    {

        int width = (int)imageBuffer.width;
        int height = (int)imageBuffer.height;
        int uv_buffer_offset = width * height;

        float brightestPixel = 0;
        int[] brightestPixelUV = { 0, 0 };

        List<ColorPoint> brightSpots = new List<ColorPoint>();

        int wS = width / outTexture.width;
        int hS = height / outTexture.height;
        // Code from TangoEnvironmentalLighting script.
        for (int i = 0; i < outTexture.height; ++i)
        {
            for (int j = 0; j < outTexture.width; ++j)
            {
                int iS = i * hS;
                int jS = j * wS;

                int x_index = jS;
                if (jS % 2 != 0)
                {
                    x_index = jS - 1;
                }

                // Get the YUV color for this pixel.
                int yValue = imageBuffer.data[(iS * width) + jS];
                int uValue = imageBuffer.data[uv_buffer_offset + ((iS / 2) * width) + x_index + 1];
                int vValue = imageBuffer.data[uv_buffer_offset + ((iS / 2) * width) + x_index];

                // Convert the YUV value to RGB.
                float r = yValue + (1.370705f * (vValue - 128));
                float g = yValue - (0.689001f * (vValue - 128)) - (0.337633f * (uValue - 128));
                float b = yValue + (1.732446f * (uValue - 128));
                Vector3 result = new Vector3(r / 255.0f, g / 255.0f, b / 255.0f);

                // Gamma correct color to linear scale.
                //result.x = Mathf.Pow(Mathf.Max(0.0f, result.x), 2.2f);
                //result.y = Mathf.Pow(Mathf.Max(0.0f, result.y), 2.2f);
                //result.z = Mathf.Pow(Mathf.Max(0.0f, result.z), 2.2f);

                if (yValue / 255f >= brightestPixel) {
                    brightestPixel = yValue / 255f;
                    brightestPixelUV[0] = j;
                    brightestPixelUV[1] = height / 8 - i - 1;
                }

                if (yValue / 255f > threshold)
                {
                    brightSpots.Add(new ColorPoint(j, height / 8 - i - 1, yValue / 255f, result));
                    outTexture.SetPixel(j, height / 8 - i - 1, new Color(result.x, result.y*0.5f, result.z*0.5f));
                }
                else
                    outTexture.SetPixel(j, height / 8 - i - 1, new Color(result.x, result.y, result.z));
            }
        }

        ColorPoint ave = new ColorPoint(0, 0, 0, Vector3.zero);
        if (brightestPixel > threshold)
        {
            //for (int w = -3; w < 3; w++)
            //{
            //    for (int h = -3; h < 3; h++)
            //    {
            //        outTexture.SetPixel(brightestPixelUV[0] + w, brightestPixelUV[1] + h, Color.green);
            //    }
            //}

            int count = 0;
            foreach (ColorPoint p in brightSpots)
            {
                
                if (p.X < brightestPixelUV[0] + spotSize / 2 &&
                    p.X > brightestPixelUV[0] - spotSize / 2 &&
                    p.Y < brightestPixelUV[1] + spotSize / 2 &&
                    p.Y > brightestPixelUV[1] - spotSize / 2) {

                    ave.X += p.X;
                    ave.Y += p.Y;
                    ave.Luma += p.Luma;
                    ave.RGB += p.RGB;

                    outTexture.SetPixel(p.X, p.Y, new Color(p.Luma*0.5f, p.Luma*0.5f, p.Luma));

                    count++;
                }
            }
            ave.X /= count;
            ave.Y /= count;
            ave.Luma /= count;
            ave.RGB /= count;

            int avePRad = 4;
            for (int w = -avePRad; w < avePRad; w++)
            {
                for (int h = -avePRad; h < avePRad; h++)
                {
                    if (w == -avePRad || w == avePRad-1 || h == -avePRad || h == avePRad-1)
                    {
                        // Outline the spot in black.
                        outTexture.SetPixel(ave.X + w, ave.Y + h, Color.black);
                    }
                    else
                    {
                        outTexture.SetPixel(ave.X + w, ave.Y + h, new Color(ave.RGB.x, ave.RGB.y, ave.RGB.z));
                    }
                }
            }

        }

        outTexture.Apply();

        return ave;
    }

    /// <summary>
    /// Code from ARGUIController.
    /// Wait for the next depth update, then find the plane at the touch position.
    /// </summary>
    /// <param name="touchPosition">Touch position to find a plane at.</param>
    private IEnumerator _WaitForDepthAndFindPlane(Vector2 touchPosition)
    {
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
        
        int idx = _PointCloud.FindClosestPoint(_MainCam, touchPosition, 4);
        if (idx == -1)
        {
            yield break;
        }
        Vector3 point = _PointCloud.m_points[idx];
        GameObject newGO =
            Instantiate(_LightObject.gameObject, point, Quaternion.identity) as GameObject;
        yield break;

    }

    public void OnTangoImageAvailableEventHandler(TangoEnums.TangoCameraId cameraId, TangoUnityImageData imageBuffer)
    {
        //if(cameraId == TangoEnums.TangoCameraId.TANGO_CAMERA_FISHEYE)
        //{
        //    Debug.Log("Fisheye");
        //}

        _lastImageBuffer = imageBuffer;
        if (_Continuous)
        {
            findBrightSpots(imageBuffer, ref _OutTexture, _Threshold, _SpotSize);
            _QuadResult.material.mainTexture = _OutTexture;
        }
    }

    public void OnTangoPermissions(bool permissionsGranted)
    {
#if UNITY_EDITOR
        _OutTexture = new Texture2D(1280 / 8, 720 / 8, TextureFormat.ARGB32, false);
#else
        TangoCameraIntrinsics intrinsics = new TangoCameraIntrinsics();
        VideoOverlayProvider.GetIntrinsics(TangoEnums.TangoCameraId.TANGO_CAMERA_COLOR, intrinsics);
        _OutTexture = new Texture2D((int)intrinsics.width / 8, (int)intrinsics.height / 8, TextureFormat.RGBA32, false);
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
}
