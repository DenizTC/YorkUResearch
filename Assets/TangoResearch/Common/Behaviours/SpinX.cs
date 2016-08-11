using UnityEngine;
using System.Collections;

public class SpinX : MonoBehaviour {

    public float speed = 45; 

	private	void Update () {

        transform.Rotate(new Vector3(speed * Time.deltaTime, 0, 0), Space.Self); 

	}
}
