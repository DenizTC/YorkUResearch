using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIAgent : PositionTracker {

    public enum Action { NONE, WANDER, FOLLOW };

    /// <summary>
    /// This AIAgent should only traverse the waypoints in _WPM.
    /// </summary>
    public WaypointManager _WPM;
    public PositionTracker _Target;

    private float _travellingTime = 0;
    public float _MinTravellingTime = 3;
    public float _Speed;
    public float _MinWaitTime;
    public float _MaxWaitTime;

    void Start() {
        if (!_WPM)
            Init(WaypointManager._DefaultWM, Action.FOLLOW);
    }

    public void Init(WaypointManager wpm, Action action) {
        _WPM = wpm;

        Waypoint initWP = _WPM.RandomWaypoint();
        transform.position = initWP.transform.position + new Vector3(0, 1, 0);
        _CurrentWaypoint = initWP;
        _Target = _WPM._WaypointSpawner.GetComponent<PositionTracker>();

        if (transform.parent != _WPM.transform)
            transform.parent = _WPM.transform;

        _WPM._AIAgents.Add(this);

        StartAction(action);
    }

    public IEnumerator StartWandering()
    {
        Waypoint next = _CurrentWaypoint.RandomNeighbor();

        float i = 0.0f;
        float rate = 1.0f / _Speed;
        Vector3 currentPos = transform.position;


        while (i < 1.0f && Vector3.Distance(transform.position, next.transform.position) > 0.2f)
        {
            _travellingTime += Time.deltaTime;
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(currentPos, next.transform.position, i);
            yield return null;
        }

        float randomFloat = Random.Range(0.0f, 1.0f); // Create %50 chance to wait
        if (_travellingTime < _MinTravellingTime || randomFloat > 0.5f)
        {
            StartCoroutine(StartWandering());
        }
        else
        {
            _travellingTime = 0;
            yield return new WaitForSeconds(Random.Range(_MinWaitTime, _MaxWaitTime));
            StartCoroutine(StartWandering());
        }

    }

    public IEnumerator StartFollowing() {
        if(!_Target)
            yield break;

        List<Waypoint> path = _CurrentWaypoint.FindPath(_Target._CurrentWaypoint);
        Vector3 targetPos = _Target._CurrentWaypoint.transform.position;

        for (int i = 0; i < path.Count; i++)
        {
            StartCoroutine(GoTo(targetPos, path[i].transform.position));
            while (moving) {
                yield return null;
            }

            if (_Target._CurrentWaypoint.transform.position != targetPos)
            {
                break;
            }
        }

        yield return null;
        StartCoroutine(StartFollowing());
    }

    private bool moving = false;
    public IEnumerator GoTo(Vector3 originalTargetPos, Vector3 dest) {
        moving = true;
        Vector3 curPos = transform.position;

        transform.LookAt(originalTargetPos);


        float i = 0.0f;
        float rate = 1.0f / _Speed;

        while (i < 1.0f && Vector3.Distance(transform.position, dest) > 0.25f) {

            if (_Target._CurrentWaypoint.transform.position != originalTargetPos)
            {
                moving = false;
                yield break;
            }

            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(curPos, dest, Mathf.Min(i, 1f));

            yield return null;
        }
        moving = false;
    }

    public IEnumerator MoveTo(Vector3 target) {
        float i = 0.0f;
        float rate = 1.0f / _Speed;
        Vector3 currentPos = transform.position;

        while (i < 1.0f || Vector3.Distance(transform.position, target) > 0.2f)
        {
            i += Time.deltaTime * rate;
            transform.position = Vector3.Lerp(currentPos, target, i);
            yield return null;
        }

    }

    public void StartAction(Action action) {
        StopAllCoroutines();

        _WPM.CalculatePaths();

        switch (action)
        {
            case Action.NONE:
                break;
            case Action.WANDER:
                StartCoroutine(StartWandering());
                break;
            case Action.FOLLOW:
                StartCoroutine(StartFollowing());
                break;
            default:
                break;
        }

    }

}
