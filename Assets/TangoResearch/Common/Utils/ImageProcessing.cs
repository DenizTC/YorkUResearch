using UnityEngine;
using System.Collections;
using System;

public static class ImageProcessing {

    public static float[,] HSobelKernel = {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };

    public static float[,] VSobelKernel = {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };

    #region Utilities

    public static Color RandomColor()
    {
        Color col = new Color((float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble(),
                    (float)GameGlobals.Rand.NextDouble());
        return col;
    }

    public static Texture2D RenderTextureToTexture2D(RenderTexture rTex)
    {
        Texture2D texture2D = new Texture2D(rTex.width, rTex.height);
        RenderTexture.active = rTex;
        texture2D.ReadPixels(new Rect(0, 0, rTex.width, rTex.height), 0, 0);
        texture2D.Apply();
        return texture2D;
    }

    public static Vector3[,] RenderTextureToRGBArray(RenderTexture rTex)
    {
        Texture2D t = RenderTextureToTexture2D(rTex);
        Vector3[,] c = new Vector3[t.width, t.height];
        for (int i = 0; i < t.width; i++)
        {
            for (int j = 0; j < t.height; j++)
            {
                c[i, j] = ColorToVector3(t.GetPixel(i, t.height - j - 1)) * 255;
            }
        }
        return c;
    }

    #endregion

    #region Converters

    /// <summary>
    /// Converts an unscaled YUV to RGB.
    /// </summary>
    /// <param name="yuv">The unscaled (0-255) YUV.</param>
    /// <param name="gammaCorrect">if set to <c>true</c> [gamma correct].</param>
    /// <param name="scaleRGB">if set to <c>true</c> [scale RGB between 0-1].</param>
    /// <returns></returns>
    public static Vector3 YUVToRGB(Vector3 yuv, bool gammaCorrect = false, bool scaleRGB = true)
    {

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

    public static Color Vector3ToColor(Vector3 v)
    {
        return new Color(v.x, v.y, v.z);
    }

    public static Vector3 ColorToVector3(Color c)
    {
        Vector3 result = new Vector3(c.r, c.g, c.b);
        return result;
    }

    #endregion

    #region Filters

    private static float[] SortedWindow = new float[9];

    private static float quant(float x)
    {
        x = Mathf.Clamp(x, 0f, 1f);
        return Mathf.Floor(x * 255f);
    }

    private static float pack(Vector3 c)
    {
        float lum = (c.x + c.y + c.z) * (1.0f / 3.0f);

        return quant(c.x) + quant(c.y) * 256f + quant(lum) * 65536f;

    }

    private static Vector3 unpack(float x)
    {
        float lum = Mathf.Floor(x * (1f / 65536f)) * (1f / 255f);
        Vector3 c = Vector3.zero;
        c.x = Mathf.Floor(x % 256f) * (1f / 255f);
        c.y = Mathf.Floor((x * (1f / 256f) % 256f)) * (1f / 255f);
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

    /// <summary>
    /// Finds the gradient using a 3x3 sobel operator.
    /// Source: http://cuda-programming.blogspot.ca/2013/01/sobel-filter-implementation-in-c.html
    /// </summary>
    /// <param name="pixels">The pixels array.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns></returns>
    public static int Sobel(ref Vector3[,] pixels, int x, int y)
    {

        // Horizontal convolution.
        float gX = 0f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xn = x + i - 1;
                int yn = y + j - 1;
                try
                {
                    gX += ImageProcessing.Grayscale(pixels[xn, yn]) * HSobelKernel[i, j];
                }
                catch (Exception)
                {
                    Debug.Log("Index error: " + xn + "x" + yn + " - Pixels[] size: " + pixels.GetLength(0) + "x" + pixels.GetLength(1));
                    throw;
                }

            }
        }

        // Vertical convolution.
        float gY = 0f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xn = x + i - 1;
                int yn = y + j - 1;
                gY += ImageProcessing.Grayscale(pixels[xn, yn]) * VSobelKernel[i, j];
            }
        }

        float length = Mathf.Sqrt(Mathf.Pow(gX, 2) + Mathf.Pow(gY, 2));
        // Normalize between 0-1.
        length /= 4328.0f;
        // Normalize between 0-255.
        length *= 255f;

        return (int)length;
    }

    /// <summary>
    /// Applies a sobel filter on the specified pixels array.
    /// </summary>
    /// <param name="pixels">The array of non-normalized (0-255) pixels.</param>
    /// <returns></returns>
    public static float[,] SobelFilter3x3(ref Vector3[,] pixels, bool normalize = false)
    {

        float[,] result = new float[pixels.GetLength(0), pixels.GetLength(1)];

        for (int i = 1; i < pixels.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < pixels.GetLength(1) - 1; j++)
            {
                result[i, j] = Sobel(ref pixels, i, j);
                if (normalize) result[i, j] /= 255f;
            }
        }
        return result;
    }

    public static float Grayscale(Vector3 rgb)
    {
        float luma = 0.2126f * rgb.x + 0.7152f * rgb.y + 0.0722f * rgb.z;
        return luma;
    }

    #endregion

    #region Rendering

    public static Vector3 LightDirection(Vector3 lightPos, Vector3 targetPos)
    {
        return (lightPos- targetPos).normalized;
    }

    public static bool ComputeImageIntensity(float albedo, Vector3 normal, Vector3 lightDir, out float intensity, float lightIntensity = 1)
    {
        float ns = Vector3.Dot(normal, lightDir);
        //intensity = albedo * Mathf.Min(ns, 0);
        //intensity = Mathf.Min(albedo * ns * lightIntensity, 1f);
        intensity = albedo * ns * lightIntensity;

        return ns > 0;
    }

    public static bool ComputeImageIntensity(float albedo, float ns, out float intensity, float lightIntensity = 1)
    {
        //intensity = Mathf.Min(albedo * ns*lightIntensity, 1f);
        intensity = albedo * ns * lightIntensity;
        return ns > 0;
    }

    public static bool ComputeAlbedo(float imageIntensity, Vector3 normal, Vector3 lightDir, out float ns, out float albedo, float lightIntensity = 1)
    {
        //float ns = normal.x * lightDir.x + normal.y*lightDir.y + normal.z*lightDir.z;
        ns = Vector3.Dot(normal, lightDir) * lightIntensity;
        albedo = imageIntensity / ns;

        //if (albedo > 1)
        //{
        //    albedo = 1;
        //    //return false;
        //}

        return ns > 0;
    }

    public static bool ComputeAlbedo(float imageIntensity, float ns, out float albedo, float lightIntensity = 1)
    {
        albedo = imageIntensity / ns*lightIntensity;


        //if (albedo > 1)
        //{
        //    albedo = 1;
        //    //return false;
        //}

        return ns > 0;
    }

    #endregion

}
