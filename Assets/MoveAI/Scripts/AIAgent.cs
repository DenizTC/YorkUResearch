using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AIAgent : MonoBehaviour {

    public enum Action { NONE, WANDER, FOLLOW };

    public Waypoint _CurrentWaypoint;
    public AIAgent _Target;

    private float _travellingTime = 0;
    public float _MinTravellingTime = 3;
    public float _Speed;
    public float _MinWaitTime;
    public float _MaxWaitTime;

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


        Debug.Log("Chasing wp" + _Target._CurrentWaypoint.ID);

        Debug.Log("Finding path");
        List<Waypoint> path = _CurrentWaypoint.FindPath(_Target._CurrentWaypoint);
        Debug.Log("Path found");
        Vector3 targetPos = _Target._CurrentWaypoint.transform.position;
        foreach (var cur in path)
        {
            float i = 0.0f;
            float rate = 1.0f / _Speed;
            
            Vector3 currentPos = transform.position;
            Debug.Log("Entering while");
             //i < 1.0f && Vector3.Distance(transform.position, cur.transform.position) > 0.2f
            while (i < 1.0f)
            {
                Debug.Log("Yield return null");
                yield return null;
                Debug.Log("Return from yield null");

                i += Time.deltaTime * rate;
                transform.position = Vector3.Lerp(currentPos, cur.transform.position, i);
                //Debug.Log(i);

                if (_Target._CurrentWaypoint.transform.position != targetPos)
                {
                    Debug.Log("Break while i:" + i);
                    break;
                }
                
            }
            Debug.Log("Exited while");
            if (_Target._CurrentWaypoint.transform.position != targetPos)
                break;
        }
        Debug.Log("Finished chasing wp");
        //StartCoroutine(StartFollowing());
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

        switch (action)
        {
            case Action.NONE:
                break;
            case Action.WANDER:
                StartCoroutine(StartWandering());
                break;
            case Action.FOLLOW:
                StartCoroutine(StartFollowing());
                //Debug.Log("Finding Path:");
                //List<Waypoint> path = _CurrentWaypoint.FindPath(_Target._CurrentWaypoint);
                

                ////foreach (var wp in path)
                ////{
                ////    Debug.Log(wp.transform.position);
                ////}
                //Debug.Log("Finished");
                break;
            default:
                break;
        }

    }

}
