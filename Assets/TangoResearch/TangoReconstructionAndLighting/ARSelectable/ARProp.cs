using UnityEngine;
using System.Collections.Generic;
using System;

public class ARProp : ARSelectable {

    private GUIARProp _ui;

    public Renderer[] _PRBRenderer;
    /// <summary>
    ///  Materials with the 'Standard' material. Allows controlling their 
    ///  metallic and glossiness values.
    /// </summary>
    private Material[] _PBRMaterials;

    public float _Metallic = 0.5f;
    public float _Smoothness = 0.5f;

    public override void Start()
    {
        base.Start();

        this._SelectableType = Enums.SelectionType.PROP;

        _ui = GUIProperties._Properties._PanelProp;

        List<Material> pbrMat = new List<Material>();
        foreach (var r in _PRBRenderer)
        {
            foreach (var m in r.materials)
            {
                if(m.shader.name == "Standard" && m.GetTexture("_MetallicGlossMap") == null)
                    pbrMat.Add(m);
            }
        }
        _PBRMaterials = pbrMat.ToArray();
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.PROP);

        _ui.Reset();
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });
        _ui._ButtonMove.onClick.AddListener(this.OnClickMove);

        if (_PBRMaterials.Length > 0)
        {
            _ui._SliderMetallic.transform.parent.parent.gameObject.SetActive(true);
            _ui._SliderSmoothness.transform.parent.parent.gameObject.SetActive(true);
            _ui._SliderMetallic.onValueChanged.AddListener(onValueChangedMetallic);
            _ui._SliderSmoothness.onValueChanged.AddListener(onValueChangedSmoothness);

            _ui._SliderMetallic.value = _Metallic;
            _ui._SliderSmoothness.value = _Smoothness;
        }
        else
        {
            _ui._SliderMetallic.transform.parent.parent.gameObject.SetActive(false);
            _ui._SliderSmoothness.transform.parent.parent.gameObject.SetActive(false);
        }

    }

    private void onValueChangedMetallic(float value)
    {
        _Metallic = value;
        foreach (var m in _PBRMaterials)
        {
            m.SetFloat("_Metallic", _Metallic);
        }
    }

    private void onValueChangedSmoothness(float value) {
        _Smoothness = value;
        foreach (var m in _PBRMaterials)
        {
            m.SetFloat("_Glossiness", _Smoothness);
        }
    }

    /// <summary>
    /// TODO: Move this to a utility class!
    /// Gets all the materials of this transform and its children (if any).
    /// </summary>
    public List<Material> GetAllMaterials() {
         List<Material> materials = new List<Material>();

        // Add all the materials (if any) of this transform.
        if (GetComponent<Renderer>())
        {
            foreach (var m in GetComponent<Renderer>().materials)
            {
                materials.Add(m);
            }
        }

        // Add all the materials (if any) of this transform's children.
        if (transform.childCount > 0)
        {
            Renderer[] r = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < r.Length; i++)
            {
                foreach (var m in r[i].materials)
                {
                    materials.Add(m);
                }
            }
        }

        return materials;
    }

}
