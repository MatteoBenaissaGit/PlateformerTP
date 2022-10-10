using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(CharacterLifeManager))]
public class CharacterLifeManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        var myScript = (CharacterLifeManager)target;
        if(GUILayout.Button("Add One Life"))
        {
            myScript.TakeDamage(-1);
        }
        if(GUILayout.Button("Damage One Life"))
        {
            myScript.TakeDamage(1);
        }
    }
}
