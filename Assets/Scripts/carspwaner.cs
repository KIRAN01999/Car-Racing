using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carspwaner : MonoBehaviour
{
    public GameObject[] carPrefabs;  // Change to array to make it easier to assign in Inspector
    public Transform spawnPoint;

    void Start()
    {
        SpawnSelectedCar();
    }

    void SpawnSelectedCar()
    {
        // Get saved car index
        int selectedIndex = PlayerPrefs.GetInt("SelectedCarIndex", 0);
        Debug.Log("Retrieved car index: " + selectedIndex);

        // Verify prefabs are assigned
        if (carPrefabs == null || carPrefabs.Length == 0)
        {
            Debug.LogError("No car prefabs assigned to CarSpawner!");
            return;
        }

        // Verify index
        if (selectedIndex >= 0 && selectedIndex < carPrefabs.Length)
        {
            // Verify the prefab at this index exists
            if (carPrefabs[selectedIndex] != null)
            {
                GameObject car = Instantiate(carPrefabs[selectedIndex],
                                          spawnPoint.position,
                                          spawnPoint.rotation);
                Debug.Log("Successfully spawned car: " + carPrefabs[selectedIndex].name);
            }
            else
            {
                Debug.LogError("Car prefab at index " + selectedIndex + " is null!");
            }
        }
        else
        {
            Debug.LogError($"Invalid car index: {selectedIndex}. Number of prefabs: {carPrefabs.Length}");
        }
    }
}
