using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

/// <summary>
/// CIELABXY Ck = [lk, ak, bk, xk, yk]^T, 
/// where [lab] is the pixel color vector in CIELAB color space,
/// and [xy] is the pixel coordinate.
/// </summary>
public class CIELABXY
{
    public float L;
    public float A;
    public float B;
    public int X;
    public int Y;

    public Vector3 RGB;

    public int label;

    public CIELABXY()
    {
        L = 0;
        A = 0;
        B = 0;
        X = 0;
        Y = 0;
    }

    public CIELABXY(float l, float a, float b, int x, int y)
    {
        this.L = l;
        this.A = a;
        this.B = b;
        this.X = x;
        this.Y = y;
    }

    public CIELABXY(Vector3 lab, int x, int y) : this(lab.x, lab.y, lab.z, x, y){
    }

    public CIELABXY(CIELABXY c) : this(c.L, c.A, c.B, c.X, c.Y) {
    }

}

public class CIELABXYCenter : CIELABXY
{
    public List<CIELABXY> Region = new List<CIELABXY>();

    public float ResidualError = float.MaxValue;

    public CIELABXYCenter() : base() {
    }

    public CIELABXYCenter(float l, float a, float b, int x, int y) : base(l, a, b, x, y) {
    }

    public CIELABXYCenter(Vector3 lab, int x, int y) : base(lab, x, y) {
    }

    public CIELABXYCenter(CIELABXYCenter c) : base(c.L, c.A, c.B, c.X, c.Y) {
        this.Region = c.Region;
    }

    public CIELABXYCenter(CIELABXY c) : this(c.L, c.A, c.B, c.X, c.Y) {
    }
}

/// <summary>
/// SLIC superpixel segmentation.
/// Based on the paper: http://www.kev-smith.com/papers/SLIC_Superpixels.pdf
/// </summary>
public class SLICSegmentation
{
    private uint _width = 1280;
    private uint _height = 720;

    // SuperPixel center at every grid interval S = sqrt(N / K).
    private float S = 0f;

    public float ResidualErrorThreshold = 1f;

    public int MaxIterations = 4;

    public int _ClusterCount = 32;

    public int Compactness = 10;

    private List<Superpixel> _superpixels;

    /// <summary>
    /// Converts RGB to XYZ.
    /// Source: http://www.easyrgb.com/index.php?X=MATH&H=02#text2
    /// </summary>
    /// <param name="rgb">The normalized RGB color.</param>
    /// <returns>Normalized XYZ color.</returns>
    public static Vector3 RGBToXYZ(Vector3 rgb)
    {
        float r = rgb.x;
        float g = rgb.y;
        float b = rgb.z;

        if (r > 0.04045)
            r = Mathf.Pow(((r + 0.055f) / 1.055f), 2.4f);
        else
            r /= 12.92f;

        if (g > 0.04045)
            g = Mathf.Pow(((g + 0.055f) / 1.055f), 2.4f);
        else
            g /= 12.92f;

        if (b > 0.04045)
            b = Mathf.Pow(((b + 0.055f) / 1.055f), 2.4f);
        else
            b /= 12.92f;

        //r = r * 100f;
        //g = g * 100f;
        //b = b * 100f;

        //Observer. = 2°, Illuminant = D65
        float R = r * 0.4124f + g * 0.3576f + b * 0.1805f;
        float G = r * 0.2126f + g * 0.7152f + b * 0.0722f;
        float B = r * 0.0193f + g * 0.1192f + b * 0.9505f;

        return new Vector3(R, G, B);
    }

    public static Vector3 XYZToCIELAB(Vector3 xyz)
    {
        float x = xyz.x*100f / 95.047f;     //ref_X =  95.047   Observer= 2°, Illuminant= D65
        float y = xyz.y*100f / 100.0f;      //ref_Y = 100.000
        float z = xyz.z*100f / 108.883f;    //ref_Z = 108.883

        if (x > 0.008856f)
            x = Mathf.Pow(x, (1 / 3f));
        else
            x = (7.787f * x) + (16 / 116f);

        if (y > 0.008856f)
            y = Mathf.Pow(y, (1 / 3f));
        else
            y = (7.787f * y) + (16 / 116f);

        if (z > 0.008856f)
            z = Mathf.Pow(z, (1 / 3f));
        else
            z = (7.787f * z) + (16 / 116f);


        float l = (116 * y) - 16;
        float a = 500 * (x - y);
        float b = 200 * (y - z);

        l /= 100f;
        a /= 100f;
        b /= 100f;

        return new Vector3(l, a, b);
    }

