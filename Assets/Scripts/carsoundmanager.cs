using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class carsoundmanager : MonoBehaviour
{

    [Header("Audio Sources")]
    public AudioSource engineSource;
    public AudioSource transmissionSource;
    public AudioSource skidSource;

    [Header("Engine Audio Clips")]
    public AudioClip[] engineClipsPerGear;  // Array of engine sounds for each gear
    public AudioClip idleSound;
    public AudioClip revDownSound;
    public AudioClip revUpSound;

    [Header("Transmission Audio")]
    public AudioClip gearShiftSound;
    public AudioClip reverseGearSound;

    [Header("Tire Audio")]
    public AudioClip tireSkidSound;

    [Header("Audio Parameters")]
    [Range(0f, 1f)]
    public float minEnginePitch = 0.5f;
    [Range(1f, 2f)]
    public float maxEnginePitch = 1.5f;
    [Range(0f, 1f)]
    public float minEngineVolume = 0.2f;
    [Range(0f, 1f)]
    public float maxEngineVolume = 1.0f;

    private CarController carController;
    private float targetPitch;
    private float currentPitch;
    private float pitchSmoothing = 5f;
    private int lastGear = -1;
    private bool isShifting = false;

    private void Start()
    {
        carController = GetComponent<CarController>();

        if (!engineSource || !transmissionSource || !skidSource)
        {
            Debug.LogError("Audio sources not assigned to CarAudioManager!");
            return;
        }

        // Setup initial engine sound
        if (idleSound)
        {
            engineSource.clip = idleSound;
            engineSource.loop = true;
            engineSource.Play();
        }

        // Setup continuous skid sound
        if (tireSkidSound)
        {
            skidSource.clip = tireSkidSound;
            skidSource.loop = true;
            skidSource.volume = 0;
        }
    }

    private void Update()
    {
        if (!carController || !engineSource) return;

        UpdateEngineSound();
        UpdateGearShiftSound();
        UpdateSkidSound();
    }

    private void UpdateEngineSound()
    {
        float speedRatio = carController.GetSpeedRatio();

        // Calculate target pitch based on RPM
        targetPitch = Mathf.Lerp(minEnginePitch, maxEnginePitch, speedRatio);
        currentPitch = Mathf.Lerp(currentPitch, targetPitch, Time.deltaTime * pitchSmoothing);

        // Apply pitch and volume
        engineSource.pitch = currentPitch;
        engineSource.volume = Mathf.Lerp(minEngineVolume, maxEngineVolume, speedRatio * 0.8f);

        // Change engine sound based on gear if we have different clips per gear
        if (engineClipsPerGear != null && engineClipsPerGear.Length > 0)
        {
            int currentGear = carController.currentGear;
            if (currentGear >= 0 && currentGear < engineClipsPerGear.Length && currentGear != lastGear)
            {
                StartCoroutine(CrossfadeToNewEngineSound(engineClipsPerGear[currentGear]));
                lastGear = currentGear;
            }
        }
    }

    private void UpdateGearShiftSound()
    {
        if (carController.CurrentGearState == GearState.Changing && !isShifting)
        {
            isShifting = true;
            if (gearShiftSound)
            {
                transmissionSource.PlayOneShot(gearShiftSound);
            }
            StartCoroutine(ResetShiftingFlag());
        }
    }

    private void UpdateSkidSound()
    {
        // Get wheel colliders from car controller
        WheelCollider[] wheels = new WheelCollider[]
        {
            carController.colliders.FLWheel,
            carController.colliders.FRWheel,
            carController.colliders.RLWheel,
            carController.colliders.RRWheel
        };

        float totalSlip = 0f;
        foreach (WheelCollider wheel in wheels)
        {
            WheelHit hit;
            if (wheel.GetGroundHit(out hit))
            {
                totalSlip += Mathf.Abs(hit.sidewaysSlip) + Mathf.Abs(hit.forwardSlip);
            }
        }

        // Normalize slip value and apply to skid sound
        float slipIntensity = Mathf.Clamp01(totalSlip / 4f);
        skidSource.volume = Mathf.Lerp(skidSource.volume, slipIntensity, Time.deltaTime * 10f);

        if (skidSource.volume > 0.01f && !skidSource.isPlaying)
        {
            skidSource.Play();
        }
        else if (skidSource.volume <= 0.01f && skidSource.isPlaying)
        {
            skidSource.Stop();
        }
    }

    private IEnumerator CrossfadeToNewEngineSound(AudioClip newClip)
    {
        float crossfadeTime = 0.2f;
        float startVolume = engineSource.volume;

        // Create temporary audio source for crossfade
        AudioSource tempSource = gameObject.AddComponent<AudioSource>();
        tempSource.clip = newClip;
        tempSource.loop = true;
        tempSource.volume = 0;
        tempSource.pitch = engineSource.pitch;
        tempSource.Play();

        // Crossfade between sources
        float elapsed = 0f;
        while (elapsed < crossfadeTime)
        {
            elapsed += Time.deltaTime;
            float normalizedTime = elapsed / crossfadeTime;

            engineSource.volume = Mathf.Lerp(startVolume, 0f, normalizedTime);
            tempSource.volume = Mathf.Lerp(0f, startVolume, normalizedTime);

            yield return null;
        }

        // Switch to new clip
        engineSource.clip = newClip;
        engineSource.volume = startVolume;
        engineSource.Play();

        // Clean up temporary source
        Destroy(tempSource);
    }

    private IEnumerator ResetShiftingFlag()
    {
        yield return new WaitForSeconds(0.5f);
        isShifting = false;
    }

    // Call this when starting the engine
    public void StartEngine()
    {
        if (revUpSound)
        {
            engineSource.PlayOneShot(revUpSound);
        }
    }

    // Call this when stopping the engine
    public void StopEngine()
    {
        if (revDownSound)
        {
            engineSource.PlayOneShot(revDownSound);
            StartCoroutine(FadeOutEngine());
        }
    }

    private IEnumerator FadeOutEngine()
    {
        float startVolume = engineSource.volume;
        float fadeTime = 1f;
        float elapsed = 0f;

        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            engineSource.volume = Mathf.Lerp(startVolume, 0f, elapsed / fadeTime);
            yield return null;
        }

        engineSource.Stop();
    }
}
