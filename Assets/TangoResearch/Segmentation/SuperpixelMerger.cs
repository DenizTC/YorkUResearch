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

    private void ComputePixelDistanceThreshold()
    {
        throw new NotImplementedException();
    }

    private void ComputeColorDistanceThreshold()
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Checks if p belongs to q.
    /// </summary>
    public bool BelongsToRegion(Superpixel p, Superpixel q)
    {
        if (p.Label == q.Label)
            return false;

        float Dxy = Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(q.X, q.Y));
        if (Input.GetKeyDown(KeyCode.Space))
        {

            Debug.Log(Dxy);
        }


        if (Dxy > Td)
            return false;

        float Dcol = Vector3.Distance(new Vector3(p.R, p.G, p.B), new Vector3(q.R, q.G, q.B));
        if (Dcol > Tc)
            return false;

        

        return true;

        float Dnorm = Vector3.Distance(p.Normal, q.Normal);
        if (Dnorm > Etheta)
            return false;

        return true;
    }

    /// <summary>
    /// Checks if p belongs to q.
    /// </summary>
    public bool BelongsToRegion(Superpixel p, Superpixel q, ref Dictionary<int, Superpixel> labelSuperpixelPair)
    {
        if (!labelSuperpixelPair.ContainsKey(p.Label) && !labelSuperpixelPair.ContainsKey(q.Label))
            return false;

        if (p.Label == q.Label)
            return false;
           
        float Dxy = Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(q.X, q.Y));
        if (Dxy > Td)
            return false;

        float Dcol = Vector3.Distance(new Vector3(p.R, p.G, p.B), new Vector3(q.R, q.G, q.B));
        if (Dcol > Tc)
            return false;

        if (Input.GetMouseButtonDown(1))
        {
            Debug.Log(Dxy + " " + Dcol);
        }

        return true;

        float Dnorm = Vector3.Distance(p.Normal, q.Normal);
        if (Dnorm > Etheta)
            return false;

        return true;
    }

    public List<Superpixel> MergeSuperpixels(List<Superpixel> superpixels)
    {
        ComputeSurfaceNormals(ref superpixels);
        
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
                if (BelongsToRegion(bCur, aCur))
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
