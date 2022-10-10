
using Interfaces;
using UnityEngine;

public class Ammunition : MonoBehaviour, IPickable
{
    private Sound sound;
    public void Start()
    {
        sound = Sound.Instance;
    }
    public void OnTriggerEnter2D(Collider2D col)
    {
        CharacterAttackManager characterAttackManager = col.gameObject.GetComponent<CharacterAttackManager>();
        if (characterAttackManager != null)
        {
            if (characterAttackManager.CurrentAmmunitionCount + 1 <= characterAttackManager.MaxAmmunitionCount)
            {
                characterAttackManager.CurrentAmmunitionCount++;
                characterAttackManager.UpdateAttackUI();
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
