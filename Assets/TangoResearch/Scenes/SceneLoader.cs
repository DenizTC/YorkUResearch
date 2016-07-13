using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    public Button _LoadDepthMap;
    public Button _LoadOcclusionAndLighting;

    public void Start() {

        _LoadDepthMap.onClick.AddListener(onLoadDepthMapClick);
        _LoadOcclusionAndLighting.onClick.AddListener(onOcclusionAndLightingClick);

    }

    private void onLoadDepthMapClick() {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    private void onOcclusionAndLightingClick()
    {
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

}
