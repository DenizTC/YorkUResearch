using System.Collections;
using Tango;
using UnityEngine;

/// <summary>
/// Collection of helpers functions for tango data.
/// </summary>
public static class TangoHelpers {

    /// <summary>
    /// Gets the YUV color from an image.
    /// </summary>
    /// <param name="imageBuffer">The image buffer.</param>
    /// <param name="x">The x coordinate of the image.</param>
    /// <param name="y">The y coordinate of the image.</param>
    /// <returns>The unscaled YUV color. Each component ranges from 0 to 255. </returns>
    public static Vector3 GetYUV(TangoUnityImageData imageBuffer, int x, int y)
    {
        int width = (int)imageBuffer.width;
        int height = (int)imageBuffer.height;
        int size = width * height;

        int x_index = x;
        if (x % 2 != 0)
        {
            x_index = x - 1;
        }

        int Y = imageBuffer.data[(y * width) + x];
        int U = imageBuffer.data[size + ((y / 2) * width) + x_index + 1];
        int V = imageBuffer.data[size + ((y / 2) * width) + x_index];

        Vector3 result = new Vector3(Y, U, V);
        return result;
    }

    public static Vector3[,] ImageBufferToArray(TangoUnityImageData imageBuffer, uint resDiv = 8, bool convertToRGB = true)
    {
        uint _width = imageBuffer.width / resDiv;
        uint _height = imageBuffer.height / resDiv;

        //Debug.Log("resDiv: " + resDiv + " w: " + _width + " h: " + _height);

        Vector3[,] result = new Vector3[_width, _height];
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                Vector3 yuv = GetYUV(imageBuffer, i*(int)resDiv, j * (int)resDiv);
                if (!convertToRGB)
                    result[i, j] = yuv;
                else
                    result[i, j] = ImageProcessing.YUVToRGB(yuv, false, false);
            }
        }

        return result;
    }

    public static int[,] ImageBufferToGrayscaleArray(TangoUnityImageData imageBuffer, uint resDiv = 8)
    {
        uint _width = imageBuffer.width / resDiv;
        uint _height = imageBuffer.height / resDiv;

        int[,] result = new int[_width, _height];
        for (int i = 0; i < imageBuffer.width; i++)
        {
            for (int j = 0; j < imageBuffer.height; j++)
            {
                Vector3 yuv = GetYUV(imageBuffer, i, j);
                Vector3 rgb = ImageProcessing.YUVToRGB(yuv, false, false);
                result[i, j] = (int)ImageProcessing.Grayscale(rgb);
            }
        }

        return result;
    }

    
}
