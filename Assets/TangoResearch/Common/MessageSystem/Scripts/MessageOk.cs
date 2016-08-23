using UnityEngine;
using UnityEngine.UI;

public class MessageOk : MonoBehaviour
{

    public Button _ButtonOK;
    public Text _TextMessage;

    [HideInInspector]
    public bool _WaitingForResponse = true;

    void Start()
    {
        _ButtonOK.onClick.AddListener(onClickOK);
    }

    private void onClickOK()
    {
        _WaitingForResponse = false;
        transform.parent.gameObject.SetActive(false);
        Destroy(transform.gameObject);
    }

    public void Init(string message)
    {
        _TextMessage.text = message;
    }

}
