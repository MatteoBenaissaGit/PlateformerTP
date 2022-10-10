using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(TeleportationPointsCreator))]
public class TeleportationPointsCreatorEditor : Editor
{
    public override void OnInspectorGUI()
    {
#if UNITY_EDITOR

        DrawDefaultInspector();
        var myScript = (TeleportationPointsCreator)target;
        if(GUILayout.Button("Create A Teleportation Point"))
        {
            myScript.CreateTeleportationPoint();
        }
        if(GUILayout.Button("Delete All Teleportation Point"))
        {
            myScript.DeleteAllPoints();
        }
    }
#endif    
}
