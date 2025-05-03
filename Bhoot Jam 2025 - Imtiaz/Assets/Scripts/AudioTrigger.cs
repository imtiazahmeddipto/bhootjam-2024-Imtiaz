using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioTrigger : MonoBehaviour
{
    private AudioSource AS;
    private bool canRestart;

    private void Start()
    {
        AS = GetComponent<AudioSource>();
        canRestart = false;
    }
    public void Update()
    {
        if (canRestart)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                SceneManager.LoadScene("CutScene2");
            }
        }
    }
    public void PlayAudio()
    {
        AS.Play();
    }
    public void StopAudio()
    {
        AS.Stop();
    }

    public void CanRestart()
    {
        canRestart = true;
    }
    public void UpdateTime()
    {
        Time.timeScale = 1;
    }
}
