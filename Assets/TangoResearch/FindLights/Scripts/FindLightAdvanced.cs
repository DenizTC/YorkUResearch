using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindLightAdvanced : MonoBehaviour {

    public Vector3 _EstimatedLightPos = Vector3.zero;
    public float[,,] _LightErrorGrid;

    public void SetupLightErrorGrid(int size = 5)
    {
        _LightErrorGrid = new float[size, size, size];
    }

    public Vector3 LightEstimation(ref List<Superpixel> superpixels, int textureWidth, int textureHeight)
    {
        VectorInt3 minError = new VectorInt3(0, 0, 0);
        Vector3 minLightPos = Vector3.zero;
        float[] Io = new float[superpixels.Count];

        for (int i = 0; i < superpixels.Count; i++)
        {
            Io[i] = superpixels[i].Intensity / 255f;
        }

        for (int x = 0; x < _LightErrorGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _LightErrorGrid.GetLength(1); y++)
            {
                for (int z = 0; z < _LightErrorGrid.GetLength(2); z++)
                {

                    Vector3 lightPos = Camera.main.transform.TransformPoint(
                       x - _LightErrorGrid.GetLength(0) / 2f,
                       y - _LightErrorGrid.GetLength(1) / 2f,
                       z - _LightErrorGrid.GetLength(2) / 2f);

                    float error = IoIrL2Norm(ref superpixels, Io, lightPos, textureWidth, textureHeight);
                    _LightErrorGrid[x, y, z] = error;

                    if (error < _LightErrorGrid[minError.X, minError.Y, minError.Z])
                    {
                        minError = new VectorInt3(x, y, z);
                        minLightPos = lightPos;
                    }


                }
            }
        }

        Debug.Log("LightPos: " + minLightPos + " error: " + _LightErrorGrid[minError.X, minError.Y, minError.Z]);
        return minLightPos;
    }

    public Vector3 LightEstimation(ref List<RegionPixel> pixels, int textureWidth, int textureHeight)
    {
        VectorInt3 minError = new VectorInt3(0, 0, 0);
        Vector3 minLightPos = Vector3.zero;
        float[] Io = new float[pixels.Count];

        for (int i = 0; i < pixels.Count; i++)
        {
            Io[i] = pixels[i].Intensity / 255f;
        }

        for (int x = 0; x < _LightErrorGrid.GetLength(0); x++)
        {
            for (int y = 0; y < _LightErrorGrid.GetLength(1); y++)
            {
                for (int z = 0; z < _LightErrorGrid.GetLength(2); z++)
                {

                    Vector3 lightPos = Camera.main.transform.TransformPoint(
                        x - _LightErrorGrid.GetLength(0) / 2f,
                        y - _LightErrorGrid.GetLength(1) / 2f,
                        z - _LightErrorGrid.GetLength(2) / 2f);

                    float error = IoIrL2Norm(ref pixels, Io, lightPos, textureWidth, textureHeight);
                    _LightErrorGrid[x, y, z] = error;

                    if (error < _LightErrorGrid[minError.X, minError.Y, minError.Z])
                    {
                        minError = new VectorInt3(x, y, z);
                        minLightPos = lightPos;
                    }


                }
            }
        }

        Debug.Log("LightPos: " + minLightPos + " error: " + _LightErrorGrid[minError.X, minError.Y, minError.Z]);
        return minLightPos;
    }


    public static float IoIrL2Norm(ref List<Superpixel> superpixels, float[] Io, Vector3 lightPos, int textureWidth, int textureHeight)
    {
        float[] Ir = new float[superpixels.Count];

        float dist = 0;
        for (int i = 0; i < superpixels.Count; i++)
        {
            if (superpixels[i].Normal.magnitude <= 0)
            {
                continue;
            }

            float ns = 0;
            float albedo = 0;
            float ir = 0;
            Vector3 lightDir = ImageProcessing.LightDirection(lightPos, superpixels[i].WorldPoint);
            ImageProcessing.ComputeAlbedo(Io[i], superpixels[i].Normal, lightDir, out ns, out albedo);
            if (ns > 0)
            {
                ImageProcessing.ComputeImageIntensity(albedo, superpixels[i].Normal, lightDir, out ir);
            }
            else
            {
                if (!superpixels[i].GetMedianSynthesizedIr(textureWidth, textureHeight, lightPos, out albedo, out ir))
                {
                    ir = Io[i];
                }
            }


            Ir[i] = ir;
            dist += Mathf.Pow(Io[i] - Ir[i], 2);
        }
        dist = Mathf.Pow(dist, 0.5f);

        //if (_debuggingLightPos)
        //{
        //    Debug.Log("IoIr: " + dist);
        //}

        return dist;
    }

    public static float IoIrL2Norm(ref List<RegionPixel> pixels, float[] Io, Vector3 lightPos, int textureWidth, int textureHeight)
    {
        float[] Ir = new float[pixels.Count];

        float dist = 0;
        for (int i = 0; i < pixels.Count; i++)
        {
            if (pixels[i].Normal.magnitude <= 0)
            {
                continue;
            }

            float ns = 0;
            float albedo = 0;
            float ir = 0;
            Vector3 lightDir = ImageProcessing.LightDirection(lightPos, pixels[i].WorldPoint);
            ImageProcessing.ComputeAlbedo(Io[i], pixels[i].Normal, lightDir, out ns, out albedo);

            if (ns > 0)
            {
                if (albedo > 2.5)
                {
                    continue;
                }
                ImageProcessing.ComputeImageIntensity(albedo, pixels[i].Normal, lightDir, out ir);
            }
            else
            {
                ir = Io[i];
            }

            Ir[i] = ir;
            dist += Mathf.Pow(Io[i] - Ir[i], 2);
        }
        dist = Mathf.Pow(dist, 0.5f);

        //if (_debuggingLightPos)
        //{
        //    Debug.Log("IoIr: " + dist);
        //}

        return dist;
    }


}
