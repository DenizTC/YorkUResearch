using UnityEngine;
using System.Collections;

public class AIAgent : MonoBehaviour {

    public Waypoint _CurrentWaypoint;

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


        while (i < 1.0f || Vector3.Distance(transform.position, next.transform.position) > 0.2f)
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

}
