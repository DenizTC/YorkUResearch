using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Multiplies the scaleFactor of the canvas by a specified amount if running on an android device.
/// </summary>
[RequireComponent(typeof(Canvas))]
public class AndroidScaleFactorController : MonoBehaviour {

    public float _Scale = 3.0f;
    private CanvasScaler _scaler;

	void Start () {

#if UNITY_ANDROID && !UNITY_EDITOR
        _scaler = GetComponent<CanvasScaler>();
        _scaler.scaleFactor *= _PercentWidth;
#endif

    }

}
