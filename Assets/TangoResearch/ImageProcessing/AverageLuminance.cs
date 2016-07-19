using UnityEngine;
using System.Collections;

public class AverageLuminance : MonoBehaviour {

    public RenderTexture _LiveTexture; // The RGB live video of from the tango
    public Texture2D _ResultTexture; // RenderTexture must be copied to Texture2D to read its pixels

    // Called once only at the start.
    private void Start()
    {
        // Make sure the texture exists.
        if(_LiveTexture == null)
        {
            Debug.LogWarning("The live texture in the script AverageLuminance cannot be empty!");
        }

        _ResultTexture = new Texture2D(_LiveTexture.width, _LiveTexture.height);

    }

    // Called once every frame.
    private void Update()
    {

        float averageLuminance = AverageLuma();
        //Debug.Log("Average luminance of this frame is: " + averageLuminance);
    }

    private float AverageLuma() {
        float result = 0.0f;

        // TODO! Compute average luminance off all pixels.
        // Each pixel is in RGB, so make sure to convert to another color space that has luminance.
        // Get pixel like this: Color curPixel = _RGBTexture.GetPixel(x, y);
        // The red channel: curPixel.r
        // The green channel: curPixel.g
        // The blue channel: curPixel.b
        // The alpha channel (not important for what we are doing): curPixel.a

        RenderTexture.active = _LiveTexture;
        _ResultTexture.ReadPixels(new Rect(0, 0, _LiveTexture.width, _LiveTexture.height), 0, 0);

        for (int i = 0; i < _LiveTexture.width; i++)
        {
            for (int j = 0; j < _LiveTexture.height; j++)
            {
                Color curPixel = _ResultTexture.GetPixel(i, j);
                // Do stuff here.


            }
        }
        return result;
    }

}
