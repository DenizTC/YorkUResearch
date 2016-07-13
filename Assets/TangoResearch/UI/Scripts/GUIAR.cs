using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class GUIAR : MonoBehaviour
{

    public Button _DestroyButton;

    public virtual void Reset() {
        _DestroyButton.onClick.RemoveAllListeners();
        _DestroyButton.interactable = true;
    }

}