    public static float Distance(CIELABXY cA, CIELABXY cB, float S, out float dlab, out float dxy, int compactness = 10)
    {
        float dLAB = Mathf.Sqrt(Mathf.Pow(cA.L - cB.L, 2) + Mathf.Pow(cA.A - cB.A, 2) + Mathf.Pow(cA.B - cB.B, 2));
        float dXY = Mathf.Sqrt(Mathf.Pow(cA.X / 1280f - cB.X / 1280f, 2) + Mathf.Pow(cA.Y / 720f - cB.Y / 720f, 2));

        //float dXY = Mathf.Sqrt(Mathf.Pow(cA.X - cB.X, 2) + Mathf.Pow(cA.Y - cB.Y, 2));

        //Debug.Log("c0:" + cA.X + "x" + cA.Y + " c1:" + cB.X + "x" + cB.Y + " dLAB:" + dLAB + " dXY:" + dXY);
        dlab = dLAB;
        dxy = dXY;
        return dLAB + (compactness/S)*dXY;
        //return dLAB;
    }

    public static float Gradient(ref Vector3[,] pixels, int x, int y)
    {
        Vector3 rgbLeft = pixels[x - 1, y];
        Vector3 rgbRight = pixels[x + 1, y];

        Vector3 rgbDown = pixels[x, y - 1];
        Vector3 rgbUp = pixels[x, y + 1];

        float horizontal = Mathf.Pow((rgbRight - rgbLeft).magnitude, 2);
        float vertical = Mathf.Pow((rgbUp - rgbDown).magnitude, 2);

        return horizontal + vertical;

    }

    public static CIELABXYCenter GetAverage(List<CIELABXY> pixels5D)
    {

        CIELABXYCenter ave = new CIELABXYCenter();
        foreach (CIELABXY c in pixels5D)
        {
            ave.L += c.L;
            ave.A += c.A;
            ave.B += c.B;
            ave.X += c.X;
            ave.Y += c.Y;
        }


        ave.L /= pixels5D.Count;
        ave.A /= pixels5D.Count;
        ave.B /= pixels5D.Count;
        ave.X /= pixels5D.Count;
        ave.Y /= pixels5D.Count;
        return ave;
    }

