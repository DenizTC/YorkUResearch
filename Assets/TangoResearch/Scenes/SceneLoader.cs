using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour {

    public Button _LoadDepthMap;
    public Button _LoadOcclusionAndLighting;
    public Button _LoadOcclusionAndLightingSC;

    public static bool SpaceClearing3DR = false;

    public void Start() {

        _LoadDepthMap.onClick.AddListener(onLoadDepthMapClick);
        _LoadOcclusionAndLighting.onClick.AddListener(onOcclusionAndLightingClick);
        _LoadOcclusionAndLightingSC.onClick.AddListener(onOcclusionAndLightingSCClick);
    }

    private void onLoadDepthMapClick() {
        SceneManager.LoadScene(1, LoadSceneMode.Single);
    }

    private void onOcclusionAndLightingClick()
    {
       SpaceClearing3DR = false;
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

    private void onOcclusionAndLightingSCClick()
    {
        SpaceClearing3DR = true;
        SceneManager.LoadScene(2, LoadSceneMode.Single);
    }

}
