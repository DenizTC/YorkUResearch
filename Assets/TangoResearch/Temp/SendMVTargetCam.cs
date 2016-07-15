using UnityEngine;
using System.Collections;

public class SendMVTargetCam : MonoBehaviour {

    public Camera _TargetCam;

    public Renderer _TargetRenderer;

	// Use this for initialization
	void Start () {
	
	}
	
	void LateUpdate () {
        _TargetRenderer.sharedMaterial.SetMatrix("_MVPTargetCam",
            _TargetCam.projectionMatrix *_TargetCam.worldToCameraMatrix * _TargetRenderer.localToWorldMatrix);
	}
}
