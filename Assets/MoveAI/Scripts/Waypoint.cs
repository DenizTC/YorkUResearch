using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour {

    public float _Radius = 1;

    private SphereCollider _collider;

    public List<Waypoint> _Neighbors;

	private void Awake () {
        _collider = transform.GetComponent<SphereCollider>();
        _collider.radius = _Radius;

        //UpdateNeighbors();
	}

    private void Start() {
        
    }

    public void UpdateRadius(float radius) {
        _Radius = radius;
        if (!_collider)
            _collider = transform.GetComponent<SphereCollider>();
        _collider.radius = _Radius;
    }

    public void UpdateNeighbors() {
        Collider[] waypoints = Physics.OverlapSphere(transform.position, _Radius, 1 << transform.gameObject.layer);
        UpdateNeighbors(waypoints);
    }

    public void UpdateNeighbors(Collider[] waypoints)
    {
        if (waypoints.Length == 0)
            return;
        _Neighbors = new List<Waypoint>();
        for (int i = 0; i < waypoints.Length; i++)
        {
            Waypoint wp = waypoints[i].transform.GetComponent<Waypoint>();
            if (wp != this)
                _Neighbors.Add(wp);
        }
    }

    public Waypoint RandomNeighbor()
    {
        int index = GameGlobals.Rand.Next(0, _Neighbors.Count);
        return _Neighbors[index];
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.tag == "AIAgent")
        {
            other.transform.parent.GetComponent<AIAgent>()._CurrentWaypoint = this;
        }
    }


}
