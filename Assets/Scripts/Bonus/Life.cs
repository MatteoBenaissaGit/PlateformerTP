
using Interfaces;
using UnityEngine;

public class Life : MonoBehaviour, IPickable
{
    private Sound sound;

    public void Start()
    {
        sound = Sound.Instance;
    }
    public void OnTriggerEnter2D(Collider2D col)
    {
        CharacterLifeManager characterLifeManager = col.gameObject.GetComponent<CharacterLifeManager>();
        if (characterLifeManager != null)
        {
            if (characterLifeManager.CurrentLife + 1 <= characterLifeManager.Life)
            {
                characterLifeManager.CurrentLife++;
                characterLifeManager.UpdateLifeUI();
                PickedUp();
            }
        }
    }

    public void PickedUp()
    {
        sound.ItemPickup(true);
        Destroy(gameObject);
    }
}
