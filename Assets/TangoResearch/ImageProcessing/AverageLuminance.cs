using UnityEngine;
using System.Collections;

public class AverageLuminance : MonoBehaviour {


    public Texture2D _RGBTexture;

    // Called once only at the start.
    private void Start()
    {
        // Make sure the texture exists.
        if(_RGBTexture == null)
        {
            Debug.LogWarning("The RGB texture in the script AverageLuminance cannot be empty!");
        }
    }

    // Called once every frame.
    private void Update() {

        //float averageLuminance = AverageLuma();
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

        for (int i = 0; i < _RGBTexture.width; i++)
        {
            for (int j = 0; j < _RGBTexture.height; j++)
            {
                Color curPixel = _RGBTexture.GetPixel(i, j);
                // Do stuff here.

            }
        }

        return result;
    }

}
