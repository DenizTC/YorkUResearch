using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Waypoint : MonoBehaviour {



    public float _Radius = 1;

    private CapsuleCollider _collider;

    public List<Waypoint> _Neighbors;
    public List<Waypoint> _Paths = new List<Waypoint>();
    public Dictionary<Vector3, Waypoint> posInd = new Dictionary<Vector3, Waypoint>(); // key a came from value b

    public int ID = 0;

    private void Awake () {
        _collider = transform.GetComponent<CapsuleCollider>();
        _collider.radius = _Radius;

        //UpdateNeighbors();
	}

    private void Start() {
        
    }

    public void UpdateRadius(float radius) {
        _Radius = radius;
        if (!_collider)
            _collider = transform.GetComponent<CapsuleCollider>();
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
        if (other.transform.GetComponent<PositionTracker>() != null)
            other.transform.GetComponent<PositionTracker>()._CurrentWaypoint = this;

        //if (other.transform.tag == "AIPositionTracker")
        //{
        //    if (other.transform.GetComponent<PositionTracker>() != null)
        //        other.transform.GetComponent<PositionTracker>()._CurrentWaypoint = this;
        //    else
        //        other.transform.parent.GetComponent<PositionTracker>()._CurrentWaypoint = this;
        //}
    }


    public void CalculatePaths()
    {

        // Breadth first search

        

        _Paths.Clear();
        posInd.Clear();

        Queue<Waypoint> q = new Queue<Waypoint>();
        q.Enqueue(this);

        Waypoint cur;
        while (q.Count > 0)
        {
            cur = q.Dequeue();

            //if (cur.transform.position == end.transform.position)
            //    break;
            foreach (var next in cur._Neighbors)
            {
                if (!posInd.ContainsKey(next.transform.position))
                {
                    q.Enqueue(next);
                    posInd.Add(next.transform.position, cur);
                    //_Paths.Add(cur);
                }
            }
        }

        
    }

    public List<Waypoint> FindPath(Waypoint end) {
        Waypoint cur = end;
        List<Waypoint> path = new List<Waypoint>();
        while (cur.transform.position != this.transform.position)
        {
            cur = posInd[cur.transform.position];
            path.Add(cur);
        }
        path.Reverse();
        return path;
    }

}
