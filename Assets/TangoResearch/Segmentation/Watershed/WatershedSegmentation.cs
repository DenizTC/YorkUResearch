using UnityEngine;
using System.Collections.Generic;
using System;

public class VectorInt3
{
    public int X;
    public int Y;
    public int Z;

    public VectorInt3()
    {
    }

    public VectorInt3(int x, int y, int z)
    {
        X = x;
        Y = y;
        Z = z;
    }

    public override bool Equals(object obj)
    {
        // Check for null values and compare run-time types.
        if (obj == null || GetType() != obj.GetType())
            return false;

        VectorInt3 p = (VectorInt3)obj;
        return (X == p.X) && (Y == p.Y) && (Z == p.Z);
    }

    public override int GetHashCode()
    {
        return X ^ Y ^ Z;
        //unchecked // Overflow is fine, just wrap
        //{
        //    int hash = 17;
        //    // Suitable nullity checks etc, of course :)
        //    hash = hash * 23 + X.GetHashCode();
        //    hash = hash * 23 + Y.GetHashCode();
        //    return hash;
        //}
    }
}

public class WatershedPixel
{
    public int X;

    public int Y;

    public WatershedPixel()
    {
        X = 0;
        Y = 0;
    }

    public WatershedPixel(int x, int y)
    {
        X = x;
        Y = y;
    }

}

/// <summary>
/// Watershed gradient map superpixel segmentation.
/// Based on the paper: http://www.cogsys.cs.uni-tuebingen.de/publikationen/2015/Jiang_ROBIO15.pdf
/// </summary>
public class WatershedSegmentation {

    public static float[,] HSobelKernel = {
        { -1, 0, 1 },
        { -2, 0, 2 },
        { -1, 0, 1 }
    };

    public static float[,] VSobelKernel = {
        { -1, -2, -1 },
        { 0, 0, 0 },
        { 1, 2, 1 }
    };

    public int _BorderThreshold = 128;

    public int _ClusterCount = 32;

    public Queue<WatershedPixel>[] Q;
    public int[,] S;
    public int[,] G;

    private Dictionary<int, int> _labelIndexPair;
    private List<Superpixel> _superpixels;

    private uint _width = 1280;
    private uint _height = 720;

    public static int Gradient(ref Vector3[,] pixels, int x, int y)
    {
        return Sobel(ref pixels, x, y);
    }

