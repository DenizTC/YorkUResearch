using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class AdjustAmbientIntensity : MonoBehaviour {

    public Slider _Slider;

	// Use this for initialization
	void Start () {
        _Slider.onValueChanged.AddListener(onValueChangedAmbient);
	}

    private void onValueChangedAmbient(float value)
    {
        RenderSettings.ambientIntensity = value;
        DynamicGI.UpdateEnvironment();
    }

    // Update is called once per frame
    void Update () {
	
	}
}
