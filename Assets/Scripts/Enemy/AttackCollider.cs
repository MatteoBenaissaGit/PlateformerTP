using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class AttackCollider : MonoBehaviour
{
    [SerializeField] private int _shootDamage;

    private void Start()
    {
        StartCoroutine(Destroy());
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        Damageable damageable = col.gameObject.GetComponent<Damageable>();
        if (damageable != null && col.gameObject.GetComponent<CharacterLifeManager>() == false)
        {
            damageable.TakeDamage(_shootDamage);
        }
    }

    private IEnumerator Destroy()
    {
        yield return new WaitForSeconds(0.1f);
        Destroy(gameObject);
    }
}
