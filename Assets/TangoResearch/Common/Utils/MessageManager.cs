using UnityEngine;
using UnityEngine.UI;

public class MessageManager : MonoBehaviour {

    public Text _TextMessage;

    public static MessageManager _MessageManager;
    
	void Start () {
        if (!_MessageManager)
            _MessageManager = this;
	}

    /// <summary>
    /// Pushes the message to the screen, and pops it after the specified time.
    /// </summary>
    /// <param name="message">The message.</param>
    /// <param name="lifeTime">The life time in seconds, of the message. 
    /// Life time equals infinity if non positive.</param>
    public void PushMessage(string message, float lifeTime = 1f) {
        Text t = Instantiate(_TextMessage) as Text;
        t.text = message;

        t.transform.SetParent(this.gameObject.transform);
        t.GetComponent<RectTransform>().localScale = Vector3.one;
        if(lifeTime > 0)
            Destroy(t.gameObject, lifeTime);
    }

}
