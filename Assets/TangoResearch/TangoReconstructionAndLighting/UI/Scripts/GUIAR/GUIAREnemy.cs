using UnityEngine;
using System.Collections;
using System;
using UnityEngine.UI;

public class GUIAREnemy : GUIAR
{

    public Slider _Speed;
    public Slider _Size;

    public override void Reset()
    {
        base.Reset();
        _ButtonMove.interactable = false;

        _Speed.onValueChanged.RemoveAllListeners();
        _Size.onValueChanged.RemoveAllListeners();
    }

}
