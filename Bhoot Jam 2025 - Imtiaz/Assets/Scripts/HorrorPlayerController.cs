using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class HorrorPlayerControllerURP : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float walkSpeed = 2.5f;
    [SerializeField] private float sprintSpeed = 4f;
    [SerializeField] private float crouchSpeed = 1.5f;
    [SerializeField] private float crouchHeight = 1f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float gravity = -9.81f;

    [Header("Head Bobbing")]
    [SerializeField] private float bobAmount = 0.05f;
    [SerializeField] private float bobSpeed = 10f;
    private float defaultYPos;
    private float timer = 0;

    [Header("Audio")]
    [SerializeField] private AudioClip[] footstepSounds;
    [SerializeField] private AudioClip heartbeatSound;
    [SerializeField] private AudioClip breathSound;
    private AudioSource audioSource;

    [Header("Visual Effects")]
    [SerializeField] private float baseFlashlightIntensity = 2.0f;
    [SerializeField] private float maxFlashlightIntensity = 2.0f;
    [SerializeField] private Light flashlight;
    [SerializeField] private float flickerDuration = 0.1f;
    [SerializeField] private Volume globalVolume;
    [SerializeField] private float maxSanity = 100f;

    // URP Volume Components
    private Vignette vignette;
    private FilmGrain filmGrain;
    private ChromaticAberration chromaticAberration;
    private LensDistortion lensDistortion;

    private CharacterController controller;
    private Vector3 velocity;
    private bool isCrouching = false;
    private bool isSprinting = false;
    public float currentSanity;
    private float defaultFOV;
    public Camera playerCamera;
    private TaskManager taskManager;
    private bool captionSpamPrevent = false;
    public GameObject dieScreen;
    public TMPro.TMP_Text dieReason;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        taskManager = GetComponent<TaskManager>();
        audioSource = GetComponent<AudioSource>();
        defaultYPos = playerCamera.transform.localPosition.y;
        defaultFOV = playerCamera.fieldOfView;
        currentSanity = maxSanity;

        // Get URP volume components
        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out vignette);
            globalVolume.profile.TryGet(out filmGrain);
            globalVolume.profile.TryGet(out chromaticAberration);
            globalVolume.profile.TryGet(out lensDistortion);
        }

        StartCoroutine(FlashlightFlicker());
        StartCoroutine(RandomSanityEffects());
    }

    private void Update()
    {
        HandleMovement();
        HandleHeadBob();
        HandleFootsteps();
        HandleSanity();
        HandleFlashlight();
        HandleBreathing();

        if (!captionSpamPrevent && (
                Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f ||
                Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f
            ))
        {
            taskManager.caption.text = "";
            captionSpamPrevent = true;
        }

        if (Input.GetKeyDown(KeyCode.F)) ToggleFlashlight();
        //if (Input.GetKeyDown(KeyCode.C)) ToggleCrouch();
    }

    private void HandleMovement()
    {
        if (currentSanity < 30f)
        {
            // Add movement resistance when sanity is low
            float resistance = 1f + (30f - currentSanity) / 30f;
            walkSpeed = Mathf.Lerp(2.5f, 1.5f, (30f - currentSanity) / 30f);
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;

        float speed = isCrouching ? crouchSpeed : walkSpeed;
        isSprinting = Input.GetKey(KeyCode.LeftShift) && !isCrouching && velocity.magnitude > 0;

        if (isSprinting)
        {
            speed = sprintSpeed;
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV + 10f, Time.deltaTime * 5f);
        }
        else
        {
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, defaultFOV, Time.deltaTime * 5f);
        }

        controller.Move(move * speed * Time.deltaTime);

        // Jump
        if (Input.GetButtonDown("Jump") && controller.isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpForce * -2f * gravity);
        }

        // Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void HandleHeadBob()
    {
        if (controller.isGrounded && (Mathf.Abs(Input.GetAxis("Horizontal")) > 0.1f || Mathf.Abs(Input.GetAxis("Vertical")) > 0.1f))
        {
            timer += Time.deltaTime * (isSprinting ? bobSpeed * 1.5f : bobSpeed);
            playerCamera.transform.localPosition = new Vector3(
                playerCamera.transform.localPosition.x,
                defaultYPos + Mathf.Sin(timer) * bobAmount * (isCrouching ? 0.5f : 1f),
                playerCamera.transform.localPosition.z
            );
        }
        else
        {
            timer = 0;
            playerCamera.transform.localPosition = Vector3.Lerp(
                playerCamera.transform.localPosition,
                new Vector3(playerCamera.transform.localPosition.x, defaultYPos, playerCamera.transform.localPosition.z),
                Time.deltaTime * bobSpeed
            );
        }
    }

    private void HandleFootsteps()
    {
        if (controller.isGrounded && (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0))
        {
            if (!audioSource.isPlaying)
            {
                float delay = isSprinting ? 0.3f : (isCrouching ? 0.7f : 0.5f);
                audioSource.clip = footstepSounds[Random.Range(0, footstepSounds.Length)];
                audioSource.pitch = Random.Range(0.8f, 1.2f);
                audioSource.volume = isSprinting ? 0.6f : (isCrouching ? 0.2f : 0.4f);
                audioSource.PlayDelayed(delay);
            }
        }
    }

    private void HandleBreathing()
    {
        if (isSprinting && !audioSource.isPlaying && currentSanity < 70f)
        {
            audioSource.PlayOneShot(breathSound, Mathf.Lerp(0.1f, 0.5f, (70f - currentSanity) / 70f));
        }
    }

    private void HandleSanity()
    {
       /*
        if (vignette != null)
        {
            vignette.intensity.value = Mathf.Lerp(0.3f, 0.6f, 1 - (currentSanity / maxSanity));
            vignette.smoothness.value = Mathf.Lerp(0.2f, 0.8f, 1 - (currentSanity / maxSanity));
        }

        if (filmGrain != null)
        {
            filmGrain.intensity.value = Mathf.Lerp(0.1f, 0.4f, 1 - (currentSanity / maxSanity));
        }

        if (chromaticAberration != null)
        {
            chromaticAberration.intensity.value = Mathf.Lerp(0.1f, 0.5f, 1 - (currentSanity / maxSanity));
        }

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = Mathf.Lerp(0f, -0.3f, 1 - (currentSanity / maxSanity));
        }*/

        if (currentSanity < 30f && !audioSource.isPlaying && Random.value > 0.95f)
        {
            audioSource.PlayOneShot(heartbeatSound, Mathf.Lerp(0.3f, 0.8f, (30f - currentSanity) / 30f));
        }
    }

    private void HandleFlashlight()
    {
        if (flashlight.enabled && currentSanity > 0)
        {
            //currentSanity -= Time.deltaTime * 2f;
            flashlight.intensity = Mathf.Lerp(baseFlashlightIntensity, maxFlashlightIntensity, Mathf.PingPong(Time.time * 0.5f, 1));
        }
    }

    private IEnumerator FlashlightFlicker()
    {
        while (true)
        {
            if (flashlight.enabled)
            {
                // Wait for a random interval between 5 seconds and 30 seconds
                float randomDelay = Random.Range(2f, 5f);
                yield return new WaitForSeconds(randomDelay);

                // Check if the flashlight is still enabled after waiting
                if (!flashlight.enabled)
                    continue;

                // Log to check if flickering is happening
                Debug.Log("Flickering flashlight...");

                // Turn off the flashlight
                flashlight.enabled = false;
                yield return new WaitForSeconds(flickerDuration);  // Duration for light to stay off

                // Turn on the flashlight again
                flashlight.enabled = true;

                // Occasionally do a double flicker (turn off and on again quickly)
                if (Random.value > 0.7f)
                {
                    yield return new WaitForSeconds(0.1f);  // Short wait before double flicker
                    flashlight.enabled = false;
                    yield return new WaitForSeconds(flickerDuration * 0.5f);  // Shorter off time for double flicker
                    flashlight.enabled = true;
                }
            }
            yield return null;
        }
    }


    private IEnumerator RandomSanityEffects()
    {
        while (true)
        {
            if (currentSanity < 50f)
            {
                float waitTime = Random.Range(10f, 30f) * (currentSanity / 50f);
                yield return new WaitForSeconds(waitTime);

                // Random camera twitch
                if (currentSanity < 30f && Random.value > 0.5f)
                {
                    Vector3 originalPos = playerCamera.transform.localPosition;
                    playerCamera.transform.localPosition += new Vector3(
                        Random.Range(-0.05f, 0.05f),
                        Random.Range(-0.03f, 0.03f),
                        0
                    );
                    yield return new WaitForSeconds(0.1f);
                    playerCamera.transform.localPosition = originalPos;
                }

                // Brief visual distortion
                if (lensDistortion != null)
                {
                    float originalDistortion = lensDistortion.intensity.value;
                    lensDistortion.intensity.value = Random.Range(-0.5f, -0.7f);
                    yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
                    lensDistortion.intensity.value = originalDistortion;
                }
            }
            else
            {
                yield return new WaitForSeconds(10f);
            }
        }
    }

    private void ToggleFlashlight()
    {
        flashlight.enabled = !flashlight.enabled;

        if (!captionSpamPrevent)
        {
            taskManager.caption.text = "";
            captionSpamPrevent = true;
        }
    }

    private void ToggleCrouch()
    {
        isCrouching = !isCrouching;
        controller.height = isCrouching ? crouchHeight : 2f;
        playerCamera.transform.localPosition = new Vector3(
            playerCamera.transform.localPosition.x,
            isCrouching ? defaultYPos - 0.5f : defaultYPos,
            playerCamera.transform.localPosition.z
        );
    }

    public void ApplySanityDamage(float amount)
    {
        currentSanity = Mathf.Clamp(currentSanity - amount, 0, maxSanity);
        StartCoroutine(SanityImpactEffect());
    }

    private IEnumerator SanityImpactEffect()
    {
        if (chromaticAberration != null)
        {
            float originalChroma = chromaticAberration.intensity.value;
            chromaticAberration.intensity.value = 0.8f;
            yield return new WaitForSeconds(0.5f);
            chromaticAberration.intensity.value = originalChroma;
        }
    }

    public void Die()
    {
        StartCoroutine(DieScreenAppear());
        enabled = false;
    }
    private IEnumerator DieScreenAppear()
    {
        yield return new WaitForSecondsRealtime(1f);
        dieScreen.SetActive(true);
    }
}