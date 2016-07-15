using UnityEngine;
using System.Collections;

public class GUIProperties : MonoBehaviour {

    //private Enums.SelectionType _curSelectionType = Enums.SelectionType.NONE;

    public GUIARAmbientLight _PanelARAmbientLight;
    public GUIARDirectionalLight _PanelARDirectionalLight;
    public GUIARPointLight _PanelARPointLight;
    public GUIARProp _PanelProp;

    public static GUIProperties _Properties;

	void Awake () {
        if (!_Properties)
            _Properties = this;

        _Properties.gameObject.SetActive(false);
	}

}
