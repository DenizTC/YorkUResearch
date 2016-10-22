using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Superpixel : RegionPixel {

    public Vector3 Normal;

    public List<RegionPixel> Pixels = new List<RegionPixel>();

    public Superpixel(int x, int y, Vector3 rgb) : base(x, y, rgb)
    {
    }

    public Superpixel(int x, int y, Vector3 rgb, Vector3 normal) : base(x, y, rgb)
    {
        Normal = normal;
    }
}
