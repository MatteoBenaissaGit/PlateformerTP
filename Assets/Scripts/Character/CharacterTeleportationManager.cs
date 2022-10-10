using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class CharacterTeleportationManager : MonoBehaviour
{
    [Header("Parameters")] 
    [SerializeField] private bool _enableGizmos = true;
    [SerializeField] private Color _gizmosColor = Color.white;
    
    [Space(10), Header("Companion")] 
    [SerializeField, Range(0,40)] private float _teleportationRange;

    [Space(10), Header("Reference")] 
    [SerializeField] private LineRenderer _line;

    private List<TeleportationPointController> _teleportationPointsInRangeList = new();
    [HideInInspector] public TeleportationPointController ClosestTeleportationPoint;

    private void Start()
    {
        ChangeCircleColliderRadius(_teleportationRange);
    }

    private void Update()
    {
        UpdateClosestTeleportationPoint();
    }

    private void ChangeCircleColliderRadius(float radius)
    {
        var circleCollider2d = gameObject.GetComponent<CircleCollider2D>();
        const float convertToRadius = 5.7f;
        circleCollider2d.radius = radius/convertToRadius;
    }

    private void UpdateClosestTeleportationPoint()
    {
        if (_teleportationPointsInRangeList.Count <= 0)
        {
            ClosestTeleportationPoint = null;
            _line.SetPosition(1, Vector3.zero);
            return;
        }

        var closest = GetClosestTeleportationPoint();
        _teleportationPointsInRangeList.ForEach(point => point.IsTheClosestPointToPlayer = false);
        closest.IsTheClosestPointToPlayer = true;
        
        //trail
        Vector3 linePosition = closest.transform.localPosition - transform.localPosition;
        _line.SetPosition(1, closest.transform.localPosition);
    }
    
    private TeleportationPointController GetClosestTeleportationPoint()
    {
        //check in list the closest teleportation point
        ClosestTeleportationPoint = null;
        foreach (var point in _teleportationPointsInRangeList)
        {
            if (ClosestTeleportationPoint is null) {
                ClosestTeleportationPoint = point;
                continue;
            }
            var position = transform.position;
            var closestPointDistance = Vector2.Distance(ClosestTeleportationPoint.transform.position, position);
            var pointDistance = Vector2.Distance(point.transform.position, position);
            ClosestTeleportationPoint = Math.Min(closestPointDistance, pointDistance) == closestPointDistance ? ClosestTeleportationPoint : point;
        }
        return ClosestTeleportationPoint;
    }
    
    private void OnDrawGizmos()
    {
        if (!_enableGizmos) return;
        
        var position = transform.position;
        //parrot range
        Gizmos.color = _gizmosColor;
        Gizmos.DrawWireSphere(position,_teleportationRange);
        #if UNITY_EDITOR
        var textPosition = position + new Vector3(0, _teleportationRange + 0.5f, 0);
        var style = new GUIStyle {
            alignment = TextAnchor.MiddleCenter,
            normal = { textColor = _gizmosColor }
        };
        UnityEditor.Handles.color = _gizmosColor;
        UnityEditor.Handles.Label( textPosition, "Teleportation Radius", style);
        #endif
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        UpdateTeleportationPointInList(TriggerAction.Enter, other);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        UpdateTeleportationPointInList(TriggerAction.Exit, other);
    }

    private void UpdateTeleportationPointInList(TriggerAction triggerAction, Collider2D collider)
    {
        var teleportationPointController = collider.gameObject.GetComponent<TeleportationPointController>();
        if (teleportationPointController == null) return;
        switch (triggerAction)
        {
            case TriggerAction.Enter:
                _teleportationPointsInRangeList.Add(teleportationPointController);
                break;
            case TriggerAction.Exit:
                _teleportationPointsInRangeList.Remove(teleportationPointController);
                teleportationPointController.IsTheClosestPointToPlayer = false;
                break;
        }
    }
}

public enum TriggerAction
{
    Enter,
    Exit
}
