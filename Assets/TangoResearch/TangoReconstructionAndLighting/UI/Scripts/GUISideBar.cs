using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Tango;

public class GUISideBar : MonoBehaviour {

    public static GUISideBar _GUISideBar;

    public Toggle _SpaceClearToggle;
    public Toggle _WireframeToggle;
    public Toggle _EnvShadowToggle;
    public Button _SunSettingsButton;
    public Button _AmbientSettingsButton;
    public GUIARDirectionalLight _SunController;
    public Renderer _DynamicMesh;
    public Button _SwitchMap;
    public Image _ImageMap;

    private TangoApplication _tangoApplication;

    void Awake () {
        _tangoApplication = GameObject.FindObjectOfType<TangoApplication>();

        if (!_GUISideBar)
            _GUISideBar = this;

        _GUISideBar.gameObject.SetActive(true);
    }

    void Start() {
        _SwitchMap.onClick.AddListener(onMapSwitchClick);
        _SunSettingsButton.onClick.AddListener(onSunClick);
        _AmbientSettingsButton.onClick.AddListener(onAmbientClick);
        _WireframeToggle.onValueChanged.AddListener(onWireframeToggled);
        _EnvShadowToggle.onValueChanged.AddListener(onEnvShadowToggled);
        _SpaceClearToggle.onValueChanged.AddListener(onSpaceClearToggled);

        _WireframeToggle.isOn = GameGlobals.DrawWireframe;
        _EnvShadowToggle.isOn = GameGlobals.EnvironmentShadows;
    }


    private void onSunClick() {
        GameGlobals.ChangeSelected(Enums.SelectionType.DIRECTIONAL_LIGHT);
        GameGlobals.SetPropertiesOpen(true);
        ARDirectionalLight._Sun.MakeSelected();
    }

    private void onAmbientClick() {
        GameGlobals.ChangeSelected(Enums.SelectionType.AMBIENT_LIGHT);
        GameGlobals.SetPropertiesOpen(true);
        ARAmbientLight._SceneAmbientLight.MakeSelected();
    }

    private void onMapSwitchClick() {
        float v = (_ImageMap.material.GetFloat("_UseMap1") == 1) ? 0 : 1;
        Debug.Log(v);
        _ImageMap.material.SetFloat("_UseMap1", v);
        _ImageMap.enabled = false;
        _ImageMap.enabled = true;
    }

    private void onSpaceClearToggled(bool value) {
        _tangoApplication.m_3drSpaceClearing = value;

        //Tango3DReconstruction.SetSpaceClearing(value);
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
