using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

public class GUILightDetector : MonoBehaviour {

    public static GUILightDetector _GUILD;

    public Canvas _MainUI;
    public Button _ButtonStartFinishLightDet;
    public Transform _PanelLightDetector;
    public LightDetectorImageEffect _LDImageEffect;

    private Text _startFinishText;
    private int _originalCullMask;

	void Start () {
        if (!_GUILD)
            _GUILD = this;
        _ButtonStartFinishLightDet.onClick.AddListener(onClickToggleLD);
        _startFinishText = _ButtonStartFinishLightDet.GetComponentInChildren<Text>();
	}

    private void onClickToggleLD()
    {
        if (!FindLight._LightDetector.IsRunning())
        {
            FindLight._LightDetector.TurnOn();
            _MainUI.gameObject.SetActive(false);
            _PanelLightDetector.gameObject.SetActive(true);
            _LDImageEffect.enabled = true;
            _originalCullMask = _LDImageEffect.GetComponent<Camera>().cullingMask;
            _LDImageEffect.GetComponent<Camera>().cullingMask = 1 + (1 << GameGlobals.ARObjectLayer);
            _startFinishText.text = "Finish";
        }
        else
        {
            createLights(FindLight._LightDetector.TurnOff());
            _MainUI.gameObject.SetActive(true);
            _LDImageEffect.enabled = false;
            _LDImageEffect.GetComponent<Camera>().cullingMask = _originalCullMask;
            _PanelLightDetector.gameObject.SetActive(false);
            _startFinishText.text = "Light Detector";

        }
    }

    private void createLights(List<ColorPoint> points) {
        foreach (ColorPoint p in points)
        {
            ARPointLight arp = (ARPointLight)ARObjectManager._AROBJManager.InstantiateARObject(p.XYZ, Quaternion.identity);
            arp._color = new Color(p.RGB.x, p.RGB.y, p.RGB.z);
            arp._range = 5;
            arp._lightIntensity = p.Luma;
        }
        GUISelectables._GUISelectables.DeselectAll();
        _ButtonStartFinishLightDet.gameObject.SetActive(false);
    }

}
