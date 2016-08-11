using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIARDirectionalLight : GUIAR {

    public Slider _IntensitySlider;
    public Slider _ShadowIntensitySlider;
    public ColorPicker _ColorPicker;
    public Button _AimSunButton;

    private Text _aimSunText;

    void Start() {
        _aimSunText = _AimSunButton.transform.GetChild(0).GetComponent<Text>();
    }

    public override void Reset()
    {
        base.Reset();
        _DestroyButton.interactable = false;

        _IntensitySlider.onValueChanged.RemoveAllListeners();
        _ShadowIntensitySlider.onValueChanged.RemoveAllListeners();
        _ColorPicker.onValueChanged.RemoveAllListeners();
    }

    public void ToggleAimSunButtonText() {
        _aimSunText.text = (GameGlobals.IsAimingSun) ? "Finish" : "Aim Sun";
    }

}
