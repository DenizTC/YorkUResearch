using UnityEngine;
using System.Collections;
using System;

public class SimpleCharacterController : MonoBehaviour {

    public float _Speed = 0.1f;

    private Rigidbody _Rigidbody;

	void Start () {
        _Rigidbody = transform.GetComponent<Rigidbody>();
	}
	
	void LateUpdate () {

        if (Input.GetKey(KeyCode.RightArrow))
        {
            movePlayerRigidBody(_Speed);
        }
        else if (Input.GetKey(KeyCode.LeftArrow))
        {
            movePlayerRigidBody(-_Speed);
        }
        else if (!Input.GetKey(KeyCode.RightArrow) && !Input.GetKey(KeyCode.LeftArrow)) {
            movePlayerRigidBody(0);
        }

	}

    private void movePlayer(float speed)
    {
        transform.position = new Vector3(transform.position.x + speed, transform.position.y, transform.position.z);
    }

    private void movePlayerRigidBody(float speed)
    {
        //_Rigidbody.AddForce(_Speed, 0, 0, ForceMode.VelocityChange);
        _Rigidbody.velocity = new Vector3(speed, 0, 0);
    }

}
