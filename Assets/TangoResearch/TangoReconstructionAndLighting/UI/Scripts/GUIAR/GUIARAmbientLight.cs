using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class GUIARAmbientLight : GUIAR {

    public Slider _IntensitySlider;
    public ColorPicker _ColorPicker;

    public override void Reset()
    {
        base.Reset();
        _DestroyButton.interactable = false;
        _ButtonMove.interactable = false;

        _IntensitySlider.onValueChanged.RemoveAllListeners();
        _ColorPicker.onValueChanged.RemoveAllListeners();
    }

}
