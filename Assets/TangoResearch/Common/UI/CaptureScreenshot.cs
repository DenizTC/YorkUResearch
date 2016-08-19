using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CaptureScreenshot : MonoBehaviour {

    public Button _ButtonScreenshot;

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
        // Wait for screen rendering to complete
        yield return new WaitForEndOfFrame();
        string fileName = SceneManager.GetActiveScene().name + "-" + System.DateTime.Now.ToString("MM-dd-yy_hh-mm-ss") + ".png";

#if UNITY_ANDROID && !UNITY_EDITOR
            Application.CaptureScreenshot("../../../../DCIM/" + fileName, 1);
#else
            Application.CaptureScreenshot(fileName, 1);
#endif

        MessageManager._MessageManager.PushMessage("Saved screenshot " + fileName);
    }




}
