using UnityEngine;
using System.Collections;

public class CustomZBufferEffect : MonoBehaviour {

    public Shader _NoFilter;
    public Shader _GaussianFilter;
    public Shader _KuwaharaFilter;
    public Shader _MedianFilter;
    public Shader _GuidedFilter;
    public Shader _MaskedMeanFilter;
    public TangoRGB_Out _RGBMapGenerator;
    public PointCloudToDepthMap _DepthMapGenerator;

    public Material _material;
    public RenderTexture _RT;
    public Texture2D _T2D;

	void Start () {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

        _material = new Material(_NoFilter);
        _material.SetTexture("_RGBReal", _RGBMapGenerator.ResultTexture);
        _material.SetTexture("_DepthReal", _DepthMapGenerator._depthTexture);

        //_RT = new RenderTexture(160, 90, 0);

    }

    public void ChangeFilter(Enums.DepthFilter filter) {
        switch (filter)
        {
            case Enums.DepthFilter.NONE:
                _material.shader = _NoFilter;
                break;
            case Enums.DepthFilter.KUWAHARA:
                _material.shader = _KuwaharaFilter;
                break;
            case Enums.DepthFilter.GUIDEDFILTER:
                _material.shader = _GuidedFilter;
                break;
            case Enums.DepthFilter.GAUSSIAN:
                _material.shader = _GaussianFilter;
                break;
            case Enums.DepthFilter.MEDIAN:
                _material.shader = _MedianFilter;
                break;
            case Enums.DepthFilter.MASKEDMEAN:
                _material.shader = _MaskedMeanFilter;
                break;
            default:
                break;
        }
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {

        //material.SetFloat("_DepthLevel", depthLevel);
        _material.SetTexture("_RGBReal", _RGBMapGenerator.ResultTexture);
        _material.SetTexture("_DepthReal", _DepthMapGenerator._depthTexture);
        Graphics.Blit(src, dest, _material);

    }


}
