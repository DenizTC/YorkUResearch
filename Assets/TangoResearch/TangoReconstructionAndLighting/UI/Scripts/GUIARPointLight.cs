using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class GUIARPointLight : GUIAR
{

    public Slider _IntensitySlider;
    public Slider _ShadowIntensitySlider;
    public Slider _RangeSlider;
    public ColorPicker _ColorPicker;

    public override void Reset()
    {
        base.Reset();
        _IntensitySlider.onValueChanged.RemoveAllListeners();
        _ShadowIntensitySlider.onValueChanged.RemoveAllListeners();
        _RangeSlider.onValueChanged.RemoveAllListeners();
        _ColorPicker.onValueChanged.RemoveAllListeners();
    }
}
