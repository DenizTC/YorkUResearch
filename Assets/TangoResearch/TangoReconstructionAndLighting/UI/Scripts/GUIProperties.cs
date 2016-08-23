using UnityEngine;
using System.Collections;

public class GUIProperties : MonoBehaviour {

    public GUIARAmbientLight _PanelARAmbientLight;
    public GUIARDirectionalLight _PanelARDirectionalLight;
    public GUIARPointLight _PanelARPointLight;
    public GUIARProp _PanelProp;
    public GUIAREnemy _PanelEnemy;

    public static GUIProperties _Properties;

	void Awake () {

        if (!_Properties)
            _Properties = this; 

        _Properties.gameObject.SetActive(false);

        
	}

}
