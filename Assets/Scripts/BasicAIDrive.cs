using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicAIDrive : MonoBehaviour
{
    public WheelCollider frontLeft;
    public WheelCollider frontRight;
    public WheelCollider rearLeft;
    public WheelCollider rearRight;

    public float motorForce = 1500f;
    public float maxSteerAngle = 30f;

    public wayPoints waypointContainer;
    private List<Transform> waypoints;
    private int currentWaypointIndex = 0;

    public float waypointRange = 5f;

    private float startDelay = 4f; // Delay time
    private float timer = 0f;
    private bool canDrive = false;

    void Start()
    {
        waypoints = waypointContainer.waypoints;
    }

    void FixedUpdate()
    {
        if (!canDrive)
        {
            timer += Time.fixedDeltaTime;
            if (timer >= startDelay)
                canDrive = true;
            else
                return;

        }

            if (waypoints.Count == 0) return;

        Transform targetWaypoint = waypoints[currentWaypointIndex];
        Vector3 relativeVector = transform.InverseTransformPoint(targetWaypoint.position);
        float steer = relativeVector.x / relativeVector.magnitude;
        float steerAngle = Mathf.Clamp(steer * maxSteerAngle, -maxSteerAngle, maxSteerAngle);

        frontLeft.steerAngle = steerAngle;
        frontRight.steerAngle = steerAngle;

        rearLeft.motorTorque = motorForce;
        rearRight.motorTorque = motorForce;

        // Move to next waypoint if close enough
        float distance = Vector3.Distance(transform.position, targetWaypoint.position);
        if (distance < waypointRange)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Count)
                currentWaypointIndex = 0; 
        }

        Debug.DrawLine(transform.position, targetWaypoint.position, Color.yellow);
    }
}
