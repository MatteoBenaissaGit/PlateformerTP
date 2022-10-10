using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ButtonScript : MonoBehaviour
{
    private Sound sound;

    public void Start()
    {
        sound = Sound.Instance;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
            PlayButton();
    }

    public void PlayButton()
    {
        //sound.MenuClick(true);
        SceneManager.LoadScene("GameScene");
    }
}
