using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Camera))]
public class CaptureCubemap : MonoBehaviour {

    public Button _ButtonCaptureCubemap;
    public int _CubemapSize = 64;

    public RenderTexture _RTCubemap;
    public Cubemap _Cubemap;

    public Material _MatIBL;

	void Start () {
        _ButtonCaptureCubemap.onClick.AddListener(onClickCapture);

        _RTCubemap = new RenderTexture(_CubemapSize, _CubemapSize, 16);
        _RTCubemap.dimension = UnityEngine.Rendering.TextureDimension.Cube;

        _Cubemap = new Cubemap(_CubemapSize, TextureFormat.RGB24, false);

	}

    private void onClickCapture()
    {
        float sunIntensity = 0;
        if (ARDirectionalLight._Sun != null) {
            sunIntensity = ARDirectionalLight._Sun._lightIntensity;
            ARDirectionalLight._Sun._lightIntensity = 0;
        }

        
        RenderSettings.skybox = null;
        DynamicGI.UpdateEnvironment();

        GetComponent<Camera>().RenderToCubemap(_Cubemap);
        _MatIBL.SetTexture("_Tex", _Cubemap);

        RenderSettings.skybox = _MatIBL;
        DynamicGI.UpdateEnvironment();

        if (ARDirectionalLight._Sun != null)
        {
            ARDirectionalLight._Sun._lightIntensity = sunIntensity;
        }

    }

}
