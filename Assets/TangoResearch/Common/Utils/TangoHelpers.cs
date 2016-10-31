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

    /// <summary>
    /// Converts an unscaled YUV to RGB.
    /// </summary>
    /// <param name="yuv">The unscaled (0-255) YUV.</param>
    /// <param name="gammaCorrect">if set to <c>true</c> [gamma correct].</param>
    /// <param name="scaleRGB">if set to <c>true</c> [scale RGB between 0-1].</param>
    /// <returns></returns>
    public static Vector3 YUVToRGB(Vector3 yuv, bool gammaCorrect = false, bool scaleRGB = true) {
        
        float r = yuv.x + (1.370705f * (yuv.z - 128));
        float g = yuv.x - (0.689001f * (yuv.z - 128)) - (0.337633f * (yuv.y - 128));
        float b = yuv.x + (1.732446f * (yuv.y - 128));

        Vector3 result = new Vector3(r, g, b);

        if (gammaCorrect)
        {
            result.x = Mathf.Pow(Mathf.Max(0.0f, result.x), 2.2f);
            result.y = Mathf.Pow(Mathf.Max(0.0f, result.y), 2.2f);
            result.z = Mathf.Pow(Mathf.Max(0.0f, result.z), 2.2f);
        }

        return (scaleRGB) ? result / 255f : result;
    }

    public static float Grayscale(Vector3 rgb)
    {
        float luma = 0.2126f * rgb.x + 0.7152f * rgb.y + 0.0722f * rgb.z;
        return luma;
    }

    private static float[] SortedWindow = new float[9];

    private static float quant(float x)
    {
        x = Mathf.Clamp(x, 0f, 1f);
        return Mathf.Floor(x * 255f);
    }

    private static float pack(Vector3 c)
    {
        float lum = (c.x + c.y + c.z) * (1.0f / 3.0f);

        return quant(c.x) + quant(c.y)*256f + quant(lum) * 65536f;

    }

    private static Vector3 unpack(float x)
    {
        float lum = Mathf.Floor(x * (1f/65536f)) * (1f/255f);
	    Vector3 c = Vector3.zero;
	    c.x = Mathf.Floor(x % 256f) 			* (1f/255f);
	    c.y = Mathf.Floor((x*(1f/256f)%256f)) * (1f / 255f);
	    c.z = lum * 3f - c.y - c.x;
	    return c;
    }

    private static void Swap(int a, int b)
    {
        float temp = Mathf.Max(SortedWindow[a], SortedWindow[b]);
        SortedWindow[a] = Mathf.Min(SortedWindow[a], SortedWindow[b]);
        SortedWindow[b] = temp;
    }

    private static Vector3 MedianSort3x3(ref Vector3[,] pixels, int x, int y)
    {
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                SortedWindow[i * 3 + j] = pack(pixels[x + i - 1, y + j - 1] / 255f);
            }
        }

        // Sorting network generated from: http://pages.ripco.net/~jgamble/nw.html
        Swap(0, 1); Swap(3, 4); Swap(6, 7);
        Swap(1, 2); Swap(4, 5); Swap(7, 8);
        Swap(0, 1); Swap(3, 4); Swap(6, 7); Swap(0, 3);
        Swap(3, 6); Swap(0, 3); Swap(1, 4);
        Swap(4, 7); Swap(1, 4); Swap(2, 5);
        Swap(5, 8); Swap(2, 5); Swap(1, 3); Swap(5, 7);
        Swap(2, 6); Swap(4, 6);
        Swap(2, 4); Swap(2, 3);
        Swap(5, 6);

        return unpack(SortedWindow[4]) * 255f;
    }

    public static Vector3[,] MedianFilter3x3(ref Vector3[,] pixels)
    {
        Vector3[,] result = new Vector3[pixels.GetLength(0), pixels.GetLength(1)];
        for (int i = 1; i < pixels.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < pixels.GetLength(1) - 1; j++)
            {
                result[i, j] = MedianSort3x3(ref pixels, i, j);
            }
        }
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
                    result[i, j] = YUVToRGB(yuv, false, false);
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
                Vector3 rgb = YUVToRGB(yuv, false, false);
                result[i, j] = (int)Grayscale(rgb);
            }
        }

        return result;
    }

    public static Color Vector3ToColor(Vector3 v)
    {
        return new Color(v.x, v.y, v.z);
    }
}
