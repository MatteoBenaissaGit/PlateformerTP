using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Damageable : MonoBehaviour
{
    private Sound sound;

    [SerializeField, Range(0,10)] public int Life;
    [SerializeField, Range(0,10)] public int CurrentLife;

    public virtual void Start()
    {
        CurrentLife = Life;
        sound = Sound.Instance;
    }

    public virtual void TakeDamage(int damage)
    {
        CurrentLife -= damage;
        if (sound!=null) sound.PlayerDamage(true);
        if (CurrentLife <= 0)
        {
            if (sound!=null) sound.PlayerDefeat(true);
            Die();
        }
    }

    protected abstract void Die();
}
