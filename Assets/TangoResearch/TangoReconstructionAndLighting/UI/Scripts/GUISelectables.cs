using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(ToggleGroup))]
public class GUISelectables : MonoBehaviour {

    public static GUISelectables _GUISelectables;
    public Transform _ARObjectContainer;
    public ARToggle _PrefabARToggle;

    private ToggleGroup _toggleGroup;
    private List<Toggle> _toggles;

    void Awake() {
        if (!_GUISelectables)
            _GUISelectables = this;

        _GUISelectables.gameObject.SetActive(true);
    }

    void Start()
    {
        _toggleGroup = GetComponent<ToggleGroup>();
        _toggles = new List<Toggle>();
    }

    public void AddSelectable(Sprite spriteIcon, int objManagerIndex) {
        ARToggle art = Instantiate(_PrefabARToggle) as ARToggle;
        art.Init(spriteIcon);

        _toggles.Add(art.GetComponent<Toggle>());

        art.GetComponent<Toggle>().onValueChanged.AddListener((value) => onToggleChanged(objManagerIndex, value));
    }

    /// <summary>
    /// Callback for the toggle.
    /// </summary>
    /// <param name="objManagerIndex">Index of the item in object manager.</param>
    /// <param name="value">The toggled state of this toggle button.</param>
    private void onToggleChanged(int objManagerIndex, bool value) {
        if (value) {
            GameGlobals.CurrentARSelectableIndex = objManagerIndex;
        }
    }

    public bool IsAnyToggled() {
        return _toggleGroup.AnyTogglesOn();
    }

    public void DeselectAll() {
        foreach (var t in _toggles)
        {
            if (t.isOn)
                t.isOn = false;
        }
    }

}
