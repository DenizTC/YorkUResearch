using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Linq;

public class GUISelectables : MonoBehaviour {

    public static GUISelectables _GUISelectables;

    public Toggle[] _Toggles;
    public ToggleGroup _ToggleGroup;

    void Awake() {
        if (!_GUISelectables)
            _GUISelectables = this;

        _GUISelectables.gameObject.SetActive(true);
    }

    void Start()
    {
        
        for (int i = 0; i < _Toggles.Length; i++)
        {
            _Toggles[i].onValueChanged.AddListener(delegate { OnSelectableChanged(); });
        }
    }

    // Fix this!
    // Gets called for each toggle in the group.
    // Instead, it should just run once for the active toggle.
    private void OnSelectableChanged() {
        string name = _ToggleGroup.ActiveToggles().FirstOrDefault().name;
        Debug.Log(name);
        switch (name)
        {
            case "ToggleSelectablePointLight" :
                GameGlobals.ChangeDrawingSelection(Enums.SelectionType.POINT_LIGHT);
                break;
            default:
                GameGlobals.ChangeDrawingSelection(Enums.SelectionType.PROP);
                break;
        }

    }
    
    

}
