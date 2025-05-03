using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    public bool isRaining = true; // Only plays thunder if true

    public AudioSource AS;
    public AudioClip rain;
    public AudioClip[] thunderStromeSFXs;
    public GameObject RainParticle;
    public Light directionalLight; // Assign your directional light here
    public float flashIntensity = 2f; // How bright the flash is
    public float flashDuration = 0.1f; // How long the flash lasts

    private float timer = 0f;
    private float nextThunderTime;
    private float originalLightIntensity;

    void Start()
    {
        if (directionalLight != null)
        {
            originalLightIntensity = directionalLight.intensity;
        }

        SetNextThunderTime();
    }

    void Update()
    {
        if (!isRaining) return;

        RainParticle.SetActive(true);
        timer += Time.deltaTime;

        if (timer >= nextThunderTime)
        {
            PlayRandomThunder();
            StartCoroutine(LightFlash());
            SetNextThunderTime();
        }
    }

    void SetNextThunderTime()
    {
        timer = 0f;
        nextThunderTime = Random.Range(10f, 20f);
    }

    void PlayRandomThunder()
    {
        if (thunderStromeSFXs.Length > 0)
        {
            int index = Random.Range(0, thunderStromeSFXs.Length);
            
            AS.PlayOneShot(thunderStromeSFXs[index]);
        }
    }

    IEnumerator LightFlash()
    {
        if (directionalLight != null)
        {
            directionalLight.intensity = flashIntensity;
            yield return new WaitForSeconds(flashDuration);
            directionalLight.intensity = originalLightIntensity;
        }
    }
}
