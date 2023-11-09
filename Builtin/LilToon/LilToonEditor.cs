#if UNITY_EDITOR
using gomoru.su.LightController.API.Editor;
using UnityEditor;

[CustomEditor(typeof(LilToon))]
public sealed class LilToonEditor : ShaderSettingsEditor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LilToon.LightMaxLimitMax)));
        EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(LilToon.DistanceFadeEndMax)));

        DrawParameterGroup<LilToon.LightingGroup>(serializedObject.FindProperty(nameof(LilToon.Lighting)));
        DrawParameterGroup<LilToon.BacklightGroup>(serializedObject.FindProperty(nameof(LilToon.Backlight)));
        DrawParameterGroup<LilToon.DistanceFadeGroup>(serializedObject.FindProperty(nameof(LilToon.DistanceFade)));

        serializedObject.ApplyModifiedProperties();
    }
}

#endif