using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class vehicleList : MonoBehaviour
{
    public List<GameObject> vehicles;
    public Transform vehicleSpawnPoint;
    private int currentVehicleIndex = 0;
    private GameObject currentVehicle;

    public static int selectedCarIndex { get; set; }

    private void Start()
    {
        if (vehicles.Count > 0)
        {
            SpawnVehicle(currentVehicleIndex);
        }
       
    }
  
    private void SpawnVehicle(int index)
    {
        // Spawn the vehicle
        currentVehicle = Instantiate(vehicles[index], vehicleSpawnPoint.position, vehicleSpawnPoint.rotation);

        // Disable physics components
        DisablePhysics(currentVehicle);

        // Set parent and reset local position
        currentVehicle.transform.SetParent(vehicleSpawnPoint);
        currentVehicle.transform.localPosition = Vector3.zero; // Reset local position
        currentVehicle.transform.localRotation = Quaternion.identity; // Reset local rotation
    }

    private void DisablePhysics(GameObject vehicle)
    {
        // Disable Rigidbody
        Rigidbody rb = vehicle.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
        }

        // Disable CarController script
        CarController carController = vehicle.GetComponent<CarController>();
        if (carController != null)
        {
            carController.enabled = false;
        }

        // Disable wheel colliders
        WheelCollider[] wheelColliders = vehicle.GetComponentsInChildren<WheelCollider>();
        foreach (WheelCollider collider in wheelColliders)
        {
            collider.enabled = false;
        }
    }

    public void ShowNextVehicle()
    {
        if (vehicles.Count == 0) return;
        if (currentVehicle != null)
        {
            Destroy(currentVehicle);
        }
        currentVehicleIndex = (currentVehicleIndex + 1) % vehicles.Count;
        SpawnVehicle(currentVehicleIndex);
    }

    public void ShowPreviousVehicle()
    {
        if (vehicles.Count == 0) return;
        if (currentVehicle != null)
        {
            Destroy(currentVehicle);
        }
        currentVehicleIndex = (currentVehicleIndex - 1 + vehicles.Count) % vehicles.Count;
        SpawnVehicle(currentVehicleIndex);
    }
    public void SelectCarAndStartGame()
    {
        // Save selected car index using PlayerPrefs
        PlayerPrefs.SetInt("SelectedCarIndex", currentVehicleIndex);
        PlayerPrefs.Save();
        Debug.Log("Saved car index: " + currentVehicleIndex);

        // Load game scene
        SceneManager.LoadScene(1); // Make sure this is your game scene index
    }
    public void QuitGame()
    {
        Time.timeScale = 1f; // Ensure time is normal
        Application.Quit(); // Will not quit in Editor
    }

}
