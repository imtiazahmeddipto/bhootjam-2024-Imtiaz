using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class HorrorCameraController : MonoBehaviour
{
    [Header("Look Settings")]
    [SerializeField] private float mouseSensitivity = 100f;
    [SerializeField] private float maxLookAngle = 90f;
    [SerializeField] private float minLookAngle = -90f;
    [SerializeField] private bool invertY = false;

    [Header("Transforms")]
    [SerializeField] private Transform head;              // Handles rotation
    [SerializeField] private Transform cameraTransform;   // Shake only
    [SerializeField] private Transform weaponHolder;      // Weapon position

    [Header("Offsets")]
    [SerializeField] private Vector3 headPositionOffset = Vector3.zero;

    [Header("Bobbing")]
    [SerializeField] private float bobSpeed = 4f;
    [SerializeField] private float bobAmount = 0.05f;

    [Header("Horror Effects")]
    [SerializeField] private float sanityShakeIntensity = 0.1f;
    [SerializeField] private float distortionFrequency = 2f;
    [SerializeField] private Volume globalVolume;

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float bobTimer = 0f;
    private bool isShaking = false;

    private LensDistortion lensDistortion;
    private FilmGrain filmGrain;

    private void Awake()
    {
        Cursor.lockState = CursorLockMode.Locked;

        if (globalVolume != null && globalVolume.profile != null)
        {
            globalVolume.profile.TryGet(out lensDistortion);
            globalVolume.profile.TryGet(out filmGrain);
        }
    }

    private void Update()
    {
        HandleMouseLook();
        ApplyHeadBobbing();
        HandleSanityEffects();  
    }

    private void HandleMouseLook()
    {
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // Horizontal rotation (Y-axis) - Rotate the PLAYER (parent)
        transform.parent.Rotate(Vector3.up * mouseX);

        // Vertical rotation (X-axis) - Rotate the CAMERA/HEAD only
        xRotation += invertY ? mouseY : -mouseY;
        xRotation = Mathf.Clamp(xRotation, minLookAngle, maxLookAngle);

        // Apply only vertical rotation to the head
        head.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
    }

    private void ApplyHeadBobbing()
    {
        float moveInput = Mathf.Abs(Input.GetAxis("Vertical")) + Mathf.Abs(Input.GetAxis("Horizontal"));

        Vector3 bobOffset = Vector3.zero;
        if (moveInput > 0.1f)
        {
            bobTimer += Time.deltaTime * bobSpeed;
            bobOffset.y = Mathf.Sin(bobTimer) * bobAmount;
        }
        else
        {
            bobTimer = 0f;
        }

        head.localPosition = Vector3.Lerp(head.localPosition, headPositionOffset + bobOffset, Time.deltaTime * 10f);
    }

    public void ApplyCameraShake(float duration, float intensity)
    {
        if (!isShaking)
            StartCoroutine(CameraShake(duration, intensity));
    }

    private IEnumerator CameraShake(float duration, float intensity)
    {
        isShaking = true;
        Vector3 originalPos = cameraTransform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float x = Random.Range(-1f, 1f) * intensity;
            float y = Random.Range(-1f, 1f) * intensity;

            cameraTransform.localPosition = originalPos + new Vector3(x, y, 0f);
            elapsed += Time.deltaTime;
            yield return null;
        }

        cameraTransform.localPosition = originalPos;
        isShaking = false;
    }

    private void HandleSanityEffects()
    {
        float sanity = GetComponentInParent<HorrorPlayerControllerURP>().currentSanity;
        float sanityRatio = 1 - (sanity / 100f);

        if (lensDistortion != null)
        {
            lensDistortion.intensity.value = Mathf.Sin(Time.time * distortionFrequency) * sanityRatio * 0.5f;
        }

        if (filmGrain != null && sanity < 40f)
        {
            filmGrain.intensity.value = Mathf.Lerp(0.3f, 0.6f, Random.value * sanityRatio);
        }

        if (sanity < 30f && !isShaking)
        {
            float shake = sanityShakeIntensity * (1 - sanity / 30f);
            cameraTransform.localPosition += new Vector3(
                Random.Range(-shake, shake),
                Random.Range(-shake, shake),
                0f
            );
        }
    }
}
