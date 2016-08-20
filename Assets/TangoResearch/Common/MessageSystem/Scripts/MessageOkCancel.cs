using UnityEngine;
using UnityEngine.UI;

public class MessageOkCancel : MonoBehaviour {

    public Button _ButtonOK;
    public Button _ButtonCancel;
    public Text _TextMessage;

    [HideInInspector]
    public bool _WaitingForResponse = true;
    private MessageManager.Response _response = MessageManager.Response.NULL;

	void Start () {
        _ButtonOK.onClick.AddListener(onClickOK);
        _ButtonCancel.onClick.AddListener(onClickCancel);
	}

    private void onClickCancel()
    {
        _response = MessageManager.Response.CANCEL;
        _WaitingForResponse = false;
    }

    private void onClickOK()
    {
        _response = MessageManager.Response.OK;
        _WaitingForResponse = false;
    }

    public void Init(string message) {
        _TextMessage.text = message;
    }

    public void GetResponse(out MessageManager.Response response) {
        response = _response;
        transform.parent.gameObject.SetActive(false);
        Destroy(transform.gameObject);
    }

}
