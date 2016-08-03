using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class WaypointManager : MonoBehaviour {

    
    public Button _ButtonAIAgent;
    public int _WalkableLayer;
    public List<Waypoint> _Waypoints = new List<Waypoint>();
    public Waypoint _WaypointPrefab;
    public float _WaypointRadius = 1;
    public List<AIAgent> _AIAgents = new List<AIAgent>();
    public AIAgent _AIAgentPrefab;
    public Transform _WaypointSpawner;
    public bool _AutoSpawnWaypoints = false;

    private Vector3 _lastSpawnedPos;

    void Start () {
        foreach (var waypoint in GetComponentsInChildren<Waypoint>())
        {
            AddWaypoint(waypoint);
        }

        _lastSpawnedPos = _WaypointSpawner.position;
        _ButtonAIAgent.onClick.AddListener(onAIAgentClick);
    }
	
	void LateUpdate () {
        //if (Input.GetMouseButtonDown(0)) {
        //    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        //    RaycastHit hit;
        //    if (Physics.Raycast(ray, out hit, 10, 1 << _WalkableLayer)) {
        //        AddWaypoint(hit.point);
        //    }
            
        //}
        //if (Input.GetMouseButtonDown(1)) {
        //    AddAIAgent(true);
        //}
        if (_AutoSpawnWaypoints)
        {
            AutoSpawnWaypoint();
        }
        
    }

    public void AddWaypoint(Waypoint wp) {
        if(wp.transform.parent != transform)
            wp.transform.parent = transform;
        _Waypoints.Add(wp);
    }

    public void AddWaypoint(Vector3 pos) {
        Waypoint wp = Instantiate(_WaypointPrefab, pos, Quaternion.identity) as Waypoint;
        wp._Radius = _WaypointRadius;
        wp.UpdateNeighbors();
        foreach (var item in wp._Neighbors)
        {
            item._Neighbors.Add(wp);
        }
        AddWaypoint(wp);
    }

    public void AddWaypoint(Vector3 pos, Collider[] waypointNeighbors)
    {
        Waypoint wp = Instantiate(_WaypointPrefab, pos, Quaternion.identity) as Waypoint;
        //wp._Radius = _WaypointRadius;
        wp.UpdateRadius(_WaypointRadius);
        wp.UpdateNeighbors(waypointNeighbors);
        Debug.Log("AddWaypoint::Neighbors: " + wp._Neighbors.Count);
        foreach (var item in wp._Neighbors)
        {
            item._Neighbors.Add(wp);
        }
        AddWaypoint(wp);
    }

    public void AddAIAgent(bool startWandering = false) {
        Waypoint initWP = RandomWaypoint();
        AIAgent ai = Instantiate(_AIAgentPrefab, initWP.transform.position + new Vector3(0,1,0), Quaternion.identity) as AIAgent;
        ai._CurrentWaypoint = initWP;
        _AIAgents.Add(ai);

        if (startWandering) {
            ai.StartCoroutine(ai.StartWandering());
        }
    }

    public Waypoint RandomWaypoint()
    {
        int index = GameGlobals.Rand.Next(0, _Waypoints.Count - 1);
        return _Waypoints[index];
    }

    private void AutoSpawnWaypoint() {
        Vector3 currentPos = new Vector3(_WaypointSpawner.position.x, 0, _WaypointSpawner.position.z);
        if (Vector3.Distance(currentPos, _lastSpawnedPos) > 0.7f*2*_WaypointRadius)
        {
            Vector3 newWP;
            if (!IntersectionWithGround(_WaypointSpawner.position, out newWP))
                return;

            Collider[] waypoints =
                Physics.OverlapSphere(newWP, 0.7f*2*_WaypointRadius, 1 << GameGlobals.WaypointLayer);
            Debug.Log(waypoints.Length);
            if (waypoints.Length > 2)
                return;

            for (int i = 0; i < waypoints.Length; i++)
            {
                if (Vector3.Distance(waypoints[i].transform.position, newWP) < 0.7f*2*_WaypointRadius)
                    return;
            }
            AddWaypoint(newWP, waypoints);
            _lastSpawnedPos = currentPos;
        }
    }

    private bool IntersectionWithGround(Vector3 pos, out Vector3 intersection) {
        intersection = pos;
        Ray ray = new Ray(pos, -Vector3.up);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, 3, 1 << GameGlobals.WalkableLayer))
        {
            intersection = hit.point;
            return true;
        }
        else
        {
            return false;
        }

    }

    private void onAIAgentClick() {

        if(_Waypoints.Count > 0)
            AddAIAgent(true);

    }

}
