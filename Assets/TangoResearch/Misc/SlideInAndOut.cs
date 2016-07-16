using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class SlideInAndOut : MonoBehaviour {

    public Slider _Slider;
    private float x, y;

	// Use this for initialization
	void Start () {
        _Slider.onValueChanged.AddListener(onSliderChanged);
        x = transform.localPosition.x;
        y = transform.localPosition.y;
    }
	
	// Update is called once per frame
	void Update () {
	
	}

    private void onSliderChanged(float value) {
        transform.localPosition = new Vector3(x, y, value);
    }

}
