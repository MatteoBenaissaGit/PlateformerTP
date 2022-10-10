using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

[RequireComponent(typeof(Animator))]
public class CharacterController : MonoBehaviour
{
    private Sound sound;

    [Header("Character Parameters")] 
    [SerializeField] private Walk _walk;
    [SerializeField] private Jump _jump;
    [SerializeField] private CollisionBox _collisionBox;
    [SerializeField] private Gravity _gravity;

    [Space(10)] [Header("Collisions")] 
    [SerializeField] private LayerMask _groundLayerToDetect;
    [SerializeField] private int _collisionDetectorCount = 3;
    [SerializeField] private float _detectionRaycastLength = 0.1f;
    [SerializeField] [Range(0.1f, 0.3f)] private float _rayBuffer = 0.1f; // Prevents side detectors hitting the ground
    [Range(0, 20)] [SerializeField, Tooltip("Increases collision accuracy at the cost of performance.")] private int _freeColliderIterations = 10;

    [Space(10)] [Header("Gizmos")] 
    [SerializeField] private bool _enableGizmos = true;
    [SerializeField] private Color _gizmosCollisionBoxColor = Color.white;

    [Space(10)] [Header("References")] 
    [SerializeField] private CharacterTeleportationManager _characterTeleportationManager;
    [SerializeField] private ParticleSystem _fallParticleSystemPrefab;

    //floats
    private float _currentHorizontalSpeed, _currentVerticalSpeed;

    //booleans
    private bool _isGrounded => _isCollidedDown;
    private bool _canMove = true;

    //vectors
    private Vector3 _rawMovement;
    private Vector3 _velocity;
    private Vector3 _lastPosition;

    //others
    [HideInInspector] public Direction FacingDirection = Direction.Left;
    private GUIStyle _guiStyle = new();

    //reference
    private Animator _animator;
    
    public void Start()
    {
        sound = Sound.Instance;
    }
    private void Awake()
    {
        _guiStyle.alignment = TextAnchor.MiddleCenter;
        _animator = GetComponent<Animator>();
    }

    private void Update()
    {
        if (!_canMove) return;

        // Calculate velocity
        _velocity = (transform.position - _lastPosition) / Time.deltaTime;
        _lastPosition = transform.position;

        //inputs and collisions
        GatherInput();
        RunCollisionChecks();

        //x and y speed calculation
        CalculateWalk(); // Horizontal movement
        CalculateJumpApex(); // Affects fall speed, so calculate before gravity
        CalculateGravity(); // Vertical movement
        CalculateJump(); // Possibly overrides vertical

        //moving
        MoveCharacter(); // Actually perform the axis movement

        //teleport
        TeleportPlayer();
    }

    #region GatherInput

    private Inputs _inputs;

    private void GatherInput()
    {
        _inputs = new Inputs
        {
            Jump = Input.GetButtonDown("Jump"),
            X = Input.GetAxisRaw("Horizontal")
        };
        //animator
        if (_inputs.X != 0)
        {
            float baseScaleX = transform.localScale.x;
            transform.localScale = new Vector3(_currentHorizontalSpeed < 0 ? Math.Abs(baseScaleX) : -Math.Abs(baseScaleX), transform.localScale.y, 1);
            FacingDirection = _inputs.X < 0 ? Direction.Left : Direction.Right;
        }
    }

    #endregion

    #region Collisions

    private RayRange _rayRangeUp, _rayRangeRight, _rayRangeDown, _rayRangeLeft;
    private bool _isCollidedUp, _isCollidedRight, _isCollidedDown, _isCollidedLeft;

    //Checks for pre-collision information
    private void RunCollisionChecks()
    {
        //Generate ray ranges
        CalculateRayRanged();

        //assign collided booleans
        bool isCollidedDown = IsRayRangeDetectingColliding(_rayRangeDown);
        _isCollidedUp = IsRayRangeDetectingColliding(_rayRangeUp);
        _isCollidedLeft = IsRayRangeDetectingColliding(_rayRangeLeft);
        _isCollidedRight = IsRayRangeDetectingColliding(_rayRangeRight);

        bool IsRayRangeDetectingColliding(RayRange range)
        {
            return EvaluateRayRangePositions(range).Any(point =>
                Physics2D.Raycast(point, range.Direction, _detectionRaycastLength, _groundLayerToDetect));
        }

        //animation
        _animator.SetBool("IsGrounded", _isCollidedDown);
        if (isCollidedDown && _isCollidedDown == false)
        {
            SpawnGroundParticleEffect();
        }

        _isCollidedDown = isCollidedDown;
    }

