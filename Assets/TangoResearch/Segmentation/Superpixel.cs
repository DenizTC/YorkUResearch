using UnityEngine;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

public class Superpixel : RegionPixel {

    public int Label;
    public Vector3 HSV;    

    public List<RegionPixel> Pixels = new List<RegionPixel>();

    public Superpixel() : base() { }

    public Superpixel(int label) : this()
    {
        this.Label = label;
    }

    public Superpixel(int x, int y, Vector3 rgb, int label) : base(x, y, rgb)
    {
        Label = label;
    }

    public Superpixel(int x, int y, Vector3 rgb, Vector3 normal, int label) : base(x, y, rgb)
    {
        Normal = normal;
        Label = label;
    }

    public Vector3 GetAverageNormal(int textureWidth, int textureHeight)
    {
        Vector3 ave = Vector3.zero;
        foreach (RegionPixel r in Pixels)
        {
            r.ComputeSurfaceNormal(textureWidth, textureHeight);
            ave += r.Normal;
        }
        ave /= (float)Pixels.Count;
        return ave;
    }

    //public float GetMedianNS(int textureWidth, int textureHeight, Vector3 lightPos)
    //{
    //    if (Pixels.Count <= 0) return 0;

    //    int count = 0;
    //    int nCount = 0;
    //    List<float> NS = new List<float>(Pixels.Count);
    //    foreach (RegionPixel r in Pixels)
    //    {
    //        r.ComputeImageIntensity(); // 255
    //        if (!r.ComputeSurfaceNormal(textureWidth, textureHeight))
    //        {
    //            continue;
    //        }
    //        nCount++;

    //        Vector3 lightDir = ImageProcessing.LightDirection(lightPos, r.WorldPoint);
    //        float ns = Vector3.Dot(r.Normal, lightDir);
    //        float ir = 0;
    //        r.ComputeAlbedo(lightPos); // 255

    //        if (ns <= 0)
    //        {
    //            continue;
    //        }
    //        NS.Add(ns);

    //        count++;
    //    }
    //    if (NS.Count <= 0) return 0;

    //    NS.Sort();

    //    float result = NS[count / 2];
    //    //Debug.LogError("Pixels: " + Pixels.Count + " nCount: " + nCount + " Irs count: " + Irs.Count + " index: " + (count / 2));
    //    //Debug.Log("Irs: " + result);
    //    return result; // 255
    //}

    public bool GetMedianSynthesizedIr(int textureWidth, int textureHeight, Vector3 lightPos, out float albedo, out float Ir)
    {
        albedo = 0;
        Ir = 0;
        if (Pixels.Count <= 0) return false;

        int count = 0;
        int nCount = 0;
        List<Vector2> Irs = new List<Vector2>(Pixels.Count);
        foreach (RegionPixel r in Pixels)
        {
            r.ComputeImageIntensity(); // 255
            if (!r.ComputeSurfaceNormal(textureWidth, textureHeight)) continue;
            nCount++;

            Vector3 lightDir = ImageProcessing.LightDirection(lightPos, r.WorldPoint);
            float ns = Vector3.Dot(r.Normal, lightDir);
            ImageProcessing.ComputeAlbedo(r.Intensity / 255f, ns, out albedo);
            //r.ComputeAlbedo(lightPos); // 1

            if (ns <= 0) continue;

            float ir = 0;
            ImageProcessing.ComputeImageIntensity(albedo, ns, out ir);
            Irs.Add(new Vector2(ir, albedo));

            count++;
        }
        if (Irs.Count <= 0) return false;

        Irs.OrderBy(v => v.x);

        Ir = Irs[count / 2].x;
        albedo = Irs[count / 2].y;

        //Debug.Log("Median - Ir: " + result + " albedo: " + albedo);

        return true;
    }

    public void Average()
    {
        if (Pixels.Count <= 0)
            return;

        Vector3 color = Vector3.zero;
        float x = 0;
        float y = 0;

        foreach (RegionPixel p in Pixels)
        {
            color += new Vector3(p.R, p.G, p.B);
            x += p.X;
            y += p.Y;
        }

        color /= Pixels.Count;
        x /= Pixels.Count;
        y /= Pixels.Count;

        X = (int)x;
        Y = (int)y;
        R = color.x;
        G = color.y;
        B = color.z;

    }

    

}
