using UnityEngine;
using System.Collections;

/// <summary>
/// This class outputs the color (RGB) feed of the tango camera to a RenderTexture, which can be used elsewhere.
/// The TangoARCamera prefab must be active, since it sends the YUV data to the ar_screen material. The resulting
/// RenderTexture output is simply the output of the ar_screen material. 
/// </summary>
public class TangoRGB_Out : MonoBehaviour {

    public RenderTexture ResultTexture;
    public int Size = 256;
    public TangoARScreen _TangoARCamera;

    void Awake()
    {
        if (ResultTexture == null)
        {
            ResultTexture = new RenderTexture(Size, Size, 0);
            ResultTexture.name = "TangoRGB_Out";
        }
        Graphics.Blit(null, ResultTexture, _TangoARCamera.m_screenMaterial);
    }

    void Update() { 
        Graphics.Blit(null, ResultTexture, _TangoARCamera.m_screenMaterial);
    }
}
