using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wayPoints : MonoBehaviour
{
    public List<Transform> waypoints;
     void Awake()
    {
        foreach(Transform tr in gameObject.GetComponentsInChildren<Transform>())
        {
            waypoints.Add(tr);
        }
        waypoints.Remove(waypoints[0]);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
