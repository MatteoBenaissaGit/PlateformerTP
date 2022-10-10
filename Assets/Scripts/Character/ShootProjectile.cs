using System;
using DG.Tweening;
using UnityEngine;

namespace Enemy
{
    public class ShootProjectile : MonoBehaviour
    {
        public Direction ShootDirection;
        [SerializeField] private float Speed;
        [SerializeField] private int ShootDamage;

        private Rigidbody2D _rigidbody2D;
        private bool _canMove = true;

        [Header("References")] 
        [SerializeField] private ParticleSystem _explosionParticleSystem;

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();

            StartScale();
        }

        private void StartScale()
        {
            Vector3 scale = transform.localScale;
            transform.localScale = ShootDirection switch
            {
                Direction.Left => new Vector3(-Math.Abs(scale.x), scale.y, 1),
                Direction.Right => new Vector3(Math.Abs(scale.x), scale.y, 1),
            };
            
            const float scaleMultiplier = 1.5f;
            const float time = 0.5f;
            transform.DOScale(transform.localScale * scaleMultiplier, time);
        }

        private void Update()
        {
            if (_canMove)
            {
                Move();
            }
        }

        private void OnDestroy()
        {
            ParticleSystem particle = Instantiate(_explosionParticleSystem,transform.position,Quaternion.identity);
            particle.Play();
        }

        private void Move()
        {
            _rigidbody2D.velocity = ShootDirection switch
            {
                Direction.Left => new Vector2(-Speed, 0),
                Direction.Right => new Vector2(Speed, 0),
            };
        }

        private void OnCollisionEnter2D(Collision2D col)
        {
            Damageable damageable = col.gameObject.GetComponent<Damageable>();

            if (damageable != null)
            {
                if (damageable.GetComponent<CharacterLifeManager>()) return;

                damageable.TakeDamage(ShootDamage);
            }

            Destroy();
        }

        private void Destroy()
        {
            Destroy(gameObject);
        }
    }
}
