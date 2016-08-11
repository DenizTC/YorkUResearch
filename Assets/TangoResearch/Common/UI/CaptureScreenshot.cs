using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaptureScreenshot : MonoBehaviour {

    public Button _ButtonScreenshot;

    public Text _TextMessage;

    public static bool _TextMessageOpen = false;

	void Start () {
        if(_ButtonScreenshot)
            _ButtonScreenshot.onClick.AddListener(onClickButtonScreenShot);
	}
	
	void Update () {
        if (Input.touchCount == 2)
        {
            Touch t0 = Input.GetTouch(0);
            Touch t1 = Input.GetTouch(1);
            if (t0.phase == TouchPhase.Began &&
                t1.phase == TouchPhase.Began)
            {
                StartCoroutine(CaptureScreen());
            }
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            StartCoroutine(CaptureScreen());
        }
    }

    private void onClickButtonScreenShot() {
        StartCoroutine(CaptureScreen());
    }

    public IEnumerator CaptureScreen()
    {
        if(_TextMessageOpen)
            yield break;

        _TextMessageOpen = true;
        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();
        string fileName = SceneManager.GetActiveScene().name + "-" + System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss") + ".png";

        #if UNITY_ANDROID && !UNITY_EDITOR
            Application.CaptureScreenshot("../../../../DCIM/" + fileName, 1);
        #else
            Application.CaptureScreenshot(fileName, 1);
        #endif

        _TextMessage.text = "Saved screenshot " + fileName;
        _TextMessage.enabled = true;

        StartCoroutine(HideTextMessage());
    }

    /// <summary>
    /// Hides the text message.
    /// TODO: Create TextMessage script and move this function to it.
    /// </summary>
    public IEnumerator HideTextMessage()
    {

        yield return new WaitForSeconds(1);
        _TextMessage.enabled = false;
        _TextMessageOpen = false;
    }


}
