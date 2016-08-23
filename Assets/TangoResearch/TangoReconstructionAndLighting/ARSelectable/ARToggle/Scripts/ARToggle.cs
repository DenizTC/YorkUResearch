using UnityEngine;
using System.Collections;
using UnityEngine.UI;

[RequireComponent(typeof(Toggle))]
public class ARToggle : MonoBehaviour {

    public Image _ImageIcon;

    private Toggle _toggle;
    
    public void Start() {
        _toggle = GetComponent<Toggle>();
        transform.SetParent(GUISelectables._GUISelectables._ARObjectContainer);
        transform.localScale = Vector3.one;
        _toggle.group = GUISelectables._GUISelectables.transform.GetComponent<ToggleGroup>();
    }

    public void Init(Sprite spriteIcon) {
        if(spriteIcon != null)
            _ImageIcon.sprite = spriteIcon;
    }

}
