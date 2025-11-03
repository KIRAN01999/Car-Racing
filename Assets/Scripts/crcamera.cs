using UnityEngine;

public class crcamera : MonoBehaviour
{
    public Transform target; // The car to follow
    public Vector3 offset = new Vector3(0, 2, -4); // Closer to the car
    public float followSpeed = 10f;
    public float rotationSpeed = 5f;

    void Start()
    {
        CarController.OnCarSpawned += SetTarget;
        FindCar();
    }

    void FindCar()
    {
        GameObject car = GameObject.FindGameObjectWithTag("SelectedCar");
        if (car != null)
        {
            SetTarget(car.transform);
        }
    }

    void SetTarget(Transform carTransform)
    {
        target = carTransform;
        Debug.Log("Camera now following: " + target.name);
    }

    void LateUpdate()
    {
        if (target == null) return;

        // Smoothly follow the car (tighter follow)
        Vector3 targetPosition = target.position + target.TransformDirection(offset);
        transform.position = Vector3.Lerp(transform.position, targetPosition, followSpeed * Time.deltaTime);

        // Keep the camera looking at the car
        Quaternion targetRotation = Quaternion.LookRotation(target.position - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    void OnDestroy()
    {
        CarController.OnCarSpawned -= SetTarget;
    }
}
