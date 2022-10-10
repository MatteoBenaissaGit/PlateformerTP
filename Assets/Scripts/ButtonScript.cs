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
    public void PlayButton()
    {
        sound.MenuClick(true);
        SceneManager.LoadScene("GameScene");
    }
}
