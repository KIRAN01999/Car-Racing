using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(CarController))]

public class AIcarcontroller : MonoBehaviour
{
    public wayPoints waypointcontainer;
    public List<Transform> waypoints;
    public int currentwaypoint;
    private CarController carController;
    public float wayPointRange;
    private float currentAngle;

   
    void Start()
    {
        carController = GetComponent<CarController>();
        waypoints = waypointcontainer.waypoints;
        currentwaypoint = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (Vector3.Distance(waypoints[currentwaypoint].position,transform.position) < wayPointRange)
        {
            currentwaypoint++;
            if(currentwaypoint == waypoints.Count)currentwaypoint= 0;
           
        }
        Vector3 fwd = transform.TransformDirection(Vector3.forward);
        currentAngle = Vector3.SignedAngle(fwd, waypoints[currentwaypoint].position-transform.position,Vector3.up);
        carController.SetInput(1, currentAngle, 0, 0);

        Debug.DrawRay(transform.position, waypoints[currentwaypoint].position-transform.position,Color.yellow);
    }
}
