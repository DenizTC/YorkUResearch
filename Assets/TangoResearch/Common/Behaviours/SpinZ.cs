using UnityEngine;
using System.Collections;

public class SpinZ : MonoBehaviour {

    public float speed = 45;

    private void Update()
    {

        transform.Rotate(new Vector3(0, 0, speed * Time.deltaTime), Space.Self);

    }
}
