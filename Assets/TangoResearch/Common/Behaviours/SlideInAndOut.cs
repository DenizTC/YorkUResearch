using UnityEngine;
using System.Collections;
using UnityEngine.UI;

/// <summary>
/// This class allows moving objects in and out using a slider.
/// </summary>
public class SlideInAndOut : MonoBehaviour {

    public Slider _Slider;
    private float x, y;

	void Start () {
        _Slider.onValueChanged.AddListener(onSliderChanged);
        x = transform.localPosition.x;
        y = transform.localPosition.y;
    }

    private void onSliderChanged(float value) {
        transform.localPosition = new Vector3(x, y, value);
    }

}
