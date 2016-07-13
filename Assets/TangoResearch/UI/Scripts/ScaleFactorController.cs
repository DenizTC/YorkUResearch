using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScaleFactorController : MonoBehaviour {

    public float _PercentWidth = 3.0f;

    private float _initScale = 1;
    private CanvasScaler _scaler;

	void Start () {
        _scaler = GetComponent<CanvasScaler>();
        _scaler.scaleFactor *= _PercentWidth;
	}
	
}
