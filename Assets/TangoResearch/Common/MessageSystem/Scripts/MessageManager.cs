using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {

    public enum Response { OK, CANCEL, YES, NO, NULL };

    public Transform _MessageStackPanel;
    public Transform _MessageBoxPanel;
    public Text _TextMessage;
    public MessageOkCancel _PrefabMessageOkCancel;
    public MessageOk _PrefabMessageOk;

    public static MessageManager _MessageManager;

	void Start () {
        if (!_MessageManager)
            _MessageManager = this;
	}

    /// <summary>
    /// Pushes a text message to the screen, and pops it after the specified time.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="lifeTime">The life time in seconds, of the message. 
    /// Life time equals infinity if non positive.</param>
    public void PushMessage(string message, float lifeTime = 1f) {
        Text t = Instantiate(_TextMessage) as Text;
        t.text = message;

        t.transform.SetParent(_MessageStackPanel);
        t.GetComponent<RectTransform>().localScale = Vector3.one;
        if(lifeTime > 0)
            Destroy(t.gameObject, lifeTime);
    }

    public void PushMessageOKCancel(string message, out MessageOkCancel oc) {
        oc = Instantiate(_PrefabMessageOkCancel) as MessageOkCancel;
        _MessageBoxPanel.gameObject.SetActive(true);
        oc.transform.SetParent(_MessageBoxPanel);
        oc.GetComponent<RectTransform>().localPosition = Vector3.zero;
        oc.GetComponent<RectTransform>().localScale = Vector3.one;
        oc.Init(message);
    }

    public void PushMessageOK(string message)
    {
        MessageOk o = Instantiate(_PrefabMessageOk) as MessageOk;
        _MessageBoxPanel.gameObject.SetActive(true);
        o.transform.SetParent(_MessageBoxPanel);
        o.GetComponent<RectTransform>().localPosition = Vector3.zero;
        o.GetComponent<RectTransform>().localScale = Vector3.one;
        o.Init(message);
    }

}
