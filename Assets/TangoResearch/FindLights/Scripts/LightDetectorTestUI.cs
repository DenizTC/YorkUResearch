using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;
using System.Collections.Generic;

public class LightDetectorTestUI : MonoBehaviour {

    public Button _ButtonStartFinish;
    public Text _TextStartFinish;
    private bool _running = true;

	void Start () {
        _ButtonStartFinish.onClick.AddListener(onClickStartFinish);
	}

    private void onClickStartFinish()
    {
        _running = !_running;

        _TextStartFinish.text = (_running) ? "Finish" : "Start";
        if (_running)
        {
            FindLight._LightDetector.TurnOn();
        }
        else
        {
            List<ColorPoint> cp;
            FindLight._LightDetector.TurnOff(out cp);
        }
    }
}
