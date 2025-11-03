using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rotate : MonoBehaviour
{
    public Transform vehicleSpawnPoint; // Reference to the spawn point that rotates
    public float rotateSpeed;

    private void FixedUpdate()
    {
        if (vehicleSpawnPoint != null)
        {
            // Rotate the spawn point around the Y-axis
            vehicleSpawnPoint.Rotate(Vector3.up * rotateSpeed * Time.deltaTime);
        }
    }
}

