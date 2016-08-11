using UnityEngine;
using System.Collections;

public class SpinY : MonoBehaviour {

    public float speed = 45;

    private void Update()
    {

        transform.Rotate(new Vector3(0, speed * Time.deltaTime, 0), Space.Self);

    }
}