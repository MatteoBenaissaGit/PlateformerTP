using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Enemy;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class CharacterAttackManager : MonoBehaviour
{
    private Sound sound;

    [Header("Shoot")] 
    [SerializeField, Range(0,10)] public int MaxAmmunitionCount;
    [SerializeField, Range(0,10)] private int _startAmmunitionCount;
    [SerializeField, Range(0,5)] private float _shootProjectileOffsetX;
    [SerializeField, Range(-2,2)] private float _shootProjectileOffsetY;
    [SerializeField, Range(0,5)] private float _attackOffsetX;
    [SerializeField, Range(-2,2)] private float _attackOffsetY;
    
    [Space(10), Header("Attack")] 
    [SerializeField, Range(0,5)] private float _shootCooldown;
    [SerializeField, Range(0,1)] private float _attackCooldown;

    [Space(10), Header("References")] 
    [SerializeField] private ShootProjectile _shootProjectilePrefab;
    [SerializeField] private Collider2D _attackCollider2D;
    [SerializeField] private Image _attackUIImage;
    
    [Space(10)] [Header("Gizmos")] 
    [SerializeField] private bool _enableGizmos = true;
    [SerializeField] private Color _gizmosColor = Color.white;
    
    [Space(10), Header("Debug")]
    
    //values
    [SerializeField] [ReadOnly] public int CurrentAmmunitionCount;
    private float _currentShootCooldown = 0;
    private float _currentAttackCooldown = 0;
    
    //booleans
    private bool _shoot = false;
    private bool _attack = false;
    
    //references
    private CharacterController _characterController;
    private Animator _animator;

    private void Start()
    {
        StartValuesSetup();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        sound = Sound.Instance;
    }

    private void Update()
    {
        GatherInput();
        Shoot();
        Attack();
    }

    private void StartValuesSetup()
    {
        CurrentAmmunitionCount = _startAmmunitionCount <= MaxAmmunitionCount ? _startAmmunitionCount : MaxAmmunitionCount;
        _currentShootCooldown = _shootCooldown;
        _currentAttackCooldown = _attackCooldown;
        UpdateAttackUI();
    }

    private void GatherInput()
    {
        _shoot = Input.GetButtonDown("shoot");
        _attack = Input.GetButtonDown("attack");
    }

    private void Shoot()
    {
        _currentShootCooldown -= Time.deltaTime;
        
        bool canShoot = _shoot && _currentShootCooldown <= 0 && CurrentAmmunitionCount > 0;
        if (!canShoot) return;

        _currentShootCooldown = _shootCooldown;
        CurrentAmmunitionCount--;
        sound.PlayerRanged(true);
        
        Direction facingDirection = _characterController.FacingDirection;
        
        _animator.SetTrigger("Attack");
            
        ShootProjectile shootProjectile = Instantiate(_shootProjectilePrefab, 
            transform.position + new Vector3(facingDirection == Direction.Left ? -_shootProjectileOffsetX : _shootProjectileOffsetX, _shootProjectileOffsetY,0), 
            Quaternion.identity);
        shootProjectile.ShootDirection = facingDirection;
        
        UpdateAttackUI();
    }

    private void Attack()
    {
        _currentAttackCooldown -= Time.deltaTime;

        bool canAttack = _attack && _currentAttackCooldown <= 0;
        if (!canAttack) return;

        _currentAttackCooldown = _attackCooldown;
        sound.PlayerMelee(true);
        
        Direction facingDirection = _characterController.FacingDirection;
        
        _animator.SetTrigger("Attack");

        Vector3 instancePosition = transform.position + new Vector3(facingDirection == Direction.Left ? -_attackOffsetX : _attackOffsetX, _attackOffsetY, 0);
        Collider2D collider2D = Instantiate(_attackCollider2D, instancePosition, Quaternion.identity);
        
        Vector3 scale = collider2D.transform.localScale;
        collider2D.transform.localScale = _characterController.FacingDirection switch
        {
            Direction.Left => new Vector3(-Math.Abs(scale.x),scale.y,scale.z),
            Direction.Right => new Vector3(Math.Abs(scale.x),scale.y,scale.z)
        };

        collider2D.transform.parent = transform;
    }
    
    public void UpdateAttackUI()
    {
        const float fillTime = .25f;
        _attackUIImage.DOFillAmount(CurrentAmmunitionCount/(float)MaxAmmunitionCount,fillTime);
    }

    private void OnDrawGizmos()
    {
        if (_enableGizmos == false) return;

        Gizmos.color = _gizmosColor;
        Gizmos.DrawCube(transform.position+new Vector3(-_shootProjectileOffsetX,_shootProjectileOffsetY,0), Vector3.one * 0.2f);
        Gizmos.DrawCube(transform.position+new Vector3(-_attackOffsetX,_attackOffsetY,0), Vector3.one * 0.3f);
    }
}