    private void CalculateRayRanged()
    {
        var center = transform.position +
                     new Vector3(_collisionBox.CollisionBoxOffsetX, _collisionBox.CollisionBoxOffsetY, 0);
        var size = new Vector3(_collisionBox.CollisionBoxWidth, _collisionBox.CollisionBoxHeight, 1);
        var bounds = new Bounds(center, size);

        _rayRangeDown = new RayRange(bounds.min.x + _rayBuffer, bounds.min.y, bounds.max.x - _rayBuffer, bounds.min.y,
            Vector2.down);
        _rayRangeUp = new RayRange(bounds.min.x + _rayBuffer, bounds.max.y, bounds.max.x - _rayBuffer, bounds.max.y,
            Vector2.up);
        _rayRangeLeft = new RayRange(bounds.min.x, bounds.min.y + _rayBuffer, bounds.min.x, bounds.max.y - _rayBuffer,
            Vector2.left);
        _rayRangeRight = new RayRange(bounds.max.x, bounds.min.y + _rayBuffer, bounds.max.x, bounds.max.y - _rayBuffer,
            Vector2.right);
    }


    private IEnumerable<Vector2> EvaluateRayRangePositions(RayRange range)
    {
        for (var i = 0; i < _collisionDetectorCount; i++)
        {
            var slerpInterpolate = (float)i / (_collisionDetectorCount - 1);
            yield return Vector2.Lerp(range.Start, range.End, slerpInterpolate);
        }
    }

    #endregion

    #region Walk

    private void CalculateWalk()
    {
        if (_inputs.X != 0)
        {
            //set horizontal speed and clamp it
            _currentHorizontalSpeed += _inputs.X * _walk.Acceleration * Time.deltaTime;
            _currentHorizontalSpeed = Mathf.Clamp(_currentHorizontalSpeed, -_walk.MoveClamp, _walk.MoveClamp);

            // Apply bonus at the apex of a jump
            var apexBonus = Mathf.Sign(_inputs.X) * _walk.ApexBonus * _apexPoint;
            _currentHorizontalSpeed += apexBonus * Time.deltaTime;

            //animator
            _animator.SetBool("IsWalking", true);
        }
        // Slow the character down if no input
        else
        {
            _currentHorizontalSpeed =
                Mathf.MoveTowards(_currentHorizontalSpeed, 0, _walk.Deacceleration * Time.deltaTime);
            _animator.SetBool("IsWalking", false);
        }

        // Don't walk through walls
        if (_currentHorizontalSpeed > 0 && _isCollidedRight || _currentHorizontalSpeed < 0 && _isCollidedLeft)
        {
            _currentHorizontalSpeed = 0;
            _animator.SetBool("IsWalking", false);
        }
    }

    #endregion

    #region Gravity

    private float _fallSpeed;

    private void CalculateGravity()
    {
        if (_isCollidedDown)
        {
            // Move out of the ground
            if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
        }
        else
        {
            //Set fallSpeed and clamp it
            _currentVerticalSpeed -= _fallSpeed * Time.deltaTime;
            if (_currentVerticalSpeed < _gravity._fallClamp) _currentVerticalSpeed = _gravity._fallClamp;
        }
    }

    #endregion

    #region Jump

    private float _apexPoint; // Becomes 1 at the apex of a jump
    private float _lastJumpPressed;
    private static readonly int IsWalking = Animator.StringToHash("IsWalking");

    private void CalculateJumpApex()
    {
        if (!_isCollidedDown)
        {
            //Increase the closer to the top of the jump
            _apexPoint = Mathf.InverseLerp(_jump.JumpApexThreshold, 0, Mathf.Abs(_velocity.y));
            _fallSpeed = Mathf.Lerp(_gravity._minFallSpeed, _gravity._maxFallSpeed, _apexPoint);
        }
        else _apexPoint = 0;
    }

    private void CalculateJump()
    {
        //jump
        if (_inputs.Jump && _isGrounded)
        {
            _currentVerticalSpeed = _jump.JumpHeight;
            _animator.SetTrigger("Jump");
            sound.PlayerJump(true);
            SpawnGroundParticleEffect();
        }

        //up collision
        if (_isCollidedUp && _currentVerticalSpeed > 0)
        {
            _currentVerticalSpeed = 0;
            _animator.SetTrigger("Fall");
        }

        //fall
        if (_currentVerticalSpeed < 0)
        {
            _animator.SetTrigger("Fall");
        }
    }

    private void SpawnGroundParticleEffect()
    {
        Instantiate(_fallParticleSystemPrefab,transform.position+new Vector3(0,-_collisionBox.CollisionBoxHeight/2,0),Quaternion.identity);
    }

    #endregion

    #region Move

