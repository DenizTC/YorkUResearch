using UnityEngine;
using System.Collections;

public class CustomZBufferEffect : MonoBehaviour {

    public Shader _Shader;
    public TangoRGB_Out _RGBMapGenerator;
    public PointCloudToDepthMap _DepthMapGenerator;

    public Material _material;

	void Start () {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        _material = new Material(_Shader);
        _material.SetTexture("_RGBReal", _RGBMapGenerator.ResultTexture);
        _material.SetTexture("_DepthReal", _DepthMapGenerator._depthTexture);
        _material.SetFloat("_DepthWidth", _DepthMapGenerator._depthMapWidth);
        _material.SetFloat("_DepthHeight", _DepthMapGenerator._depthMapHeight);
    }
	
    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        //material.SetFloat("_DepthLevel", depthLevel);
        _material.SetTexture("_RGBReal", _RGBMapGenerator.ResultTexture);
        _material.SetTexture("_DepthReal", _DepthMapGenerator._depthTexture);
        Graphics.Blit(src, dest, _material);
    }



}
