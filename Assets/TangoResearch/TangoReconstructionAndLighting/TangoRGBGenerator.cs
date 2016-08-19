using UnityEngine;
using System.Collections.Generic;

public class TangoRGBGenerator : MonoBehaviour {

    public RenderTexture _TangoRGBTexture;

    public static TangoRGBGenerator _TangoRGBGenerator;

    void Awake() {
        _TangoRGBTexture = FindObjectOfType<TangoARScreen>().transform.GetComponent<Camera>().targetTexture;
    }

}
