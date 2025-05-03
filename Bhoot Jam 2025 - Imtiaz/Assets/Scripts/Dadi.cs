using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dadi : MonoBehaviour
{
    public GameObject BloodFX;
    public AudioSource AS;
    public AudioClip dieSFX;

    public HorrorPlayerControllerURP player;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Bullet"))
        {
            Destroy(Instantiate(BloodFX, collision.transform.position, Quaternion.identity), 10f);
            AS.PlayOneShot(dieSFX);
            StartCoroutine(GameOver());
            player.dieReason.text = "Game Over";
        }
    }
    IEnumerator GameOver()
    {
        yield return new WaitForSecondsRealtime(1f);

        player.Die();
    }
}
