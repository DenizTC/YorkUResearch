using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class outputs the color (RGB) feed of the tango camera to a RenderTexture, which can be used elsewhere.
/// The TangoARCamera prefab must be active, since it sends the YUV data to the ar_screen material. The resulting
/// RenderTexture output is simply the output of the ar_screen material. 
/// </summary>
public class TangoRGBGenerator : MonoBehaviour {

    public RenderTexture _TangoRGBTexture;
    public int Size = 256;
    public TangoARScreen _TangoARCamera;

    public static TangoRGBGenerator _TangoRGBGenerator;

    void Awake()
    {
        if (!_TangoRGBGenerator)
            _TangoRGBGenerator = this;

        if (_TangoRGBTexture == null)
        {
            _TangoRGBTexture = new RenderTexture(Size, Size, 0);
            _TangoRGBTexture.name = "TangoRGBTexture";
        }
        Graphics.Blit(null, _TangoRGBTexture, _TangoARCamera.m_screenMaterial);
    }

    void Update() {
        Graphics.Blit(null, _TangoRGBTexture, _TangoARCamera.m_screenMaterial);
    }

}
