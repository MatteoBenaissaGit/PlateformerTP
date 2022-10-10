using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CompanionController : MonoBehaviour
{
    [Header("Companion")] 
    [SerializeField, Range(0,1)] private float _lerpFollowForce = 0.01f;

    [Header("References")] 
    [SerializeField] private CharacterController _characterController;

    //values
    private float _positionOffsetY = 0;
    private float _positionOffsetX = 0;
    
    //other
    private Direction _lastCharacterDirection;

    private void Start()
    {
        _lastCharacterDirection = _characterController.FacingDirection;
        _positionOffsetY = Math.Abs(transform.position.y - _characterController.transform.position.y);
        _positionOffsetX = Math.Abs(transform.position.x - _characterController.transform.position.x);
    }

    private void Update()
    {
        CheckIfCharacterChangedDirection();
        Vector3 characterPosition = _characterController.transform.position;
        Vector3 goToPosition = new Vector3(characterPosition.x+_positionOffsetX, characterPosition.y+_positionOffsetY, transform.position.z);
        transform.position = Vector3.Lerp(transform.position, goToPosition, _lerpFollowForce);
    }

    private void CheckIfCharacterChangedDirection()
    {
        if (_lastCharacterDirection != _characterController.FacingDirection)
        {
            _lastCharacterDirection = _characterController.FacingDirection;
            MoveToDirection(_lastCharacterDirection);
        }
    }
    
    private void MoveToDirection(Direction direction)
    {
        Vector3 scale = transform.localScale;
        switch (direction)
        {
            case Direction.Left:
                transform.localScale = new Vector3(-Math.Abs(scale.x), scale.y, 1);
                _positionOffsetX = Math.Abs(_positionOffsetX);
                break;
            case Direction.Right:
                transform.localScale = new Vector3(Math.Abs(scale.x), scale.y, 1);
                _positionOffsetX = - Math.Abs(_positionOffsetX);
                break;
        }
    }
}
