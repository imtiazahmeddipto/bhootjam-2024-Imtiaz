using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public GameObject DestroyFX;

    private void OnCollisionEnter(Collision collision)
    {
        Destroy(Instantiate(DestroyFX, transform.position, Quaternion.identity), .3f);
        Destroy(gameObject);
    }
}
