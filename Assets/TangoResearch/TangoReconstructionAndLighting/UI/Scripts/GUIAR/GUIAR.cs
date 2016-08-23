using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public abstract class GUIAR : MonoBehaviour
{

    public Button _DestroyButton;
    public Button _ButtonMove;

    public virtual void Reset() {
        _DestroyButton.onClick.RemoveAllListeners();
        _DestroyButton.interactable = true;

        _ButtonMove.onClick.RemoveAllListeners();
        _ButtonMove.interactable = true;
    }

}
