using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System;

public class ARPointLight : ARSelectable {

    public Light _Light;
    public float _range = 3;
    public float _lightIntensity = 0.75f;
    public float _shadowIntensity = 0.75f;
    public Color _color = Color.white;

    private GUIARPointLight _ui;

    public override void Start() {
        base.Start();

        this._SelectableType = Enums.SelectionType.POINT_LIGHT;

        _ui = GUIProperties._Properties._PanelARPointLight;

        _Light.range = _range;
        _Light.intensity = _lightIntensity;
        _Light.shadowStrength = _shadowIntensity;
        _Light.color = _color;
    }

    private void OnLightIntensityChanged(float value)
    {
        _lightIntensity = value;
        _Light.intensity = _lightIntensity;
        
    }

    private void OnShadowIntensityChanged(float value)
    {
        _shadowIntensity = value;
        _Light.shadowStrength = _shadowIntensity;
    }

    private void OnRangeChanged(float value) {
        _range = value;
        _Light.range = _range;
    }

    private void OnColorChanged(Color c) {
        _color = c;
        _Light.color = _color;
        _Gizmo.material.color = c;
        _Gizmo.material.SetColor("_EmissionColor", c);
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.POINT_LIGHT);
        //_ui = PropertiesUIController._Properties._PanelARPointLight;

        _ui.Reset();

        _ui._IntensitySlider.onValueChanged.AddListener(OnLightIntensityChanged);
        _ui._ShadowIntensitySlider.onValueChanged.AddListener(OnShadowIntensityChanged);
        _ui._ColorPicker.onValueChanged.AddListener(OnColorChanged);
        _ui._RangeSlider.onValueChanged.AddListener(OnRangeChanged);
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });
        _ui._ButtonMove.onClick.AddListener(this.OnClickMove);

        _ui._IntensitySlider.value = _lightIntensity;
        _ui._ShadowIntensitySlider.value = _shadowIntensity;
        _ui._RangeSlider.value = _range;
        _ui._ColorPicker.CurrentColor = _color;
    }

}
