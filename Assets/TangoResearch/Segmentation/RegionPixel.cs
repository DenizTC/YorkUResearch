using UnityEngine;
using System.Collections;

public class RegionPixel {

    public int X;
    public int Y;
    public float R;
    public float G;
    public float B;

    public RegionPixel()
    {
    }

    public RegionPixel(int x, int y, float r, float g, float b)
    {
        X = x;
        Y = y;
        R = r;
        G = g;
        B = b;
    }

    public RegionPixel(int x, int y, Vector3 rgb) : this(x, y, rgb.x, rgb.y, rgb.z)
    {
    }

}
