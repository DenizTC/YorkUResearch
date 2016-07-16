using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class TangoDepthMapGUIController : MonoBehaviour {

    public Button _SwitchBGButton;
    public CustomZBufferEffect _CustorZBuffer;

    private Text _switchBGText;
    private bool _rgbMode = true;

    private void Start() {
        _switchBGText = _SwitchBGButton.GetComponentInChildren<Text>();
        _SwitchBGButton.onClick.AddListener(onSwitchBGClick);
    }

    private void onSwitchBGClick() {
        _rgbMode = !_rgbMode;
        float val = (_rgbMode) ? 1 : 0;
        string text = (_rgbMode) ? "Color" : "Depth";

        _switchBGText.text = text;
        _CustorZBuffer._material.SetFloat("_RGBMode", val);
    }


}
