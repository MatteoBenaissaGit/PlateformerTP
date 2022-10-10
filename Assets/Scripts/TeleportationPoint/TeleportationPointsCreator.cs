using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class TeleportationPointsCreator : MonoBehaviour
{
    [Header("References")] 
    public TeleportationPointController TeleportationPointPrefab;

    [Space(10)] [Header("Gizmos")] 
    [SerializeField] private bool _enableGizmos = true;
    [SerializeField] private Color _gizmosColor = Color.white;
    
    private List<TeleportationPointController> _teleportationPointControllersList = new();

    #if UNITY_EDITOR
    public void CreateTeleportationPoint()
    {
        var sceneView = SceneView.lastActiveSceneView.camera.transform.position;
        var position = new Vector3(sceneView.x, sceneView.y, 10);
        _teleportationPointControllersList.Add(Instantiate(TeleportationPointPrefab, position, Quaternion.identity, gameObject.transform));
    }
    #endif

    public void DeleteAllPoints()
    {
        foreach (var point in _teleportationPointControllersList.ToList().Where(point => point is null))
        {
            _teleportationPointControllersList.Remove(point);
        }
        _teleportationPointControllersList.ForEach(point => DestroyImmediate(point.gameObject));
        //destroy in scene
        _teleportationPointControllersList.Clear();
    }
    
#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (!_enableGizmos || Selection.activeGameObject != gameObject) return;


        var position = SceneView.lastActiveSceneView.camera.transform.position;
        var textPosition = position + new Vector3(0, 2f, 0);
        Gizmos.color = _gizmosColor;
        Gizmos.DrawLine(new Vector3(position.x -1,position.y,0), new Vector3(position.x + 1,position.y,0));
        Gizmos.DrawLine(new Vector3(position.x,position.y-1,0), new Vector3(position.x,position.y + 1,0));
        Handles.Label( textPosition, "Place Teleportation Point", new GUIStyle{ alignment = TextAnchor.MiddleCenter,normal = {textColor = _gizmosColor}});
    }
#endif
}
