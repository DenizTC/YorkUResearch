using UnityEngine;
using System.Collections;

/// <summary>
/// This class sets the properties of the custom tango occlusion shader.
/// </summary>
[RequireComponent(typeof(Camera))]
public class TangoOcclusion : MonoBehaviour {

    public Shader _NoFilter;
    public Shader _GaussianFilter;
    public Shader _KuwaharaFilter;
    public Shader _MedianFilter;
    public Shader _GuidedFilter;
    public Shader _MaskedMeanFilter;
    public TangoRGBGenerator _RGBGenerator;
    public TangoDepthGenerator _DepthGenerator;

    private Material _material;

	void Start () {

        // Needed to use built in _CameraDepthTexture shader variable.
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth; 

        _material = new Material(_NoFilter);
        _material.SetTexture("_RGBReal", _RGBGenerator._TangoRGBTexture);
        _material.SetTexture("_DepthReal", _DepthGenerator._depthTexture);
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

    public void ChangeBackground(Enums.BackgroundMode mode) {

        switch (mode)
        {
            case Enums.BackgroundMode.COLOR:
                _material.SetFloat("_RGBMode", 1);
                break;
            case Enums.BackgroundMode.DEPTH:
                _material.SetFloat("_RGBMode", 0);
                break;
            default:
                break;
        }

    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        
        _material.SetTexture("_RGBReal", _RGBGenerator._TangoRGBTexture);
        _material.SetTexture("_DepthReal", _DepthGenerator._depthTexture);
        Graphics.Blit(src, dest, _material);

    }


}
