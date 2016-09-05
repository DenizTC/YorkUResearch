using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class LightDetectorImageEffect : MonoBehaviour {

    public Shader _LightDetectorShader;
    private Material _material;

    void Start()
    {
        _material = new Material(_LightDetectorShader);
#if UNITY_ANDROID && !UNITY_EDITOR
        _material.SetFloat("_Android", 1);
#else
        _material.SetFloat("_Android", 0);
#endif
        enabled = false;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {

        if(FindLight._LightDetector.IsRunning())
            _material.SetTexture("_OtherTex", FindLight._LightDetector._OutTexture);
        Graphics.Blit(src, dest, _material);

    }
}
