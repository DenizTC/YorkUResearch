using UnityEngine;
using System.Collections;

public class RegionPixel {

    public int X;
    public int Y;
    public float R;
    public float G;
    public float B;

    public float Intensity;
    public float Albedo;
    public Vector3 Normal;
    public Vector3 WorldPoint;

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

    public void ComputeImageIntensity()
    {
        this.Intensity = ImageProcessing.Grayscale(new Vector3(this.R, this.G, this.B));
    }

    public bool ComputeSurfaceNormal(int textureWidth, int textureHeight)
    {
        Normal = Vector3.zero;
        RaycastHit hit;
        float x = this.X * (Camera.main.pixelWidth / (float)textureWidth);
        float y = Camera.main.pixelHeight - this.Y * (Camera.main.pixelHeight / (float)textureHeight);
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(x, y, 0));
        if (Physics.Raycast(ray, out hit, 10, 1 << GameGlobals.WalkableLayer))
        {
            Normal = Vector3.Normalize(hit.normal);
            //Normal = hit.normal;
            WorldPoint = hit.point;
            return true;
        }
        return false;
    }

    public bool ComputeAlbedo(Vector3 lightPos)
    {
        Vector3 lightDir = ImageProcessing.LightDirection(lightPos, WorldPoint);
        return ImageProcessing.ComputeAlbedo(Intensity/255f, Normal, lightDir, out Albedo);
    }


}
