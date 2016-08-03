using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class FrameDisplay : MonoBehaviour {

    private Text _text;

    private float deltaTime = 0.0f;

    void Start() {
        _text = GetComponent<Text>();
    }

    void Update()
    {
        deltaTime += (Time.deltaTime - deltaTime) * 0.1f;

        float msec = deltaTime * 1000.0f;
        float fps = 1.0f / deltaTime;
        string text = string.Format("{0:0.0} fps", fps);

        _text.text = text;

    }

}
