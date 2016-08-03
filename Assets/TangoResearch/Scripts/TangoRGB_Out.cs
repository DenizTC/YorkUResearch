using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This class outputs the color (RGB) feed of the tango camera to a RenderTexture, which can be used elsewhere.
/// The TangoARCamera prefab must be active, since it sends the YUV data to the ar_screen material. The resulting
/// RenderTexture output is simply the output of the ar_screen material. 
/// </summary>
public class TangoRGB_Out : MonoBehaviour {

    public RenderTexture ResultTexture;
    public RenderTexture LastRGBSyncedWithDepth;
    public int Size = 256;
    public TangoARScreen _TangoARCamera;

    public static TangoRGB_Out TangoRGBGenerator;

    public Queue<RenderTexture> RTs = new Queue<RenderTexture>(5);

    void Awake()
    {
        if (!TangoRGBGenerator)
            TangoRGBGenerator = this;

        if (ResultTexture == null)
        {
            ResultTexture = new RenderTexture(Size, Size, 0);
            ResultTexture.name = "TangoRGB_Out";
        }
        Graphics.Blit(null, ResultTexture, _TangoARCamera.m_screenMaterial);
    }

    int i = 0;
    void Update() {
        //RenderTexture RT = new RenderTexture(Size, Size, 0);
        //RT.name = i++.ToString();
        Graphics.Blit(null, ResultTexture, _TangoARCamera.m_screenMaterial);
        //if (RTs.Count >= 5)
        //{
        //    LastRGBSyncedWithDepth = RTs.Peek();
        //    RTs.Clear();
        //}
        //RTs.Enqueue(RT);

        //ResultTexture = RT;

    }

    public void SetSyncedRGB() {
        LastRGBSyncedWithDepth = ResultTexture;
    }

}
