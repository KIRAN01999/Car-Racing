using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public enum GearState
{
    Neutral,
    Running,
    CheckingChange,
    Changing
};

public class CarController : MonoBehaviour
{
    private Rigidbody playerRB;
    public WheelColliders colliders;
    public WheelMeshes wheelMeshes;
    public WheelParticles wheelParticles;
    private float gasInput;
    private float brakeInput;
    private float steeringInput;
    public GameObject smokePrefab;
    public float motorPower;
    public float brakePower;
    public float slipAngle;
    public float speed;
    private float speedClamped;
    public float maxSpeed;
    public AnimationCurve steeringCurve;

    public int isEngineRunning;

    public float RPM;
    public float redLine;
    public float idleRPM;
    public TMP_Text rpmText;
    public TMP_Text gearText;
    public Transform rpmNeedle;
    public float minNeedleRotation;
    public float maxNeedleRotation;
    public int currentGear;

    public float[] gearRatios;
    public float differentialRatio;
    private float currentTorque;
    private float clutch;
    private float wheelRPM;
    public AnimationCurve hpToRPMCurve;
    private GearState gearState;
    public float increaseGearRPM;
    public float decreaseGearRPM;
    public float changeGearTime = 0.5f;

    public GameObject tireTrail;
    public Material brakeMaterial;
    public Color brakingColor;
    public float brakeColorIntensity;

    private float gearShiftCooldown = 0.7f;  // Seconds between gear shifts
    private float lastGearShiftTime = -999f;
    private bool isShifting = false;
    private float smoothedRPM = 0f;
    private float maxRPMPerGear = 1000f;

    public ParticleSystem[] gearShiftEffects;

    public GearState CurrentGearState { get { return gearState; } }

    public static event System.Action<Transform> OnCarSpawned;

    public float throttle;
    public float steering;
    public float clutchh;
    public float handBrake;



    // Start is called before the first frame update
    void Start()
    {
        playerRB = gameObject.GetComponent<Rigidbody>();
        gameObject.tag = "SelectedCar";

        OnCarSpawned?.Invoke(transform);
        // Ensure engine is running when the game starts
        if (isEngineRunning == 0)
        {
            isEngineRunning = 1; // Set the engine to running
        }

        // Check gear change on startup (optional)
        if (RPM > increaseGearRPM && currentGear < gearRatios.Length - 1)
        {
            StartCoroutine(ChangeGear(1));  // Automatically change to the next gear if RPM is high enough
        }

        InitiateParticles();
        StartCoroutine(GearShiftEffectLoop());

    }
    

