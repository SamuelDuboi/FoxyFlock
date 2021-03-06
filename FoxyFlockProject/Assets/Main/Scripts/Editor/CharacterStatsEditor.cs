using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.XR.Interaction.Toolkit;

[CustomEditor(typeof(CharacterStats))]
public class CharacterStatsEditor : Editor
{
    private CharacterStats statsTarget;
    private void OnEnable()
    {
        statsTarget = target as CharacterStats;
    }

    public override void OnInspectorGUI()
    {


        EditorGUILayout.HelpBox("Put me in the VRRig prefab on SCR_Locomation script, at player Stats. Right under movement",MessageType.Info);


        statsTarget.minTriggerTreshold = EditorGUILayout.Slider(" Minimum press forced to activate Move", statsTarget.minTriggerTreshold, 0,1);
        EditorGUILayout.HelpBox("Rotation",MessageType.None);
        statsTarget.xPower = EditorGUILayout.Slider(" Power", statsTarget.xPower,-10,10);
        EditorGUILayout.BeginHorizontal();
        statsTarget.RotateXOffset = EditorGUILayout.FloatField("Left-Right Offsset", statsTarget.RotateXOffset);
        statsTarget.RotateYOffset = EditorGUILayout.FloatField("Up-Down Offsset", statsTarget.RotateYOffset);
        EditorGUILayout.EndHorizontal();


        EditorGUILayout.HelpBox("Up Down", MessageType.None);
        statsTarget.yPower = EditorGUILayout.Slider("Power",statsTarget.yPower,-10,10);
        EditorGUILayout.BeginHorizontal();
        statsTarget.upOffset = EditorGUILayout.FloatField("Up-Down Offsett", statsTarget.upOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.HelpBox("Forward", MessageType.None);
        statsTarget.zPower = EditorGUILayout.Slider(" Power",statsTarget.zPower,-10,10);
        EditorGUILayout.BeginHorizontal();
        statsTarget.forwardZOffset = EditorGUILayout.FloatField("Forwrad-Backward Offsset", statsTarget.forwardZOffset);
        statsTarget.forwardYOffset = EditorGUILayout.FloatField("Up-Down Offsset", statsTarget.forwardYOffset);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        EditorGUILayout.Space();
        statsTarget.snapTurnAngle = EditorGUILayout.Slider(" Angle of snap turn", statsTarget.snapTurnAngle, 0, 180);
        EditorGUILayout.Space();
        
        EditorGUILayout.HelpBox("Buttons", MessageType.None);
        statsTarget.gripBtton = (InputHelpers.Button) EditorGUILayout.EnumPopup("Grip Button", statsTarget.gripBtton);
        statsTarget.fingerButton = (InputHelpers.Button)EditorGUILayout.EnumPopup("Activate Finger button", statsTarget.fingerButton);
        statsTarget.spawnButton = (InputHelpers.Button)EditorGUILayout.EnumPopup(" Spawn button", statsTarget.spawnButton);
        statsTarget.moveTrigger = (InputHelpers.Button)EditorGUILayout.EnumPopup(" Move trigger", statsTarget.moveTrigger);
        EditorUtility.SetDirty(statsTarget);
        Repaint();
        serializedObject.ApplyModifiedProperties();
    }
}
