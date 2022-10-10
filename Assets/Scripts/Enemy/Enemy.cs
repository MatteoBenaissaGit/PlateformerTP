using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using static Enums;

namespace Enemy
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(Animator))]
    public class Enemy : Damageable
    {
        private Sound sound;


        [Header("Enemy")]
        [SerializeField, Range(0,20)] 
        private float _walkSpeed;
        [SerializeField, Range(0,10)] 
        private float _attackRange;
        [SerializeField, Range(0,10)] 
        private float _fleeRange;
        [SerializeField, Range(0,10)] 
        private float _attackSpeed;
        // [SerializeField, Tooltip("If the player is in this range, the enemy will consider it to be on the same level as him"), Range(0,20)] 
        // private float _playerHeightDetection;

        [Header("Detections")]
        [SerializeField] private Detection _fallDetection;
        [SerializeField] private Detection _wallDetection;
        [SerializeField] private LayerMask _wallLayer;

        [Header("Gizmos")] 
        [SerializeField] 
        private bool _showGizmos;
        [SerializeField] 
        private Color _gizmosColor;

        [Header("Debug")] 
        [SerializeField] 
        private EnemyState _enemyState = EnemyState.IdlePassive;

        //booleans
        private bool _fallOnLeft = false;
        private bool _fallOnRight = false;
        private bool _wallOnLeft = false;
        private bool _wallOnRight = false;
        
        //references
        private Rigidbody2D _rigidbody2D;
        private Transform _characterTransform;
        private SpriteRenderer _spriteRenderer;
        private Animator _animator;

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _animator = GetComponent<Animator>();
            
            //TODO change by referencing _playerTransform from the spawning
            _characterTransform = FindObjectOfType<CharacterController>().transform;
            sound = Sound.Instance;
        }

        private void ChangeEnemyState(EnemyState enemyState)
        {
            switch (enemyState)
            {
                case EnemyState.IdlePassive:
                    _animator.SetBool("IsWalking",false);
                    break;
                case EnemyState.RunToPlayer:
                    _animator.SetBool("IsWalking",true);
                    break;
                case EnemyState.IdleAttack:
                    _animator.SetBool("IsWalking",false);
                    StartCoroutine(AttackCoroutine());
                    break;
                case EnemyState.Flee:
                    _animator.SetBool("IsWalking",true);
                    _spriteRenderer.flipX = !_spriteRenderer.flipX;
                    break;
            }
            _enemyState = enemyState;
        }

        private void Update()
        {
            FallAndWallDetection();
            CheckPlayerDistance();
        }

        private void FixedUpdate()
        {
            Walk();
        }

        private IEnumerator AttackCoroutine()
        {
            yield return new WaitForSeconds(_attackSpeed);

            if (_characterTransform.position.y < transform.position.y + _attackRange &&
                _characterTransform.position.y > transform.position.y - _attackRange)
            {
                Attack();
            }

            if (_enemyState == EnemyState.IdleAttack)
            {
                StartCoroutine(AttackCoroutine());
            }
        }
        
        private void Attack()
        {
            _animator.SetTrigger("Attack");
            print("attack");
            _characterTransform.GetComponent<CharacterLifeManager>().TakeDamage(1);
        }

        private Direction DirectionToCharacter()
        {
            Direction direction = transform.position.x < _characterTransform.position.x ? Direction.Right : Direction.Left;
            _spriteRenderer.flipX = direction == Direction.Right;
            return direction;
        }

        private void CheckPlayerDistance()
        {
            float distanceToPlayer = Math.Abs(transform.position.x - _characterTransform.position.x);
            const float divider = 1.5f;
            
            if (distanceToPlayer > _attackRange / divider)
            {
                ChangeEnemyState(EnemyState.RunToPlayer);
            }
            else if (distanceToPlayer <= _attackRange / divider && distanceToPlayer >= _fleeRange && _enemyState != EnemyState.IdleAttack)
            {
                ChangeEnemyState(EnemyState.IdleAttack);
            }
            else if (distanceToPlayer < _fleeRange)
            {
                ChangeEnemyState(EnemyState.Flee);
            }
        }

        private void Walk()
        {
            int directionValue = DirectionToCharacter() == Direction.Left ? -1 : 1;
            const float decelerationSpeed = 0.5f;
            var velocity = _rigidbody2D.velocity;
            switch (_enemyState)
            {
                case EnemyState.IdlePassive:
                    _rigidbody2D.velocity = Vector2.MoveTowards(velocity, new Vector2(0,velocity.y), decelerationSpeed);
                    break;
                case EnemyState.RunToPlayer:
                    _rigidbody2D.velocity = new Vector2(directionValue * _walkSpeed, velocity.y);
                    break;
                case EnemyState.IdleAttack:
                    _rigidbody2D.velocity = Vector2.MoveTowards(velocity, new Vector2(0,velocity.y), decelerationSpeed);
                    break;
                case EnemyState.Flee:
                    _rigidbody2D.velocity = new Vector2(-directionValue * _walkSpeed, _rigidbody2D.velocity.y);
                    break;
            }
        }

        private void FallAndWallDetection()
        {
            if (_enemyState == EnemyState.IdleAttack) return;
            
            CheckFall();
            CheckWall();
        }
        private void CheckFall()
        {
            Vector2 rightPoint = (Vector2)transform.position + new Vector2(_fallDetection.DetectionX,_fallDetection.DetectionY);
            _fallOnRight = !Physics2D.OverlapCircle(rightPoint, _fallDetection.DetectionRadius);
            Vector2 leftPoint = (Vector2)transform.position + new Vector2(-_fallDetection.DetectionX,_fallDetection.DetectionY);
            _fallOnLeft = !Physics2D.OverlapCircle(leftPoint, _fallDetection.DetectionRadius);

            if (_fallOnLeft && DirectionToCharacter() == Direction.Left)
            {
                ChangeEnemyState(EnemyState.IdlePassive);
            }
            if (_fallOnRight && DirectionToCharacter() == Direction.Right)
            {
                ChangeEnemyState(EnemyState.IdlePassive);
            }
        }

        private void CheckWall()
        {

            Vector2 rightPoint = (Vector2)transform.position + new Vector2(_wallDetection.DetectionX,_wallDetection.DetectionY);
            Collider2D[] hitRight = Physics2D.OverlapCircleAll(rightPoint, _wallDetection.DetectionRadius);
            if (hitRight != null && hitRight.Length>0)
            {
                _wallOnRight = true;
            }
            else
            {
                _wallOnRight = false;
            }

            Vector2 leftPoint = (Vector2)transform.position + new Vector2(-_wallDetection.DetectionX,_wallDetection.DetectionY);
            Collider2D[] hitLeft = Physics2D.OverlapCircleAll(leftPoint, _wallDetection.DetectionRadius);
            if (hitLeft != null && hitLeft.Length>0)
            {
                _wallOnLeft = true;
            }
            else
            {
                _wallOnLeft = false;
            }
            
            if (_wallOnLeft && DirectionToCharacter() == Direction.Left)
            {
                ChangeEnemyState(EnemyState.IdlePassive);
            }
            if (_wallOnRight && DirectionToCharacter() == Direction.Right)
            {
                ChangeEnemyState(EnemyState.IdlePassive);
            }
        }

        protected override void Die()
        {
            sound.EnemyDead(true);
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            if (!_showGizmos) return;
            
            //ranges
            Gizmos.color = _gizmosColor;
            var position = transform.position;
            Gizmos.DrawWireSphere(position,_attackRange);
            Gizmos.DrawWireSphere(position,_fleeRange);
            
            // //height detection
            // #if UNITY_EDITOR
            // var textPosition = position + new Vector3(0, _playerHeightDetection + 2, 0);
            // Gizmos.DrawLine(new Vector3(position.x,position.y ,0), new Vector3(position.x,position.y + _playerHeightDetection,0));
            // Gizmos.DrawLine(new Vector3(position.x,position.y ,0), new Vector3(position.x,position.y - _playerHeightDetection,0));
            // Handles.Label(textPosition, "player height detection", new GUIStyle{ alignment = TextAnchor.MiddleCenter,normal = {textColor = _gizmosColor}}); 
            // #endif
            
            //fall detection
            Gizmos.color = _fallOnLeft ? Color.green : Color.red;
            Gizmos.DrawSphere(position+new Vector3(-_fallDetection.DetectionX,_fallDetection.DetectionY,0),_fallDetection.DetectionRadius);
            Gizmos.color = _fallOnRight ? Color.green : Color.red;
            Gizmos.DrawSphere(position+new Vector3(_fallDetection.DetectionX,_fallDetection.DetectionY,0),_fallDetection.DetectionRadius);
            
            //fall detection
            Gizmos.color = _wallOnLeft ? Color.green : Color.red;
            Gizmos.DrawWireSphere(position+new Vector3(-_wallDetection.DetectionX,_wallDetection.DetectionY,0),_wallDetection.DetectionRadius);
            Gizmos.color = _wallOnRight ? Color.green : Color.red;
            Gizmos.DrawWireSphere(position+new Vector3(_wallDetection.DetectionX,_wallDetection.DetectionY,0),_wallDetection.DetectionRadius);
        }
    }
}

[Serializable]
public struct Detection
{
    [Range(0,10)] 
    public float DetectionX;
    [Range(-10,10)]
    public float DetectionY;
    [Range(0,5)]
    public float DetectionRadius;
}
