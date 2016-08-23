using UnityEngine;
using System.Collections;


public class PositionTracker : MonoBehaviour {

    public Waypoint _CurrentWaypoint;

    void Start () {
        if (transform.tag != "AIPositionTracker")
            Debug.LogWarning("Tag of " + transform.name + " must be set to AIPositionTracker!");

	}
	
}
