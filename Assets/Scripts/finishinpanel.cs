using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class finishinpanel : MonoBehaviour
{
    public GameObject finishingPanel;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("SelectedCar")) // car must be tagged as Player
        {
            finishingPanel.SetActive(true);
            Time.timeScale = 0f;
            Debug.Log("Panel activated from cube!");
        }
    }
}
