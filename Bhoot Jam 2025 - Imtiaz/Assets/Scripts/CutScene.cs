using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutScene : MonoBehaviour
{
    public bool canStartGame = false;

    private void Start()
    {
        canStartGame = false;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && canStartGame)
        {
            SceneManager.LoadScene("CutScene2");
        }
    }

    public void ChangeBool()
    {
        canStartGame = true;
    }

    public void StartGame()
    {
        SceneManager.LoadScene("Game");
    }
}
