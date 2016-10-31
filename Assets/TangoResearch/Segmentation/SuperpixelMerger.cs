using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SuperpixelMerger : MonoBehaviour {

    /// <summary>
    /// The Euclidean distance threshold.
    /// </summary>
    public float Td = 0;

    /// <summary>
    /// The colorimetric distance threshold.
    /// </summary>
    public float Tc = 0;

    /// <summary>
    /// The established threshold for the maximum angle between the two normal vectors.
    /// </summary>
    public float Etheta = 0;

    /// <summary>
    /// Finds the surface normal intersecting the ray casted from the specified screen coordinates.
    /// </summary>
    /// <param name="x">The x coordinate (screenspace).</param>
    /// <param name="y">The y coordinate (screenspace).</param>
    private bool SurfaceNormal(int x, int y, out Vector3 normal)
    {
        normal = Vector3.zero;
        RaycastHit hit;
        Ray ray = Camera.main.ScreenPointToRay(new Vector2(x, y));
        if (Physics.Raycast(ray, out hit))
        {
            normal = hit.normal;
            return true;
        }
        
        return false;

    }

    private void ComputeSurfaceNormals(ref List<Superpixel> superpixels)
    {
        foreach (Superpixel s in superpixels)
        {
            Vector3 normal;
            SurfaceNormal(s.X, s.Y, out normal);
            s.Normal = normal;
        }
    }

    private void ComputeHSVs(ref List<Superpixel> superpixels)
    {
        foreach (Superpixel s in superpixels)
        {
            float H, S, V;
            Color.RGBToHSV(new Color(s.R/255f, s.G/255f, s.B/255f), out H, out S, out V);
            s.HSV = new Vector3(H, S, V);
        }
    }

    private float ComputePixelDistanceThreshold(ref List<Superpixel> superpixels, Superpixel p)
    {
        float ave = 0;

        int count = 0;
        foreach (Superpixel s in superpixels)
        {
            if (s.Label != p.Label)
            {
                ave += PixelDistance(p, s);
                count++;
            }
        }

        ave /= count;
        return ave;
    }

    private float ComputeColorDistanceThreshold(ref List<Superpixel> superpixels, Superpixel p)
    {
        float ave = 0;

        int count = 0;
        foreach (Superpixel s in superpixels)
        {
            if (s.Label != p.Label)
            {
                ave += ColorDistance(p, s);
                count++;
            }
        }

        ave /= count;
        return ave;
    }

    private float PixelDistance(Superpixel p, Superpixel q)
    {
        return Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(q.X, q.Y));
    }

    private float ColorDistance(Superpixel p, Superpixel q)
    {
        float Dcol = Mathf.Pow((p.HSV.y * p.HSV.z * Mathf.Cos(p.HSV.x * Mathf.Deg2Rad)) - (q.HSV.y * q.HSV.z * Mathf.Cos(q.HSV.x * Mathf.Deg2Rad)), 2) +
            Mathf.Pow((p.HSV.y * p.HSV.z * Mathf.Sin(p.HSV.x * Mathf.Deg2Rad)) - (q.HSV.y * q.HSV.z * Mathf.Sin(q.HSV.x * Mathf.Deg2Rad)), 2) +
            Mathf.Pow(p.HSV.z - q.HSV.z, 2);
        Dcol = Mathf.Pow(Dcol, 0.5f);
        Dcol *= 100;
        return Dcol;
    }

    /// <summary>
    /// Checks if p belongs to q.
    /// </summary>
    public bool BelongsToRegion(ref List<Superpixel> superpixels, Superpixel p, Superpixel q)
    {
        if (p.Label == q.Label)
            return false;

        float Dxy = PixelDistance(p, q);
        Td = ComputePixelDistanceThreshold(ref superpixels, q);
        if (Dxy > Td)
            return false;

        float Dcol = ColorDistance(p, q);
        Tc = ComputeColorDistanceThreshold(ref superpixels, q);
        if (Dcol > Tc)
            return false;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log((int)Dxy + " " + Dcol);
            //Debug.Log("Phsv: " + p.HSV.x + ":" + p.HSV.y + ":" + p.HSV.z + " Qhsv: " + q.HSV.x + ":" + q.HSV.y + ":" + q.HSV.z);
        }
                
        //float Dnorm = Vector3.Distance(p.Normal, q.Normal);
        //if (Dnorm > Etheta)
        //    return false;

        return true;
    }

    public List<Superpixel> MergeSuperpixels(List<Superpixel> superpixels)
    {
        ComputeSurfaceNormals(ref superpixels);
        ComputeHSVs(ref superpixels);


        Stack<Superpixel> a = new Stack<Superpixel>();
        Stack<Superpixel> b = new Stack<Superpixel>();
        foreach (Superpixel s in superpixels)
        {
            a.Push(s);
        }
        
        List<Superpixel> sp = new List<Superpixel>();
        while (a.Count > 0)
        {
            Superpixel aCur = a.Pop();

            while (a.Count > 0)
            {
                Superpixel bCur = a.Pop();
                if (BelongsToRegion(ref superpixels, bCur, aCur))
                {
                    aCur.Pixels.AddRange(bCur.Pixels);
                }
                else
                {
                    b.Push(bCur);
                }
                
            }

            sp.Add(aCur);

            while (b.Count > 0)
            {
                a.Push(b.Pop());
            }

        }
        
        return sp;

    }


}