    /// <summary>
    /// Finds the gradient using a 3x3 sobel operator.
    /// Source: http://cuda-programming.blogspot.ca/2013/01/sobel-filter-implementation-in-c.html
    /// </summary>
    /// <param name="pixels">The pixels array.</param>
    /// <param name="x">The x coordinate.</param>
    /// <param name="y">The y coordinate.</param>
    /// <returns></returns>
    public static int Sobel(ref Vector3[,] pixels, int x, int y)
    {
        
        // Horizontal convolution.
        float gX = 0f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xn = x + i - 1;
                int yn = y + j - 1;
                try
                {
                    gX += TangoHelpers.Grayscale(pixels[xn, yn]) * HSobelKernel[i, j];
                }
                catch (Exception)
                {
                    Debug.Log("Index error: " + xn + "x" + yn);
                    throw;
                }
                
            }
        }

        // Vertical convolution.
        float gY = 0f;
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                int xn = x + i - 1;
                int yn = y + j - 1;
                gY += TangoHelpers.Grayscale(pixels[xn, yn]) * VSobelKernel[i, j];
            }
        }

        float length = Mathf.Sqrt(Mathf.Pow(gX, 2) + Mathf.Pow(gY, 2));
        // Normalize between 0-1.
        length /= 4328.0f;
        // Normalize between 0-255.
        length *= 255f;

        return (int)length;
    }

    private bool IsBorder(int gradientValue)
    {
        return gradientValue >= _BorderThreshold;
    }

    private static int DistRGB(ref Vector3[,] pixels, VectorInt2 a, VectorInt2 b)
    {
        Vector3 p_a = pixels[a.X, a.Y];
        Vector3 p_b = pixels[b.X, b.Y];
        return (int)Math.Max(Math.Max(Math.Abs(p_a.x - p_b.x), Math.Abs(p_a.y - p_b.y)), Math.Abs(p_a.z - p_b.z));
    }

    private static int Dist4N(ref Vector3[,] pixels, int x, int y)
    {
        VectorInt2 cur = new VectorInt2(x, y);
        int left = DistRGB(ref pixels, cur, new VectorInt2(cur.X - 1, cur.Y));
        int min = left;
        int top = DistRGB(ref pixels, cur, new VectorInt2(cur.X, cur.Y + 1));
        min = (top < min) ? top : min;
        int right = DistRGB(ref pixels, cur, new VectorInt2(cur.X + 1, cur.Y));
        min = (right < min) ? right : min;
        int bottom = DistRGB(ref pixels, cur, new VectorInt2(cur.X, cur.Y - 1));
        min = (bottom < min) ? bottom : min;
        return min;
    }

    private bool adjGreaterZero(int x, int y, out int greaterZeroLabel)
    {

        greaterZeroLabel = (S[x - 1, y] > 0) ? S[x - 1, y] :
            (S[x, y + 1] > 0) ? S[x, y + 1] :
            (S[x + 1, y] > 0) ? S[x + 1, y] :
            (S[x, y - 1] > 0) ? S[x, y - 1] :
            0;


        return greaterZeroLabel != 0;
    }

    private static List<VectorInt2> adj(int x, int y)
    {
        List<VectorInt2> adjList = new List<VectorInt2>(4);
        adjList.Add(new VectorInt2(x - 1, y));
        adjList.Add(new VectorInt2(x + 1, y));
        adjList.Add(new VectorInt2(x, y + 1));
        adjList.Add(new VectorInt2(x, y - 1));
        return adjList;
    }

    private bool IsQueuesEmpty()
    {
        for (int i = 0; i < Q.Length; i++)
        {
            if (Q[i].Count > 0)
                return false;
        }
        return true;
    }

    private VectorInt2 LowestGradient(ref Vector3[,] pixels, int x, int y)
    {
        VectorInt2 result = new VectorInt2(x, y);
        int lowestGraient = int.MaxValue;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (S[x + i, y + j] == -1 || (i + j == 0) )
                    continue;
                int gradient = Gradient(ref pixels, x + i, y + i);
                if (gradient < lowestGraient)
                {
                    result.X = x + i;
                    result.Y = y + i;
                }
            }
        }

        return result;
    }

    public void SetupWatersheds(ref Vector3[,] pixels)
    {
        S = new int[_width, _height];
        G = new int[_width, _height];
        Q = new Queue<WatershedPixel>[256];
        for (int i = 0; i < 256; i++)
        {
            Q[i] = new Queue<WatershedPixel>();
        }

        // Set vertical borders = -1
        for (int i = 0; i < _height; i++)
        {
            S[0, i] = -1;
            S[_width - 1, i] = -1;
        }
        // Set horizontal borders = -1
        for (int i = 0; i < _width; i++)
        {
            S[i, 0] = -1;
            S[i, 1] = -1;
            S[i, _height - 1] = -1;

            
            //S[i, _height - 3] = -1;
        }


        // Approximate size of super pixels (N / K).
        float spSize = _width * _height / (float)_ClusterCount;

        float gridInterval = Mathf.Sqrt(spSize);

        // Sample '_ClusterCount' seeds over S using gradient values.
        _labelIndexPair = new Dictionary<int, int>();
        _superpixels = new List<Superpixel>();
        int count = 1;
        for (int i = (int)(gridInterval/2); i < _width; i+=(int)gridInterval)
        {
            for (int j = (int)(gridInterval / 2); j < _height; j+=(int)gridInterval)
            {
                if (S[i, j] != -1)
                {
                    _labelIndexPair.Add(count, _labelIndexPair.Count);

                    // Perturb to the lowest gradient in the 3x3 pixel neighborhood.
                    VectorInt2 lowestGradient = LowestGradient(ref pixels, i, j);
                    _superpixels.Add(new Superpixel(lowestGradient.X, lowestGradient.Y, pixels[lowestGradient.X, lowestGradient.Y], count));
                    S[lowestGradient.X, lowestGradient.Y] = count++;
                }
            }
        }

    }

    public void InitProcessWatershedsAndFillQueues(ref Vector3[,] pixels)
    {
        for (int i = 0; i < _width; i++)
        {
            for (int j = 0; j < _height; j++)
            {
                if (S[i, j] == 0)
                {
                    // Border pixel.
                    int g = Gradient(ref pixels, i, j);
                    G[i, j] = g;
                    if (IsBorder(g)) {
                        // New watershed border.
                        S[i, j] = -1;
                    }
                }
                int adjLabel;
                if (S[i, j] == 0 && adjGreaterZero(i, j, out adjLabel))
                {
                    // In pixel.
                    // Calculate queue index.
                    int hIndex = Dist4N(ref pixels, i, j);
                    Q[hIndex].Enqueue(new WatershedPixel(i, j));
                    //S[i, j] = -2;
                    S[i, j] = -adjLabel - 1;
                }
            } // height
        } // width
    }

    public void ProcessQueues(ref Vector3[,] pixels)
    {
        for (int h = 0; h < 256; h++)
        {
            
            while (Q[h].Count > 0)
            {
                WatershedPixel cur = Q[h].Dequeue();
                List<VectorInt2> a_i = adj(cur.X, cur.Y);
                for (int i = 0; i < a_i.Count; i++)
                {
                    if (S[a_i[i].X, a_i[i].Y] == 0)
                    {
                        int h_a_i = DistRGB(ref pixels, new VectorInt2(cur.X, cur.Y), a_i[i]);
                        // Push pixel a_i into Q[h_a_i].
                        Q[h_a_i].Enqueue(new WatershedPixel(a_i[i].X, a_i[i].Y));
                        //S[a_i[i].X, a_i[i].Y] = -2;
                        S[a_i[i].X, a_i[i].Y] = S[cur.X, cur.Y];
                    }
                    else
                    {
                        for (int j = 0; j < a_i.Count; j++)
                        {
                            if (j >= i) continue;
                            if (S[a_i[i].X, a_i[i].Y] < -1 &&
                                S[a_i[j].X, a_i[j].Y] < -1 &&
                                S[a_i[i].X, a_i[i].Y] != S[a_i[j].X, a_i[j].Y])
                            {
                                S[cur.X, cur.Y] = -1;
                            }
                        } // adjacent a_j
                    }
                } // adjacent a_i
            } // Q[h]
        } // h
    }

    /// <summary>
    /// Finds a neighbor value in S such that S[x_i, y_j] = filled.
    /// </summary>
    private int findFilledNeighbor(int x, int y)
    {
        int curIter = 1;

        while (curIter < 5)
        {
            // top
            for (int i = x - curIter; i <= x + curIter; i++)
            {
                if (i < 0) continue;
                if (i >= S.GetLength(0)) break;
                if (S[i, y+1] < -1)
                    return S[i, y+1];
            }

            // bottom
            for (int i = x - curIter; i <= x + curIter; i++)
            {
                if (i < 0) continue;
                if (i >= S.GetLength(0)) break;
                if (S[i, y - 1] < -1)
                    return S[i, y-1];
            }

            // right
            for (int i = y - curIter; i <= y + curIter; i++)
            {
                if (i < 0) continue;
                if (i >= S.GetLength(1)) break;
                if (x + 1 < 0 || x + 1 >= _width || 
                    i < 0 || i >= _height)
                {
                    Debug.Log(x + 1 + "x" + i + " - " + _width + "x" + _height + " - " + S.GetLength(0) + "x" + S.GetLength(1));
                }
                if (S[x + 1, i] < -1)
                    return S[x + 1, i];
            }

            // left
            for (int i = y - curIter; i <= y + curIter; i++)
            {
                if (i < 0) continue;
                if (i >= S.GetLength(1)) break;
                if (S[x - 1, i] < -1)
                    return S[x - 1, i];
            }

            curIter++;
        }
        return 0;
    }

    public void SetSuperpixels(ref Vector3[,] pixels)
    {
        for (int i = 1; i < S.GetLength(0) - 1; i++)
        {
            for (int j = 1; j < S.GetLength(1) - 1; j++)
            {
                if (S[i, j] < -1)
                {
                    int labelIndex = _labelIndexPair[-S[i, j] - 1];
                    _superpixels[labelIndex].Pixels.Add(new RegionPixel(i, j, pixels[i, j]));
                }
                else
                {
                    int label = -findFilledNeighbor(i, j) - 1;
                    if (label > 0)
                    {
                        int labelIndex = _labelIndexPair[-findFilledNeighbor(i, j) - 1];
                        _superpixels[labelIndex].Pixels.Add(new RegionPixel(i, j, pixels[i, j]));
                    }
                }

            }
        }
        foreach (Superpixel s in _superpixels)
        {
            s.Average();
        }
    }

    public int[,] Run(Vector3[,] pixels, out List<Superpixel> superpixels)
    {
        _width = (uint)pixels.GetLength(0);
        _height = (uint)pixels.GetLength(1);

        SetupWatersheds(ref pixels);
        InitProcessWatershedsAndFillQueues(ref pixels);
        int count = 0;
        do
        {
            ProcessQueues(ref pixels);
            count++;
        }
        while (!IsQueuesEmpty());

        SetSuperpixels(ref pixels);
        superpixels = _superpixels;
        return S;
    }

    
}
