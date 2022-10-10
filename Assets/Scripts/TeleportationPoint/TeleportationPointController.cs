using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;

public class TeleportationPointController : MonoBehaviour
{
    [Header("Parameters")] 
    [SerializeField] [Range(0,1)] private float _animationRotationSpeed = 0.1f;
    
    [Header("Gizmos")]
    [SerializeField] private bool _enableGizmos = true;
    [SerializeField] [Range(0,10)] private float _gizmosSize = 1f;
    [SerializeField] private Vector2 _gizmosOffset = Vector2.zero;

    [Header("References")] 
    [SerializeField] private MMF_Player _mmfPlayer = new();

    //bools
    [HideInInspector] public bool IsTheClosestPointToPlayer = false;
    private bool _hasPlayedStartAnimation = false;
    
    //other
    private Color _gizmosColor => IsTheClosestPointToPlayer ? Color.green : Color.red;


    private void Update()
    {
        switch (IsTheClosestPointToPlayer)
        {
            case true:
            {
                //rotate
                if (!_hasPlayedStartAnimation)
                    LaunchFeelEffect();
                break;
            }
            case false:
                StopFeelEffect();
                break;
        }
    }

    private void LaunchFeelEffect()
    {   
        _hasPlayedStartAnimation = true;
        _mmfPlayer.PlayFeedbacks();
    }
    private void StopFeelEffect()
    {
        _hasPlayedStartAnimation = false;
        transform.rotation = Quaternion.Euler(new Vector3(0,0,transform.rotation.eulerAngles.z%360));
        transform.DORotate(Vector3.zero, .5f);
    }
    
    private void OnDrawGizmos()
    {
        if (!_enableGizmos) return;

        //cube 
        var center = transform.position + (Vector3)_gizmosOffset;
        var size = new Vector3(_gizmosSize, _gizmosSize, _gizmosSize);
        Gizmos.color = _gizmosColor;
        Gizmos.DrawCube(center,size);
    }
}
