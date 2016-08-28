using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GUIARProp : GUIAR
{

    public bool _UseGravity = true;
    public Slider _SliderMetallic;
    public Slider _SliderSmoothness;
    public Toggle _TogglePhysics;

    public override void Reset()
    {
        base.Reset();
        _SliderMetallic.onValueChanged.RemoveAllListeners();
        _SliderSmoothness.onValueChanged.RemoveAllListeners();
        _TogglePhysics.onValueChanged.RemoveAllListeners();
    }

}