    public void InitClusterCenters(ref Vector3[,] pixels,
        out List<CIELABXYCenter> clusterCenters,
        out List<CIELABXY> pixel5Ds)
    {
        
        clusterCenters = new List<CIELABXYCenter>();
        pixel5Ds = new List<CIELABXY>();

        // Approximate size of super pixels (N / K).
        float spSize = _width * _height /
            ((float)_ClusterCount);

        S = Mathf.Sqrt(spSize);

        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            { 
                Vector3 XYZ = RGBToXYZ(pixels[i,j] / 255f);
                Vector3 LAB = XYZToCIELAB(XYZ);

                CIELABXY c = new CIELABXY(LAB, i, j);
                c.RGB = pixels[i, j];

                pixel5Ds.Add(c);
                if ((i % (int)S == 0) && (j % (int)S == 0)){
                    clusterCenters.Add(new CIELABXYCenter(c));
                }

            }
        }

    }

    public static CIELABXY LowestGradient(ref Vector3[,] pixels,
        CIELABXY clusterCentre,
        int size = 3)
    {
        throw new NotImplementedException();
    }

    public void PertubClusterCentersToLowestGradient(ref Vector3[,] pixels, 
        ref List<CIELABXYCenter> clusterCenters,
        int size = 3)
    {
        for (int i = 0; i < clusterCenters.Count; i++)
        {
            clusterCenters[i] = new CIELABXYCenter(LowestGradient(ref pixels, clusterCenters[i]));
        }
    }

    public void AssignToNearestClusterCenter(ref List<CIELABXYCenter> clusterCenters, CIELABXY p)
    {
        int newClusterCenterIdx = -1;
        float smallestDiff = float.MaxValue;
        for (int i = 0; i < clusterCenters.Count; i++)
        {

            if (clusterCenters[i].ResidualError < ResidualErrorThreshold)
                continue;

            float dlab = 0;
            float dxy = 0;

            int curX = clusterCenters[i].X;
            int curY = clusterCenters[i].Y;
            if (p.X >= curX - S &&
                p.X <= curX + S &&
                p.Y >= curY - S &&
                p.Y <= curY + S)
            {
                float ds = Distance(clusterCenters[i], p, S, out dlab, out dxy, Compactness) + 1;
                if (ds < smallestDiff)
                {
                    smallestDiff = ds;
                    newClusterCenterIdx = i;
                }
            } // If cur pixel within search space of cur Ck
            
        } // Foreach Ck

        if (newClusterCenterIdx >= 0)
        {
            clusterCenters[newClusterCenterIdx].Region.Add(p);
            p.label = clusterCenters[newClusterCenterIdx].GetHashCode();
        }
    }


    public float ComputeNewClusterCenters(ref List<CIELABXYCenter> clusterCenters)
    {
        float residualError = 0;
        List<CIELABXYCenter> newClusterCenters = new List<CIELABXYCenter>();

        float dlabSum = 0, dxySum = 0;
        int count = 0;
        for (int i = 0; i < clusterCenters.Count; i++)
        {
            if (clusterCenters[i].ResidualError < ResidualErrorThreshold)
                continue;

            float dlab, dxy;
            float curResError = 0;
            CIELABXYCenter newClusterCenter = GetAverage(clusterCenters[i].Region);
            curResError += Distance(clusterCenters[i], newClusterCenter, S, out dlab, out dxy, Compactness);
            newClusterCenter.ResidualError = curResError;
            newClusterCenter.Region = clusterCenters[i].Region;
            clusterCenters[i] = newClusterCenter;

            dlabSum += dlab;
            dxySum += dxy;
            residualError += newClusterCenter.ResidualError;
            count++;
        }
        residualError /= count;
        return residualError;
    }

    public void EnforeConnectivity()
    {
        throw new NotImplementedException();
    }

    public void SetSuperpixels(ref List<CIELABXYCenter> clusterCenters)
    {
        _superpixels = new List<Superpixel>();
        foreach (CIELABXYCenter cc in clusterCenters)
        {
            Superpixel s = new Superpixel(cc.X, cc.Y, cc.RGB, _superpixels.Count);
            foreach (CIELABXY c in cc.Region)
            {
                s.Pixels.Add(new RegionPixel(c.X, c.Y, c.RGB));
            }
            _superpixels.Add(s);
            //Debug.Log("Superpixels " + count + ": " + _superpixels[count].Pixels.Count);
        }
        //Debug.Log("Superpixels: " + _superpixels.Count);
    }

    public List<CIELABXYCenter> RunSLICSegmentation(Vector3[,] pixels, out List<Superpixel> superpixels)
    {
        _width = (uint)pixels.GetLength(0);
        _height = (uint)pixels.GetLength(1);

        float residualError = float.MaxValue;
        List<CIELABXY> pixel5Ds;
        List<CIELABXYCenter> clusterCenters;
        InitClusterCenters(ref pixels, out clusterCenters, out pixel5Ds);
        //PertubClusterCentersToLowestGradient(imageBuffer, ref clusterCenters);

        int count = MaxIterations;
        while (count > 0)
        {
            for (int i = 0; i < pixel5Ds.Count; i++)
            {
                AssignToNearestClusterCenter(ref clusterCenters, pixel5Ds[i]);

            }
            residualError = ComputeNewClusterCenters(ref clusterCenters);
            //Debug.Log("ResidualError " + residualError);
            count--;
        }

        //EnforeConnectivity();

        SetSuperpixels(ref clusterCenters);
        superpixels = _superpixels;
        return clusterCenters;
    }

}
