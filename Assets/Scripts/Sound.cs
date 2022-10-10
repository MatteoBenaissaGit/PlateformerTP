using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Sound : MonoBehaviour
{
    public static Sound Instance;

    [Header("------------------- Musics -----------------")]
    public AudioSource Music_Menu;
    public AudioSource Music_Fight;
    [Header("------------------- Sounds -----------------")]
    public AudioSource Jump;
    public AudioSource Get_Item;
    public AudioSource Wave_Completed;
    public AudioSource Enemy_Death;
    public AudioSource Player_Defeat;
    public AudioSource Player_Damage;
    public AudioSource Player_Ranged_Attack;
    public AudioSource Player_Melee_Attack;
    public AudioSource Menu_Click;
    public AudioSource Teleportation;


    // Start is called before the first frame update
    void Start()
    {
        string name = SceneManager.GetActiveScene().name;
        Instance = this;
        Music(name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Music(string scene)
    {
        if (scene == "MenuScene")
        {
            Music_Menu.Play();
            Music_Fight.Stop();
        }
        if (scene == "GameScene")
        {
            Music_Menu.Stop();
            Music_Fight.Play();
        }
        
    }
    public void PlayerJump(bool jump)
    {
        if(jump == true && !Jump.isPlaying)
            Jump.Play();
    }

    public void ItemPickup(bool item)
    {
        if (item == true && !Get_Item.isPlaying)
            Get_Item.Play();
    }

    public void WaveCompleted(bool completed)
    {
        if (completed == true && !Wave_Completed.isPlaying)
            Wave_Completed.Play();
    }

    public void EnemyDead(bool dead)
    {
        if (dead == true && !Enemy_Death.isPlaying)
            Enemy_Death.Play();
    }

    public void PlayerDefeat(bool defeated)
    {
        if (defeated == true && !Player_Defeat.isPlaying)
            Player_Defeat.Play();
    }
    public void PlayerDamage(bool dmg)
    {
        if (dmg == true && !Player_Damage.isPlaying)
            Player_Damage.Play();
    }

    public void PlayerRanged(bool shot)
    {
        if (shot == true && !Player_Ranged_Attack.isPlaying)
            Player_Ranged_Attack.Play();
    }

    public void PlayerMelee(bool melee)
    {
        if (melee == true && !Player_Melee_Attack.isPlaying)
            Player_Melee_Attack.Play();
    }

    public void MenuClick(bool clicked)
    {
        if (clicked == true && !Menu_Click.isPlaying)
            Menu_Click.Play();
    }

    public void Teleport(bool tp)
    {
        if (tp == true && !Teleportation.isPlaying)
            Teleportation.Play();
    }
}
