using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LightDetectorTestUI : MonoBehaviour {

    public Button _ButtonStartFinish;
    public Text _TextStartFinish;

	void Start () {
        _ButtonStartFinish.onClick.AddListener(onClickStartFinish);
	}

    private void onClickStartFinish()
    {
        if (!FindLight._LightDetector.IsRunning())
        {
            FindLight._LightDetector.TurnOn();
            _TextStartFinish.text = "Finish";
        }
        else
        {
            List<ColorPoint> cp = FindLight._LightDetector.TurnOff();
            _TextStartFinish.text = "Start";
        }
    }
}
