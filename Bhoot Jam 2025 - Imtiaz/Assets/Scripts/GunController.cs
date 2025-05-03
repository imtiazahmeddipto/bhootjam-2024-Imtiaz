using System.Collections;
using Unity.VisualScripting;
using UnityEngine;

public class Gun : MonoBehaviour
{
    [Header("References")]
    public Transform bulletSpawnPoint;
    public GameObject bulletPrefab;
    public Camera playerCamera;
    public HorrorCameraController cameraController;

    [Header("Settings")]
    public float bulletSpeed = 50f;
    public float fireRate = 0.1f;
    public int maxAmmo = 30;
    public float reloadTime = 1.5f;

    [Header("Current State")]
    public int currentAmmo;
    public bool isReloading = false;

    private float nextTimeToFire = 0f;
    public ParticleSystem muzzleFlash;
    public Animator anim;
    public AudioSource AS;
    public AudioClip ShootSFX, PumpSFX, pickUpSFX, GunPickUpSFX, doorOpenSFX;
    public float pickupRange = 3f;
    public LayerMask animalLayer;
    public TMPro.TMP_Text InteractionView;
    public TMPro.TMP_Text MeatContainerTXT;
    public int MeatContainer;

    private bool doHaveAccessories = false;
    public bool canInterectWithCupboard = false;
    public bool canPickUpAccessories = false;
    public bool canPickUpNote = false;
    public bool canPickUpIngredients = false;
    public bool canPerformLast = false;
    private TaskManager taskManager;
    public GameObject NoteUI;
    private int Child = 0, HumanHead = 0, PigHeart = 0, GraveyardSoil = 0, DadiHair = 0, Leg = 0;
    public TMPro.TMP_Text ingredientsContainerTXT;
    public GameObject[] ingredientsObj;

    void Start()
    {
        currentAmmo = maxAmmo;
        taskManager = GameObject.FindGameObjectWithTag("Player").GetComponent<TaskManager>();
        ingredientsContainerTXT.gameObject.SetActive(false);
        MeatContainerTXT.gameObject.SetActive(false);
        foreach (GameObject obj in ingredientsObj)
        {
            obj.SetActive(false);
        }

    }

