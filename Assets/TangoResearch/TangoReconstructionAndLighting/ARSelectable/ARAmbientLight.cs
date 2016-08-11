﻿using UnityEngine;
using System.Collections;

public class ARAmbientLight : ARSelectable {

    public static ARAmbientLight _SceneAmbientLight;

    public float _intensity = 1f;
    public Color _color = new Color(0.21f,0.22f,0.26f,1);

    private GUIARAmbientLight _ui;

    void Start()
    {
        if (!_SceneAmbientLight)
            _SceneAmbientLight = this;

        _ui = GUIProperties._Properties._PanelARAmbientLight;

        RenderSettings.ambientIntensity = _intensity;
        RenderSettings.ambientLight = _color;
    }

    private void onIntensityChanged(float value)
    {
        _intensity = value;
        RenderSettings.ambientIntensity = _intensity;

    }

    private void onColorChanged(Color c)
    {
        _color = c;
        RenderSettings.ambientLight = _color;
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.AMBIENT_LIGHT);

        _ui.Reset();

        _ui._IntensitySlider.onValueChanged.AddListener(onIntensityChanged);
        _ui._ColorPicker.onValueChanged.AddListener(onColorChanged);
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });

        _ui._IntensitySlider.value = _intensity;
        _ui._ColorPicker.CurrentColor = _color;
    }

}