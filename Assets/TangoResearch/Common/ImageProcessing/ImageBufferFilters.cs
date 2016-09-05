using UnityEngine;
using System.Collections;
using Tango;

public static class ImageBufferFilters {

    /// <summary>
    /// Finds the average color of a given image.
    /// </summary>
    /// <param name="imageBuffer">The image buffer.</param>
    /// <param name="resolusionDiv">The number to divide the resolution.</param>
    /// <returns>The average color (RGB).</returns>
    public static Vector3 AverageColor(TangoUnityImageData imageBuffer, int resolusionDiv = 1)
    {

        Vector3 ave = Vector3.zero;

        int wS = (int)(imageBuffer.width / (float)resolusionDiv);
        int hS = (int)(imageBuffer.height / (float)resolusionDiv);

        for (int i = 0; i < hS; i++)
        {
            for (int j = 0; j < wS; j++)
            {
                int iS = i * resolusionDiv;
                int jS = j * resolusionDiv;

                Vector3 yuv = TangoHelpers.GetYUV(imageBuffer, jS, iS);
                Vector3 rgb = TangoHelpers.YUVToRGB(yuv);

                ave += rgb;

            }
        }

        ave /= wS * hS;

        return ave;
    }


}
