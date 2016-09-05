using UnityEngine;
using System.Collections;
using Tango;

public static class TangoImageProcessing {

    /// <summary>
    /// Finds the average color of a given image.
    /// </summary>
    /// <param name="imageBuffer">The image buffer.</param>
    /// <param name="resolutionDiv">The number to divide the resolution.</param>
    /// <returns>The average color (RGB).</returns>
    public static Vector3 AverageColor(TangoUnityImageData imageBuffer, int resolutionDiv = 8)
    {

        Vector3 ave = Vector3.zero;

        int wS = (int)(imageBuffer.width / (float)resolutionDiv);
        int hS = (int)(imageBuffer.height / (float)resolutionDiv);

        for (int i = 0; i < hS; i++)
        {
            for (int j = 0; j < wS; j++)
            {
                int iS = i * resolutionDiv;
                int jS = j * resolutionDiv;

                Vector3 yuv = TangoHelpers.GetYUV(imageBuffer, jS, iS);
                Vector3 rgb = TangoHelpers.YUVToRGB(yuv);

                ave += rgb;

            }
        }

        ave /= wS * hS;

        return ave;
    }

    public static float AverageLuminescence(TangoUnityImageData imageBuffer, int resolutionDiv = 8) {

        Vector3 aveColor = AverageColor(imageBuffer, resolutionDiv);

        float luma = 0.2126f * aveColor.x + 0.7152f * aveColor.y + 0.0722f * aveColor.z;

        return luma;
    }


}