    void InitiateParticles()
    {
        if (smokePrefab)
        {
            wheelParticles.FRWheel = Instantiate(smokePrefab, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.FLWheel = Instantiate(smokePrefab, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RRWheel = Instantiate(smokePrefab, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
                .GetComponent<ParticleSystem>();
            wheelParticles.RLWheel = Instantiate(smokePrefab, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
                .GetComponent<ParticleSystem>();
        }
        if (tireTrail)
        {
            wheelParticles.FRWheelTrail = Instantiate(tireTrail, colliders.FRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FRWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.FLWheelTrail = Instantiate(tireTrail, colliders.FLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.FLWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RRWheelTrail = Instantiate(tireTrail, colliders.RRWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RRWheel.transform)
                .GetComponent<TrailRenderer>();
            wheelParticles.RLWheelTrail = Instantiate(tireTrail, colliders.RLWheel.transform.position - Vector3.up * colliders.FRWheel.radius, Quaternion.identity, colliders.RLWheel.transform)
                .GetComponent<TrailRenderer>();
        }
    }
    // Update is called once per frame

    void Update()
    {

        float wheelRPM = (colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f;
        float rawRPM = Mathf.Abs(wheelRPM) * gearRatios[currentGear] * differentialRatio;
        speed = playerRB.velocity.magnitude * 3.6f;

        // RPM Calculation
        if (gearState == GearState.Changing)
        {
            RPM = Mathf.Lerp(RPM, idleRPM * 1.5f, Time.deltaTime * 5f);
        }
        else
        {
            float targetRPM;
            if (gasInput > 0)
            {
                float maxRPMForGear = redLine * 0.95f;
                float baseRPM = Mathf.Max(rawRPM, idleRPM);
                float accelerationFactor = 3000f;

                targetRPM = baseRPM + (gasInput * accelerationFactor * Time.deltaTime);
                targetRPM = Mathf.Min(targetRPM, maxRPMForGear);
            }
            else
            {
                if (speed < 1f)
                {
                    float idleVariation = Mathf.Sin(Time.time * 5f) * 50f;
                    targetRPM = idleRPM + idleVariation;
                }
                else
                {
                    targetRPM = Mathf.Max(rawRPM, idleRPM);
                }
            }

            RPM = Mathf.Lerp(RPM, targetRPM, Time.deltaTime * 3f);
        }

        // Ensure minimum RPM when engine running
        if (isEngineRunning == 1)
        {
            RPM = Mathf.Max(RPM, idleRPM);
        }
        RPM = Mathf.Min(RPM, redLine);

        // Update UI
        float normalizedRPM = (RPM - idleRPM) / (redLine - idleRPM);
        float targetNeedleRotation = Mathf.Lerp(minNeedleRotation, maxNeedleRotation, normalizedRPM);
        rpmNeedle.rotation = Quaternion.Euler(0, 0, targetNeedleRotation);
        rpmText.text = RPM.ToString("0,000") + "rpm";
        gearText.text = (gearState == GearState.Neutral) ? "N" : (currentGear + 1).ToString();

        // Enhanced gear change logic for stopping
        if (Time.time - lastGearShiftTime > gearShiftCooldown && !isShifting && gearState == GearState.Running)
        {
            // Speed thresholds for each gear (km/h)
            float[] minSpeedForGear = new float[] { 0, 15, 30, 50, 70, 90 };
            float[] maxSpeedForGear = new float[] { 20, 40, 60, 80, 100, 120 };

            // Check for downshifting while stopping
            if (speed < minSpeedForGear[currentGear] ||
                (gasInput < 0.1f && RPM <= decreaseGearRPM && currentGear > 0))
            {
                // Find the appropriate gear for current speed
                int targetGear = 0;
                for (int i = 0; i < minSpeedForGear.Length; i++)
                {
                    if (speed > minSpeedForGear[i] && speed <= maxSpeedForGear[i])
                    {
                        targetGear = i;
                    }
                }

                // If we need to shift down multiple gears
                if (targetGear < currentGear)
                {
                    StartCoroutine(ChangeGear(-1)); // Shift down one gear at a time
                    lastGearShiftTime = Time.time;
                }
            }
            // Normal upshift logic
            else if (RPM >= increaseGearRPM && currentGear < gearRatios.Length - 1 && gasInput > 0.5f)
            {
                if (speed >= minSpeedForGear[currentGear + 1])
                {
                    StartCoroutine(ChangeGear(1));
                    lastGearShiftTime = Time.time;
                }
            }
        }

        // Auto shift to first gear when almost stopped
        if (speed < 5f && currentGear > 1 && Time.time - lastGearShiftTime > gearShiftCooldown)
        {
            StartCoroutine(ChangeGear(-1));
            lastGearShiftTime = Time.time;
        }


        // Call methods for input and other vehicle mechanics
        CheckInput();
        ApplyMotor();
        ApplySteering();
        ApplyBrake();
        CheckParticles();
        ApplyWheelPositions();

    }
    void CheckInput()
    {
        gasInput = Input.GetAxis("Vertical");
        steeringInput = Input.GetAxis("Horizontal");

        // Calculate slip angle and moving direction
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity - transform.forward);
        float movingDirection = Vector3.Dot(transform.forward, playerRB.velocity);

        // Handle gear states
        if (gearState != GearState.Changing)
        {
            if (gearState == GearState.Neutral)
            {
                clutch = 0;
                if (Mathf.Abs(gasInput) > 0)
                {
                    gearState = GearState.Running;
                }
            }
            else
            {
                clutch = Input.GetKey(KeyCode.LeftShift) ? 0 : Mathf.Lerp(clutch, 1, Time.deltaTime);
            }
        }
        else
        {
            clutch = 0;
        }

        // Determine braking input
        if (movingDirection < -0.5f && gasInput > 0)
        {
            brakeInput = Mathf.Abs(gasInput); // Apply brakes when moving backward and gas is pressed forward
        }
        else if (movingDirection > 0.5f && gasInput < 0)
        {
            brakeInput = Mathf.Abs(gasInput); // Apply brakes when moving forward and gas is pressed backward
        }
        else
        {
            brakeInput = 0; // No braking
        }



        /*
        old tutorial code
        if (slipAngle < 120f) {
            if (gasInput < 0)
            {
                brakeInput = Mathf.Abs( gasInput);
                gasInput = 0;
            }
            else
            {
                brakeInput = 0;
            }
        }
        else
        {
            brakeInput = 0;
        }*/

    }
    private float CalculateRPMInGear(int gear)
    {
        if (gear < 0 || gear >= gearRatios.Length) return 0f;

        float wheelRPM = (colliders.RRWheel.rpm + colliders.RLWheel.rpm) / 2f;
        return Mathf.Abs(wheelRPM) * gearRatios[gear] * differentialRatio;
    }
    void ApplyBrake()
    {
        // Calculate braking torque for front and rear wheels
        float frontBrakeTorque = brakeInput * brakePower * 0.7f;
        float rearBrakeTorque = brakeInput * brakePower * 0.3f;

        // Apply braking torque to the wheels
        colliders.FRWheel.brakeTorque = frontBrakeTorque;
        colliders.FLWheel.brakeTorque = frontBrakeTorque;
        colliders.RRWheel.brakeTorque = rearBrakeTorque;
        colliders.RLWheel.brakeTorque = rearBrakeTorque;

        // Handle brake light material emission
        if (brakeMaterial != null)
        {
            if (brakeInput > 0)
            {
                // Enable emission for brake lights
                if (!brakeMaterial.IsKeywordEnabled("_EMISSION"))
                    brakeMaterial.EnableKeyword("_EMISSION");

                // Set emission color for braking effect
                brakeMaterial.SetColor("_EmissionColor", brakingColor * Mathf.Pow(2, brakeColorIntensity));
            }
            else
            {
                // Disable emission when not braking
                if (brakeMaterial.IsKeywordEnabled("_EMISSION"))
                    brakeMaterial.DisableKeyword("_EMISSION");

                brakeMaterial.SetColor("_EmissionColor", Color.black);
            }
        }


    }
    void ApplyMotor()
    {
        currentTorque = CalculateTorque();

        // Log torque to see if it's being applied correctly
        Debug.Log("Current Torque: " + currentTorque);

        // If the car is in reverse (gasInput is negative), apply negative torque
        if (gasInput < 0)
        {
            // Apply reverse torque
            colliders.RRWheel.motorTorque = Mathf.Min(0, currentTorque * gasInput); // Negative torque for reverse
            colliders.RLWheel.motorTorque = Mathf.Min(0, currentTorque * gasInput); // Negative torque for reverse
        }
        else
        {
            // Apply forward torque
            colliders.RRWheel.motorTorque = Mathf.Max(0, currentTorque * gasInput); // Positive torque for forward
            colliders.RLWheel.motorTorque = Mathf.Max(0, currentTorque * gasInput); // Positive torque for forward
        }
    }

    float CalculateTorque()
    {
        float torque = 0;

        // Ensure engine is running and RPM is valid
        if (isEngineRunning > 0)
        {
            // Get the wheel RPM and ensure it's non-negative
            float wheelRPM = Mathf.Max(0, colliders.RRWheel.rpm); // Ensure wheelRPM is non-negative

            // RPM Calculation (ensure it's non-negative and not zero)
            RPM = Mathf.Max(idleRPM, RPM); // Prevent negative RPM
            RPM = Mathf.Max(RPM, 100); // Ensure RPM doesn't drop to zero

            // Log RPM for debugging
            Debug.Log("Current RPM: " + RPM);

            // Ensure motorPower and clutch are not zero
            if (motorPower == 0)
            {
                Debug.LogWarning("motorPower is zero! Please check the motorPower value.");
            }
            if (clutch == 0)
            {
                Debug.LogWarning("Clutch is zero! Please check the clutch value.");
            }

            // Calculate torque, making sure RPM is never zero
            if (RPM > 0)
            {
                // Debug the hpToRPMCurve evaluation
                float hpAtRPM = hpToRPMCurve.Evaluate(RPM / redLine);
                Debug.Log("hpToRPMCurve Value: " + hpAtRPM);

                torque = hpAtRPM * motorPower / RPM * gearRatios[currentGear] * differentialRatio * 5252f * clutch;
            }
            else
            {
                Debug.LogWarning("RPM is zero or invalid, not calculating torque.");
            }

            // Log the torque value for debugging
            Debug.Log("Calculated Torque: " + torque);
        }
        else
        {
            Debug.LogWarning("Engine is not running.");
        }

        return torque;
    }

    void ApplySteering()
    {

        // Update speed and slip angle
        speed = playerRB.velocity.magnitude;
        slipAngle = Vector3.Angle(transform.forward, playerRB.velocity);

        // Calculate steering angle
        float steeringAngle = steeringInput * steeringCurve.Evaluate(speed);

        // Adjust steering for slip angle if within range
        if (slipAngle < 120f)
        {
            steeringAngle += Vector3.SignedAngle(transform.forward, playerRB.velocity + transform.forward, Vector3.up);
        }

        // Clamp steering angle
        steeringAngle = Mathf.Clamp(steeringAngle, -90f, 90f);

        // Apply to front wheels
        colliders.FRWheel.steerAngle = steeringAngle;
        colliders.FLWheel.steerAngle = steeringAngle;
    }

    void ApplyWheelPositions()
    {
        UpdateWheel(colliders.FRWheel, wheelMeshes.FRWheel);
        UpdateWheel(colliders.FLWheel, wheelMeshes.FLWheel);
        UpdateWheel(colliders.RRWheel, wheelMeshes.RRWheel);
        UpdateWheel(colliders.RLWheel, wheelMeshes.RLWheel);
    }
    void CheckParticles()
    {
        WheelHit[] wheelHits = new WheelHit[4];
        colliders.FRWheel.GetGroundHit(out wheelHits[0]);
        colliders.FLWheel.GetGroundHit(out wheelHits[1]);

        colliders.RRWheel.GetGroundHit(out wheelHits[2]);
        colliders.RLWheel.GetGroundHit(out wheelHits[3]);

        float slipAllowance = 0.2f;
        if ((Mathf.Abs(wheelHits[0].sidewaysSlip) + Mathf.Abs(wheelHits[0].forwardSlip) > slipAllowance))
        {
            wheelParticles.FRWheel.Play();
            wheelParticles.FRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FRWheel.Stop();

            wheelParticles.FRWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[1].sidewaysSlip) + Mathf.Abs(wheelHits[1].forwardSlip) > slipAllowance))
        {
            wheelParticles.FLWheel.Play();

            wheelParticles.FLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.FLWheel.Stop();

            wheelParticles.FLWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[2].sidewaysSlip) + Mathf.Abs(wheelHits[2].forwardSlip) > slipAllowance))
        {
            wheelParticles.RRWheel.Play();

            wheelParticles.RRWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RRWheel.Stop();

            wheelParticles.RRWheelTrail.emitting = false;
        }
        if ((Mathf.Abs(wheelHits[3].sidewaysSlip) + Mathf.Abs(wheelHits[3].forwardSlip) > slipAllowance))
        {
            wheelParticles.RLWheel.Play();

            wheelParticles.RLWheelTrail.emitting = true;
        }
        else
        {
            wheelParticles.RLWheel.Stop();

            wheelParticles.RLWheelTrail.emitting = false;
        }


    }
    void UpdateWheel(WheelCollider coll, MeshRenderer wheelMesh)
    {
        Quaternion quat;
        Vector3 position;
        coll.GetWorldPose(out position, out quat);
        wheelMesh.transform.position = position;
        wheelMesh.transform.rotation = quat;
    }
    public void SetInput(float throttle, float steering, float clutch, float handBrake)
    {
        this.throttle = throttle;
        this.steering = steering;
        this.clutchh = clutch;
        this.handBrake = handBrake;
    }
        public float GetSpeedRatio()
    {
        var gas = Mathf.Clamp(Mathf.Abs(gasInput), 0.5f, 1f);
        return RPM * gas / redLine;
    }
    IEnumerator ChangeGear(int gearChange)
    {
        if (gearState == GearState.Changing) yield break;

        gearState = GearState.Changing;
        float startRPM = RPM;
        clutch = 0;

        float gearChangeProgress = 0f;
        float actualChangeTime = changeGearTime * (1 + (currentGear * 0.1f));
       
        // RPM behavior during gear change
        while (gearChangeProgress < actualChangeTime)
        {
            if (gearChange > 0)
            {
                RPM = Mathf.Lerp(startRPM, startRPM * 0.4f, gearChangeProgress / actualChangeTime);
            }
            else
            {
                RPM = Mathf.Lerp(startRPM, startRPM * 1.2f, gearChangeProgress / actualChangeTime);
            }

            gearChangeProgress += Time.deltaTime;
            yield return null;
        }

        currentGear += gearChange;

        // Calculate new RPM
        float newRPM;
        if (gearChange > 0)
        {
            newRPM = startRPM * (gearRatios[currentGear] / gearRatios[currentGear - 1]);
        }
        else
        {
            newRPM = startRPM * (gearRatios[currentGear] / gearRatios[currentGear + 1]);
        }

        // Smooth clutch engagement
        float clutchEngageTime = 0.2f;
        float elapsed = 0f;
        while (elapsed < clutchEngageTime)
        {
            clutch = Mathf.Lerp(0, 1, elapsed / clutchEngageTime);
            RPM = Mathf.Lerp(RPM, newRPM, elapsed / clutchEngageTime);
            elapsed += Time.deltaTime;
            yield return null;
        }

        clutch = 1;
        gearState = GearState.Running;
    }
    IEnumerator GearShiftEffectLoop()
    {
        while (true)
        {
            if (gearShiftEffects.Length > 0)
            {
                foreach (ParticleSystem effect in gearShiftEffects)
                {
                    if (effect != null)
                    {
                        effect.Play();
                    }
                }
                yield return new WaitForSeconds(2f); // Play for 2 seconds

                foreach (ParticleSystem effect in gearShiftEffects)
                {
                    if (effect != null)
                    {
                        effect.Stop();
                    }
                }
            }
            yield return new WaitForSeconds(4f); // Wait 4 seconds before repeating
        }
    }





}
[System.Serializable]
public class WheelColliders
{
    public WheelCollider FRWheel;
    public WheelCollider FLWheel;
    public WheelCollider RRWheel;
    public WheelCollider RLWheel;
}
[System.Serializable]
public class WheelMeshes
{
    public MeshRenderer FRWheel;
    public MeshRenderer FLWheel;
    public MeshRenderer RRWheel;
    public MeshRenderer RLWheel;
}
[System.Serializable]
public class WheelParticles
{
    public ParticleSystem FRWheel;
    public ParticleSystem FLWheel;
    public ParticleSystem RRWheel;
    public ParticleSystem RLWheel;

    public TrailRenderer FRWheelTrail;
    public TrailRenderer FLWheelTrail;
    public TrailRenderer RRWheelTrail;
    public TrailRenderer RLWheelTrail;

}