using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class SuperpixelMerger : MonoBehaviour {

    /// <summary>
    /// The Euclidean distance threshold.
    /// </summary>
    float Td = 0;

    /// <summary>
    /// The colorimetric distance threshold.
    /// </summary>
    float Tc = 0;

    /// <summary>
    /// The established threshold for the maximum angle between the two normal vectors.
    /// </summary>
    float Etheta = 0;

    /// <summary>
    /// Finds the nearest surface normal intersecting the ray casted from the specified screen coordinates.
    /// </summary>
    /// <param name="x">The x coordinate (screenspace).</param>
    /// <param name="y">The y coordinate (screenspace).</param>
    private Vector3 NearestSurfaceNormal(int x, int y)
    {
        throw new NotImplementedException();
    }

    private void ComputeSurfaceNormals(ref List<Superpixel> superpixels)
    {
        foreach (Superpixel s in superpixels)
        {
            s.Normal = NearestSurfaceNormal(s.X, s.Y);
        }
    }

    /// <summary>
    /// Checks if p belongs to q.
    /// </summary>
    public bool BelongsToRegion(Superpixel p, Superpixel q, ref Dictionary<int, Superpixel> labelSuperpixelPair)
    {
        if (!labelSuperpixelPair.ContainsKey(p.Label) && !labelSuperpixelPair.ContainsKey(q.Label))
            return false;

        float Dxy = Vector2.Distance(new Vector2(p.X, p.Y), new Vector2(q.X, q.Y));
        if (Dxy > Td)
            return false;

        float Dcol = Vector3.Distance(new Vector3(p.R, p.G, p.B), new Vector3(q.R, q.G, q.B));
        if (Dcol > Tc)
            return false;

        float Dnorm = Vector3.Distance(p.Normal, q.Normal);
        if (Dnorm > Etheta)
            return false;

        return true;
    }

    public List<Superpixel> MergeSuperpixels(List<Superpixel> superpixels)
    {
        ComputeSurfaceNormals(ref superpixels);

        Dictionary<int, Superpixel> labelSuperpixelPair = new Dictionary<int, Superpixel>();
        foreach (Superpixel s in superpixels)
        {
            labelSuperpixelPair.Add(s.Label, s);
        }

        foreach (Superpixel a in superpixels)
        {
            foreach (Superpixel b in superpixels)
            {
                if (BelongsToRegion(a, b, ref labelSuperpixelPair))
                {
                    labelSuperpixelPair[b.Label].Pixels.AddRange(a.Pixels);
                    labelSuperpixelPair.Remove(a.Label);
                }
            }
        }

        List<Superpixel> sp = new List<Superpixel>();
        sp.AddRange(labelSuperpixelPair.Values);
        return sp;

    }


}
