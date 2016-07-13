using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;

public class GUISideBar : MonoBehaviour {

    public static GUISideBar _GUISideBar;

    public Toggle _WireframeToggle;
    public Toggle _EnvShadowToggle;
    public Button _SunSettingsButton;
    public GUIARDirectionalLight _SunController;
    public Renderer _DynamicMesh;

    void Awake () {
        if (!_GUISideBar)
            _GUISideBar = this;

        _GUISideBar.gameObject.SetActive(true);
    }

    void Start() {
        _SunSettingsButton.onClick.AddListener(onSunClick);
        _WireframeToggle.onValueChanged.AddListener(onWireframeToggled);
        _EnvShadowToggle.onValueChanged.AddListener(onEnvShadowToggled);

        _WireframeToggle.isOn = GameGlobals.DrawWireframe;
        _EnvShadowToggle.isOn = GameGlobals.EnvironmentShadows;
    }

    private void onSunClick() {
        GameGlobals.ChangeSelected(Enums.SelectionType.DIRECTIONAL_LIGHT);
        GameGlobals.SetPropertiesOpen(true);
        ARDirectionalLight._Sun.MakeSelected();
    }

    private void onWireframeToggled(bool value) {
        GameGlobals.DrawWireframe = value;
        float p = (GameGlobals.DrawWireframe) ? 1 : 0;
        _DynamicMesh.sharedMaterial.SetFloat("_DrawWireframe", p);
    }

    private void onEnvShadowToggled(bool value) {
        GameGlobals.EnvironmentShadows = value;
        UnityEngine.Rendering.ShadowCastingMode sMode = (GameGlobals.EnvironmentShadows) ? 
            UnityEngine.Rendering.ShadowCastingMode.On : 
            UnityEngine.Rendering.ShadowCastingMode.Off;
        _DynamicMesh.GetComponent<TangoDynamicMesh>().UpdateShadowCastingMode(sMode);
    }

}
