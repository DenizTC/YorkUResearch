using UnityEngine;
using System.Collections;

public class ARProp : ARSelectable {

    private GUIARProp _ui;

    void Start()
    {
        _ui = GUIProperties._Properties._PanelProp;
    }

    public override void MakeSelected()
    {
        base.MakeSelected();
        GameGlobals.ChangeSelected(Enums.SelectionType.PROP);

        _ui.Reset();
        _ui._DestroyButton.onClick.AddListener(delegate { this.OnDestoyClick(); });
    }
}
