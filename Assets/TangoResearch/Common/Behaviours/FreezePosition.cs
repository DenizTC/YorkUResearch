using UnityEngine;
using System.Collections;

public class FreezePosition : MonoBehaviour {


    private Vector3 _position;

	void Start () {
        _position = transform.position;
	}
	
	void Update () {
        transform.position = _position;
	}
}