    void Update()
    {
        if (isReloading)
            return;

        if (currentAmmo <= 0 || Input.GetKeyDown(KeyCode.R))
        {
            StartCoroutine(Reload());
            return;
        }

        if (Input.GetButton("Fire1") && Time.time >= nextTimeToFire && doHaveAccessories)
        {
            nextTimeToFire = Time.time + fireRate;
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            TryPickupAnimal();
        }

        if(MeatContainer >= 10)
        {
            taskManager.TaskText.text = "Go to Grandma's room and serve the meat on the table.";
        }
        HandleInteractionCheck();
        UpdateIngredientsText();
    }
    void HandleInteractionCheck()
    {
        MeatContainerTXT.text = "Meat Collected: " + MeatContainer.ToString();
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, animalLayer))
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Table")) && MeatContainer >= 10)
        {
            InteractionView.text = "Press E to Serve the Meats";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Accessories")) && canPickUpAccessories)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Cupboard")) && canInterectWithCupboard)
        {
            InteractionView.text = "Press E to Open";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Note")) && canPickUpNote)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Child")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("HumanHead")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Leg")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("DadiHair")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PicUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Soil")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PicUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("PigHeart")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to PickUp";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("NormalSoil")) && canPickUpIngredients)
        {
            InteractionView.text = "Press E to Remove";
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("FinalDrop")) && canPerformLast)
        {
            InteractionView.text = "Press E to Drop";
        }//If Functction then under
        else
        {
            InteractionView.text = "";
        }
    }

    void TryPickupAnimal()
    {
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, animalLayer))
        {
            GameObject Meat = hit.collider.gameObject;

            MeatContainer ++;
            Destroy(Meat);
            AS.PlayOneShot(pickUpSFX);
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Table")) && MeatContainer >= 10)
        {
            MeatContainer = 0;
            GameObject Table = hit.collider.gameObject;
            foreach (Transform child in Table.transform)
            {
                child.gameObject.SetActive(true);
            }
            AS.PlayOneShot(pickUpSFX);
            MeatContainerTXT.gameObject.SetActive(false);
            StartCoroutine(taskManager.Dialogue2());
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Accessories")) && canPickUpAccessories)
        {
            GameObject Accessories = hit.collider.gameObject;

            Destroy(Accessories);
            doHaveAccessories = true;
            AS.PlayOneShot(GunPickUpSFX);
            taskManager.TaskText.text = "Go outside, kill animals, and collect their meat.";
            MeatContainerTXT.gameObject.SetActive(true);
            taskManager.caption.text = "Press the left mouse button to shoot, and the right mouse button to use the special camera. The camera will hide spiritual animals and highlight important items.";
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Cupboard")) && canInterectWithCupboard)
        {
            GameObject Cupboard = hit.collider.gameObject;
            Cupboard.transform.parent.GetComponent<Animator>().SetBool("isShaking", false);
            Cupboard.transform.parent.GetComponent<Animator>().SetTrigger("Open");
            canInterectWithCupboard = false;
            StartCoroutine(taskManager.Dialogue3());
            AS.PlayOneShot(doorOpenSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Note")) && canPickUpNote)
        {
            GameObject Note = hit.collider.gameObject;
            Destroy(Note);
            NoteUI.SetActive(true);
            taskManager.TaskText.text = "";
            canPickUpNote = false;
            StartCoroutine(NoteControl()); 
            AS.PlayOneShot(GunPickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Child")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            Child = +1;
            AS.PlayOneShot(pickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("HumanHead")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            HumanHead = +1;
            AS.PlayOneShot(pickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Leg")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            Leg = +1;
            AS.PlayOneShot(pickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("Soil")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            GraveyardSoil ++;
            AS.PlayOneShot(pickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("PigHeart")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            PigHeart = +1;
            AS.PlayOneShot(pickUpSFX);
            taskManager.DadiChangePose();
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("DadiHair")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            DadiHair = +1;
            AS.PlayOneShot(pickUpSFX);

        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("NormalSoil")) && canPickUpIngredients)
        {
            GameObject obj = hit.collider.gameObject;
            Destroy(obj);
            AS.PlayOneShot(pickUpSFX);
        }
        else if (Physics.Raycast(ray, out hit, pickupRange, LayerMask.GetMask("FinalDrop")) && canPerformLast)
        {
            AS.PlayOneShot(pickUpSFX);
            StartCoroutine(taskManager.AfterDrop());
            GameObject obj = hit.collider.gameObject;
            foreach (Transform child in obj.transform)
            {
                child.gameObject.SetActive(true);
            }

        }
        //ADD INTERECTION SIMBLE UP
        else
        {
            Debug.Log("No animal in pickup range.");
        }
    }

    private bool hasDialogue5Started = false; // Add this at the top of your script

    void UpdateIngredientsText()
    {
        if (!hasDialogue5Started &&
            Child == 1 &&
            HumanHead == 1 &&
            PigHeart == 1 &&
            GraveyardSoil == 2 &&
            DadiHair == 1 &&
            Leg == 1)
        {
            StartCoroutine(taskManager.Dialogue5());
            ingredientsContainerTXT.gameObject.SetActive(false);
            hasDialogue5Started = true;
            return;
        }

        string text = "";

        text += "Child: " + Child.ToString() + "/1" + (Child == 1 ? " - Done" : "") + "\n";
        text += "Human Head: " + HumanHead.ToString() + "/1" + (HumanHead == 1 ? " - Done" : "") + "\n";
        text += "Pig Heart: " + PigHeart.ToString() + "/1" + (PigHeart == 1 ? " - Done" : "") + "\n";
        text += "Graveyard Soil: " + GraveyardSoil.ToString() + "/2" + (GraveyardSoil == 2 ? " - Done" : "") + "\n";
        text += "Dadi's Hair: " + DadiHair.ToString() + "/1" + (DadiHair == 1 ? " - Done" : "") + "\n";
        text += "Human Leg: " + Leg.ToString() + "/1" + (Leg == 1 ? " - Done" : "");

        ingredientsContainerTXT.text = text;
    }




    IEnumerator NoteControl()
    {
        yield return new WaitForSeconds(1f);

        taskManager.caption.text = "Press Space to take down the note";

        // Wait until player presses Space
        while (!Input.GetKeyDown(KeyCode.Space))
        {
            yield return null;
        }

        Destroy(NoteUI);
        taskManager.caption.text = "";
        StartCoroutine(taskManager.Dialogue4());
        ingredientsContainerTXT.gameObject.SetActive(true);
        foreach (GameObject obj in ingredientsObj)
        {
            obj.SetActive(true);
        }

    }

    void Shoot()
    {
        // Reduce ammo
        currentAmmo--;
        muzzleFlash.Play();
        anim.SetTrigger("Shoot");
        AS.PlayOneShot(ShootSFX);
        cameraController.ApplyCameraShake(.25f, 0.1f);

        // Raycast to find where we're shooting
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }
        else
        {
            // If we don't hit anything, shoot far into the distance
            targetPoint = ray.GetPoint(100);
        }

        // Calculate direction from bullet spawn to target
        Vector3 direction = (targetPoint - bulletSpawnPoint.position).normalized;

        // Instantiate bullet
        GameObject bullet = Instantiate(bulletPrefab, bulletSpawnPoint.position, Quaternion.LookRotation(direction));

        // Get bullet's rigidbody and apply force
        Rigidbody rb = bullet.GetComponent<Rigidbody>();
        rb.velocity = direction * bulletSpeed;

        // Optional: Destroy bullet after some time to prevent too many objects in scene
        Destroy(bullet, 5f);
    }

    System.Collections.IEnumerator Reload()
    {
        isReloading = true;
        Debug.Log("Reloading...");
        anim.SetBool("isReloading", true);

        yield return new WaitForSeconds(reloadTime);

        currentAmmo = maxAmmo;
        isReloading = false;
        anim.SetBool("isReloading", false);
        Debug.Log("Reload complete!");
    }

    public void PumpSound()
    {
        AS.PlayOneShot(PumpSFX);
    }
}