    // We cast our bounds before moving to avoid future collisions
    private void MoveCharacter()
    {
        //collider box variables
        var colliderBoxCenterPosition = transform.position +
                                        new Vector3(_collisionBox.CollisionBoxOffsetX,
                                            _collisionBox.CollisionBoxOffsetY, 0);
        var colliderBoxSize = new Vector3(_collisionBox.CollisionBoxWidth, _collisionBox.CollisionBoxHeight, 0);

        //movement variables
        _rawMovement = new Vector3(_currentHorizontalSpeed, _currentVerticalSpeed); //movement
        var move = _rawMovement * Time.deltaTime; //movement adjusted with deltaTime
        var furthestPoint = colliderBoxCenterPosition + move; //next position

        // check next position. If nothing hit, move and don't do extra checks
        var hit = Physics2D.OverlapBox(furthestPoint, colliderBoxSize, 0, _groundLayerToDetect);
        if (!hit)
        {
            transform.position += move;
            return;
        }

        // otherwise increment away from current pos; see what closest position we can move to
        var positionToMoveTo = transform.position;
        for (var i = 1; i < _freeColliderIterations; i++)
        {
            // increment to check all but furthestPoint - we did that already
            var lerpInterpolate = (float)i / _freeColliderIterations;
            var positionToTry = Vector2.Lerp(colliderBoxCenterPosition, furthestPoint, lerpInterpolate);

            if (Physics2D.OverlapBox(positionToTry, colliderBoxSize, 0, _groundLayerToDetect))
            {
                transform.position = positionToMoveTo;

                //If landed on a corner or hit our head on a ledge, move the player
                if (i != 1) return;
                if (_currentVerticalSpeed < 0) _currentVerticalSpeed = 0;
                var direction = transform.position - hit.transform.position;
                transform.position += direction.normalized * move.magnitude;

                return;
            }

            positionToMoveTo = positionToTry;
        }
    }

    #endregion

    #region Teleport

    private void TeleportPlayer()
    {
        var teleportationPoint = _characterTeleportationManager.ClosestTeleportationPoint;
        if (teleportationPoint != null && Input.GetButton("Teleport"))
        {
            sound.Teleport(true);
            Vector3 point = teleportationPoint.transform.position;
            transform.position = new Vector3(point.x,point.y,0);
        }
    }

    #endregion

    #region Gizmos

    private void OnDrawGizmos()
    {
        if (!_enableGizmos) return;

#if UNITY_EDITOR

        var position = transform.position;

        //collision
        ChangeGizmosColor(_gizmosCollisionBoxColor);
        var boxPosition =
            position + new Vector3(_collisionBox.CollisionBoxOffsetX, _collisionBox.CollisionBoxOffsetY, 0);
        var boxTextPosition = position + new Vector3(_collisionBox.CollisionBoxOffsetX,
            _collisionBox.CollisionBoxHeight / 2 + _collisionBox.CollisionBoxOffsetY + 0.5f, 0);
        Gizmos.DrawWireCube(boxPosition,
            new Vector2(_collisionBox.CollisionBoxWidth, _collisionBox.CollisionBoxHeight));
        UnityEditor.Handles.Label(boxTextPosition, "Collision Box", _guiStyle);

        //raycast colliders in each direction
        var gizmoSize = new Vector2(0.25f, 0.25f);
        Vector2 leftPosition = boxPosition + new Vector3(-1, 0),
            rightPosition = boxPosition + new Vector3(1, 0),
            upPosition = boxPosition + new Vector3(0, 1),
            downPosition = boxPosition + new Vector3(0, -1);
        //left
        ChangeGizmosColor(_isCollidedLeft ? Color.green : Color.red);
        Gizmos.DrawCube(leftPosition, gizmoSize);
        //right
        ChangeGizmosColor(_isCollidedRight ? Color.green : Color.red);
        Gizmos.DrawCube(rightPosition, gizmoSize);
        //up
        ChangeGizmosColor(_isCollidedUp ? Color.green : Color.red);
        Gizmos.DrawCube(upPosition, gizmoSize);
        //down
        ChangeGizmosColor(_isCollidedDown ? Color.green : Color.red);
        Gizmos.DrawCube(downPosition, gizmoSize);

#endif
    }

    private void ChangeGizmosColor(Color color)
    {
        Gizmos.color = color;
        _guiStyle.normal.textColor = color;
    }

    #endregion
}

public enum Direction
{
    Left = 0,
    Right = 1
}

[Serializable]
public struct CollisionBox
{
    [Range(0, 5)] public float CollisionBoxWidth;
    [Range(0, 5)] public float CollisionBoxHeight;
    [Range(-5, 5)] public float CollisionBoxOffsetX;
    [Range(-5, 5)] public float CollisionBoxOffsetY;
}

[Serializable]
public struct Walk
{
    [Range(0, 200)] public float Acceleration;
    [Range(0, 20)] public float MoveClamp;
    [Range(0, 200)] public float Deacceleration;
    [Range(0, 10)] public float ApexBonus;
}

[Serializable]
public struct Jump
{
    [Range(0, 50)] public float JumpHeight;
    [Range(0, 50)] public float JumpApexThreshold;
    [Range(0, 1)] public float JumpBuffer;
}

[Serializable]
public struct Inputs
{
    public float X;
    public bool Jump;
}

[Serializable]
public struct Gravity
{
    [Range(-50, 0)] public float _fallClamp;
    [Range(0, 100)] public float _minFallSpeed;
    [Range(0, 200)] public float _maxFallSpeed;
}

[Serializable]
public struct RayRange
{
    public RayRange(float x1, float y1, float x2, float y2, Vector2 direction)
    {
        Start = new Vector2(x1, y1);
        End = new Vector2(x2, y2);
        Direction = direction;
    }

    public readonly Vector2 Start, End, Direction;
}

//without 
//early jump
//coyote time

//Matteo