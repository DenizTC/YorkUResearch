using UnityEngine;
using System.Collections;

public class LookAt : MonoBehaviour {

    public Transform _Target;
    
	void Update () {
        transform.LookAt(_Target);
	}
}
