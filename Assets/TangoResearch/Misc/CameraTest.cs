using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class CameraTest : MonoBehaviour {

    public RenderTexture _RT;

	// Use this for initialization
	void Start () {
        Camera.main.depthTextureMode = DepthTextureMode.Depth;

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnPostRender() {
        
        //_RT = RenderTexture.GetTemporary(160, 90, 24);
        _RT = RenderTexture.GetTemporary(Screen.width, Screen.height, 24, RenderTextureFormat.R8);
    }

}
