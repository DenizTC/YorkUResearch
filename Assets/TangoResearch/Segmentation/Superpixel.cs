using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Superpixel : RegionPixel {

    public int Label;
    public float Intensity;
    public Vector3 HSV;
    public Vector3 Normal;
    public Vector3 WorldPoint;
    

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

    public void ComputeImageIntensity()
    {
        this.Intensity = ImageProcessing.Grayscale(new Vector3(this.R, this.G, this.B));
    }

    public void ComputeSurfaceNormal(int textureWidth, int textureHeight)
    {
        Normal = Vector3.zero;
        RaycastHit hit;
        float x = this.X * (Camera.main.pixelWidth / (float)textureWidth);
        float y = Camera.main.pixelHeight - this.Y * (Camera.main.pixelHeight / (float)textureHeight);
        //float x = X;
        //float y = Y;
        //Debug.Log(Input.mousePosition + " - " + x + "x" + y);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        if (Physics.Raycast(ray, out hit, 10, 1 << GameGlobals.WalkableLayer))
        {
            //Debug.DrawRay(ray.origin, ray.direction, Color.blue);
            Normal = hit.normal;
            WorldPoint = hit.point;
        }
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
