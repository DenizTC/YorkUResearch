using UnityEngine;
using System.Collections;

/// <summary>
/// Sends the target camera's MVP matrix to the shader property "_MVPTargetCam" of the target renderer.
/// </summary>
public class SendMVP : MonoBehaviour {

    public Camera _TargetCam;

    public Renderer _TargetRenderer;
	
	void LateUpdate () {
        _TargetRenderer.sharedMaterial.SetMatrix("_MVPTargetCam",
            _TargetCam.projectionMatrix *_TargetCam.worldToCameraMatrix * _TargetRenderer.localToWorldMatrix);
	}
}
