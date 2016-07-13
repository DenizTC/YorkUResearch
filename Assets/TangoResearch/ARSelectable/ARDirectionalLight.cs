using UnityEngine;
using System.Collections;

public class ARDirectionalLight : ARSelectable {

    public static ARDirectionalLight _Sun;

    public Light _Light;
    public float _lightIntensity = 0.75f;
    public float _shadowIntensity = 0.75f;
    public Color _color = Color.white;

    private GUIARDirectionalLight _ui;

    void Start()
    {
        if (!_Sun)
            _Sun = this;

        _ui = GUIProperties._Properties._PanelARDirectionalLight;

        _Light.intensity = _lightIntensity;
        _Light.shadowStrength = _shadowIntensity;
        _Light.color = _color;
    }

    private void onLightIntensityChanged(float value)
    {
        _lightIntensity = value;
        _Light.intensity = _lightIntensity;

    }

    private void onShadowIntensityChanged(float value)
    {
        _shadowIntensity = value;
        _Light.shadowStrength = _shadowIntensity;
    }

    private void onColorChanged(Color c)
    {
        _color = c;
        _Light.color = _color;
        //_Gizmo.material.color = c;
        //_Gizmo.material.SetColor("_EmissionColor", c);
    }

    private void onAimSunClick() {
        GameGlobals.IsAimingSun = !GameGlobals.IsAimingSun;
        _ui.ToggleAimSunButtonText();

        if (GameGlobals.IsAimingSun)
        {
            StartCoroutine(AimSun());
        }

    }

    private IEnumerator AimSun() {
        while (GameGlobals.IsAimingSun)
        {
            _Light.transform.localRotation = Camera.main.transform.localRotation;



            yield return null;
        }
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.DIRECTIONAL_LIGHT);

        _ui.Reset();

        _ui._IntensitySlider.onValueChanged.AddListener(onLightIntensityChanged);
        _ui._ShadowIntensitySlider.onValueChanged.AddListener(onShadowIntensityChanged);
        _ui._ColorPicker.onValueChanged.AddListener(onColorChanged);
        _ui._AimSunButton.onClick.AddListener(onAimSunClick);
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });

        _ui._IntensitySlider.value = _lightIntensity;
        _ui._ShadowIntensitySlider.value = _shadowIntensity;
        _ui._ColorPicker.CurrentColor = _color;
    }

}
