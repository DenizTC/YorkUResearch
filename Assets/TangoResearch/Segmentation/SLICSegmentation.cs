using UnityEngine;
using System.Collections;
using System;
using Tango;
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

    public int label;
    //public List<CIELABXY> Region = new List<CIELABXY>();

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

public class SLICSegmentation : MonoBehaviour
{

    // SuperPixel center at every grid interval S = sqrt(N / K).
    private float S = 0f;

    public static Vector3 RGBToCIELAB(Vector3 rgb)
    {
        throw new NotImplementedException();
    }

    public static float Distance(CIELABXY cA, CIELABXY cB, float S, out float dlab, out float dxy, int compactness = 10)
    {
        float dLAB = Mathf.Sqrt(Mathf.Pow(cA.L - cB.L, 2) + Mathf.Pow(cA.A - cB.A, 2) + Mathf.Pow(cA.B - cB.B, 2));
        float dXY = Mathf.Sqrt(Mathf.Pow(cA.X/1280f - cB.X/1280f, 2) + Mathf.Pow(cA.Y/720f - cB.Y/720f, 2));

        //Debug.Log("c0:" + cA.X + "x" + cA.Y + " c1:" + cB.X + "x" + cB.Y + " dLAB:" + dLAB + " dXY:" + dXY);
        dlab = dLAB;
        dxy = dXY;
        return dLAB + (compactness/S)*dXY;
        //return dLAB;
    }

    public static float Gradient(Texture2D src, int x, int y)
    {
        throw new NotImplementedException();
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

    public void InitClusterCenters(TangoUnityImageData imageBuffer,
        out List<CIELABXYCenter> clusterCenters,
        out List<CIELABXY> pixel5Ds,
        int superPixelCount = 32,
        int resDiv = 8)
    {
        
        clusterCenters = new List<CIELABXYCenter>();
        pixel5Ds = new List<CIELABXY>();

        // Approximate size of super pixels (N / K).
        float spSize = (imageBuffer.width / (float)resDiv) * 
            (imageBuffer.height / (float)resDiv) /
            ((float)superPixelCount);

        S = Mathf.Sqrt(spSize);
        //Debug.Log("S: " + S);

        int wS = (int)(imageBuffer.width / (float)resDiv);
        int hS = (int)(imageBuffer.height / (float)resDiv);

        for (int i = 0; i < hS; i++)
        {
            for (int j = 0; j < wS; j++)
            {
                int iS = i * resDiv;
                int jS = j * resDiv;

                Vector3 yuv = TangoHelpers.GetYUV(imageBuffer, jS, iS);
                Vector3 rgb = TangoHelpers.YUVToRGB(yuv);

                pixel5Ds.Add(new CIELABXY(rgb, jS, iS));
                if ((i % (int)S == 0) && (j % (int)S == 0)){
                    clusterCenters.Add(new CIELABXYCenter(rgb, jS, iS));
                }

            }
        }

        //Debug.Log("Pixel count: " + pixel5Ds.Count + " Ck count: " + clusterCenters.Count);

    }

    public static CIELABXY LowestGradient(TangoUnityImageData imageBuffer,
        CIELABXY clusterCentre,
        int resDiv = 8, 
        int size = 3)
    {
        throw new NotImplementedException();
    }

    public void PertubClusterCentersToLowestGradient(TangoUnityImageData imageBuffer, 
        ref List<CIELABXYCenter> clusterCenters, 
        int resDiv = 8,
        int size = 3)
    {
        for (int i = 0; i < clusterCenters.Count; i++)
        {
            clusterCenters[i] = new CIELABXYCenter(LowestGradient(imageBuffer, clusterCenters[i]));
        }
    }

    public void AssignToNearestClusterCenter(ref List<CIELABXYCenter> clusterCenters,
        CIELABXY p,
        out float dlabSmallest,
        int resDiv = 8)
    {
        dlabSmallest = 0;
        int newClusterCenterIdx = -1;
        float smallestDiff = float.MaxValue;
        for (int i = 0; i < clusterCenters.Count; i++)
        {

            float dlab = 0;
            float dxy = 0;

            int curX = clusterCenters[i].X;
            int curY = clusterCenters[i].Y;
            if (p.X >= curX - S * resDiv &&
                p.X <= curX + S * resDiv &&
                p.Y >= curY - S * resDiv &&
                p.Y <= curY + S * resDiv)
            {
                float ds = Distance(clusterCenters[i], p, S, out dlab, out dxy) + 1;
                if (ds < smallestDiff)
                {
                    smallestDiff = ds;
                    newClusterCenterIdx = i;
                }
                dlabSmallest = ds;
            } // If cur pixel within search space of cur Ck
            
        } // Foreach Ck
        //Debug.Log("Smallest diff: " + dst);
        if (newClusterCenterIdx >= 0)
        {
            clusterCenters[newClusterCenterIdx].Region.Add(p);
            p.label = clusterCenters[newClusterCenterIdx].GetHashCode();
            dlabSmallest = smallestDiff;
        }
        else
        {
            //Debug.Log("Smallest diff: " + smallestDiff + " ds: " + dlabSmallest);

        }
    }


    public float ComputeNewClusterCenters(ref List<CIELABXYCenter> clusterCenters)
    {
        float residualError = 0;
        List<CIELABXYCenter> newClusterCenters = new List<CIELABXYCenter>();

        float dlabSum = 0, dxySum = 0;
        for (int i = 0; i < clusterCenters.Count; i++)
        {
            float dlab, dxy;
            CIELABXYCenter newClusterCenter = GetAverage(clusterCenters[i].Region);
            residualError += Distance(clusterCenters[i], newClusterCenter, S, out dlab, out dxy);
            newClusterCenter.Region = clusterCenters[i].Region;
            clusterCenters[i] = newClusterCenter;

            dlabSum += dlab;
            dxySum += dxy;
        }

        //dlabSum /= clusterCenters.Count;
        //dxySum /= clusterCenters.Count;
        //residualError /= clusterCenters.Count;
        //Debug.Log("Center count:" + clusterCenters.Count + " dlab:" + dlabSum + " dxy:" + dxySum + " residualError:" + residualError);
        return residualError;
    }

    public void EnforeConnectivity()
    {
        throw new NotImplementedException();
    }

    public List<CIELABXYCenter> RunSLICSegmentation(TangoUnityImageData imageBuffer, 
        float residualErrorThreshold, 
        int resDiv = 8,
        int clusterCount = 32)
    {
        float residualError = 0;
        List<CIELABXY> pixel5Ds;
        List<CIELABXYCenter> clusterCenters;
        InitClusterCenters(imageBuffer, out clusterCenters, out pixel5Ds, clusterCount, resDiv);
        //PertubClusterCentersToLowestGradient(imageBuffer, ref clusterCenters);

        int count = 4;
        do
        {
            float dlabSmallestSum = 0;
            for (int i = 0; i < pixel5Ds.Count; i++)
            {
                AssignToNearestClusterCenter(ref clusterCenters, pixel5Ds[i], out dlabSmallestSum, resDiv);

            }
            
            residualError = ComputeNewClusterCenters(ref clusterCenters);
            Debug.Log("ResidualError " + residualError);
            count--;
        } while (residualError > residualErrorThreshold);

        //EnforeConnectivity();

        return clusterCenters;
    }

}
