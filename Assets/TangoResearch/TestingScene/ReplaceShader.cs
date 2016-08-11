using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]
public class ReplaceShader : MonoBehaviour {

    public Shader _Shader;
    public string _ReplacementTag;

	// Use this for initialization
	void Start () {
        transform.GetComponent<Camera>().SetReplacementShader(_Shader, _ReplacementTag);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
