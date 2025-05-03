using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

public class TaskManager : MonoBehaviour
{
    public TMPro.TMP_Text TaskText;
    public AudioSource playerAS;
    public AudioSource dadiAS;
    public AudioSource SpiritAS;
    public AudioClip dadiRandomTalks, dialogue1Player, dialogue1Dadi, dialogue2Dadi, playerQuestion1, playerQuestion2, playerSpeak1, dadiWhispar1, playerQuestion3, DadiLastSpeech, SpiritTalk, playerLastReply;
    public TMPro.TMP_Text caption;
    public Gun raycastContoller;
    public Animator CupboardAnimator;
    public InteractableCamera interactableCamera;
    public GameObject DadiSitPose, DadiEatPose, DadiLayPose;
    public Collider dadiCollider;
    public BoxCollider DadiBox2;
    public GameObject TableMeat;
    public Rain rainContoller;
    public GameObject lastLocationIndicator;
    public GameObject Pig;
    private void Start()
    {
        TaskText.text = "Go To Your Grandma Room";
        dadiAS.clip = dadiRandomTalks;
        dadiAS.loop = true;
        dadiAS.Play();
        caption.text = "Use W, A, S, D to move. Press Shift to run. Press F to ToggleFlashlight.";
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "Trigger1stDialogue")
        {
            StartCoroutine(PlayDialogue1());
            Destroy(other.gameObject);
        }
        if (other.gameObject.name == "TriggerDialogue2nd")
        {
            StartCoroutine(Dialogue6());
            Destroy(other.gameObject);
        }
    }

    IEnumerator PlayDialogue1()
    {
        dadiAS.Stop();
        dadiAS.loop = false;
        playerAS.PlayOneShot(dialogue1Player);
        yield return StartCoroutine(ShowCaption("Grandma, what happened to you? Why are you saying random things?", dialogue1Player.length));

        yield return new WaitForSeconds(dialogue1Player.length);

        dadiAS.PlayOneShot(dialogue1Dadi);
        yield return StartCoroutine(ShowCaption("You pig's son! Go get me some food — I'm hungry!", dialogue1Dadi.length));

        yield return new WaitForSeconds(dialogue1Dadi.length - 2f);
        TaskText.text = "Go to your room and take your Accessories from your bed";
        raycastContoller.canPickUpAccessories = true;
    }
    public IEnumerator Dialogue2()
    {
        dadiAS.PlayOneShot(dialogue2Dadi);
        TaskText.text = "";

        yield return StartCoroutine(ShowCaption("Now, get out of here!", dialogue2Dadi.length));

        CupboardAnimator.SetBool("isShaking", true);
        raycastContoller.canInterectWithCupboard = true;

        yield return new WaitForSeconds(1.5f);
        playerAS.PlayOneShot(playerQuestion1);

        yield return StartCoroutine(ShowCaption("Where is the sound coming from?", playerQuestion1.length));
        TaskText.text = "Find The Sound Source";
    }

    public IEnumerator Dialogue3()
    {
        TaskText.text = "";
        DadiSitPose.SetActive(false);
        DadiEatPose.SetActive(true);
        yield return new WaitForSeconds(1.5f);

        playerAS.PlayOneShot(playerQuestion2);
        TaskText.text = "Pick up the note.";
        raycastContoller.canPickUpNote = true;
    }

    public IEnumerator Dialogue4()
    {
        yield return new WaitForSeconds(0.5f);

        playerAS.PlayOneShot(playerSpeak1);
        caption.text = "You can use your camera to highlight important items!";
        Pig.SetActive(true);
        TaskText.text = "Collect Ingredients:";
        raycastContoller.canPickUpIngredients = true;
        interactableCamera.captionSpamPrevent = false;

    }

    public IEnumerator Dialogue5()
    {
        yield return new WaitForSeconds(0.5f);

        dadiAS.spatialBlend = 0;
        dadiAS.volume = 1f;
        dadiAS.PlayOneShot(dadiWhispar1);

        yield return StartCoroutine(ShowCaption("Russel, Come Here!", dadiWhispar1.length + .5f));

        dadiAS.spatialBlend = 1;
        dadiAS.volume = 1f;
        playerAS.PlayOneShot(playerQuestion3);

        yield return StartCoroutine(ShowCaption("Is Grandma calling me?", playerQuestion3.length));

        TaskText.text = "Go to Dadi's Room";
        DadiBox2.gameObject.SetActive(true);
    }

    public IEnumerator Dialogue6()
    {
        TaskText.text = "";
        yield return new WaitForSeconds(0.5f);

        dadiAS.PlayOneShot(DadiLastSpeech);

        yield return StartCoroutine(ShowCaption("..........", DadiLastSpeech.length + .5f));

        TaskText.text = "Go to the location and use your camera to locate it.";
        lastLocationIndicator.SetActive(true);
        raycastContoller.canPerformLast = true;
    }

    public Light[] candles;
    public IEnumerator AfterDrop()
    {
        TaskText.text = "";
        foreach (Light candle in candles)
        {
            StartCoroutine(FadeInLight(candle, 2f, 1f)); // target intensity 1 over 1 second
        }

        yield return new WaitForSeconds(1f);
        DestroyAllNavAgentsInScene();
        rainContoller.isRaining = true;
        StartCoroutine(Dialogue7());
    }

    public IEnumerator Dialogue7()
    {
        yield return new WaitForSeconds(7f);
        SpiritAS.PlayOneShot(SpiritTalk);

        yield return StartCoroutine(ShowCaption("..........", SpiritTalk.length + 1f));

        playerAS.PlayOneShot(playerLastReply);

        yield return StartCoroutine(ShowCaption("..........", playerLastReply.length));

        TaskText.text = "Go to your Grandma's room and kill her";
        dadiCollider.enabled = true;

    }

    IEnumerator FadeInLight(Light light, float targetIntensity, float duration)
    {
        float startIntensity = light.intensity;
        float time = 0f;

        while (time < duration)
        {
            light.intensity = Mathf.Lerp(startIntensity, targetIntensity, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        light.intensity = targetIntensity; // ensure exact final value
    }

    public void DadiChangePose()
    {
        TableMeat.SetActive(false);
        DadiEatPose.SetActive(false);
        DadiLayPose.SetActive(true);
    }
    IEnumerator ShowCaption(string text, float duration)
    {
        caption.text = text;
        yield return new WaitForSeconds(duration);
        caption.text = "";
    }
    public void DestroyAllNavAgentsInScene()
    {
        NavMeshAgent[] agents = FindObjectsOfType<NavMeshAgent>();
        foreach (NavMeshAgent agent in agents)
        {
            Destroy(agent.gameObject);
        }
    }

}
