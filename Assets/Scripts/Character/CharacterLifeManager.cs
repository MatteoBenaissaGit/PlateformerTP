using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
public class CharacterLifeManager : Damageable
{
    [Header("References")]
    [SerializeField] private Image _lifeUIImage;

    [Header("Character")] 
    [SerializeField] private float _invicibilityTime;  
    
    private float _invicibilityInGameTimer;

    public override void Start()
    {
        base.Start();
        UpdateLifeUI();
        _lifeUIImage.DOComplete();
    }

    private void Update()
    {
        _invicibilityInGameTimer -= Time.deltaTime;
    }

    public override void TakeDamage(int damage)
    {
        if (CurrentLife - damage > Life || _invicibilityInGameTimer > 0) return;
        
        base.TakeDamage(damage);
        _invicibilityInGameTimer = _invicibilityTime;
        UpdateLifeUI();
    }
    
    protected override void Die()
    {
        print("Die");    
    }
    
    public void UpdateLifeUI()
    {
        const float fillTime = .25f;
        _lifeUIImage.DOFillAmount(CurrentLife/(float)Life,fillTime);
    }
}
