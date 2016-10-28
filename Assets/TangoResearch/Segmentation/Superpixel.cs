using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Superpixel : RegionPixel {

    public int Label;

    public Vector3 Normal;

    public List<RegionPixel> Pixels = new List<RegionPixel>();

    public Superpixel(int x, int y, Vector3 rgb, int label) : base(x, y, rgb)
    {
        Label = label;
    }

    public Superpixel(int x, int y, Vector3 rgb, Vector3 normal, int label) : base(x, y, rgb)
    {
        Normal = normal;
        Label = label;
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
