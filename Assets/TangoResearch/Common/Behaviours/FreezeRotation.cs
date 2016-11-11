using UnityEngine;
using System.Collections;

public class FreezeRotation : MonoBehaviour {

    private Quaternion _rotation;

    void Start()
    {
        _rotation = transform.rotation;
    }

    void Update()
    {
        transform.rotation = _rotation;
    }
}
