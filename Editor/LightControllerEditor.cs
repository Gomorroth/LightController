namespace gomoru.su.LightController;

[CustomEditor(typeof(LightController))]
[CanEditMultipleObjects]
public sealed class LightControllerEditor : Editor
{
    private SerializedProperty DefaultParameters;
    private SerializedProperty Excludes;

    private void OnEnable()
    {
        DefaultParameters = serializedObject.FindProperty(nameof(LightController.DefaultParameters));
        Excludes = serializedObject.FindProperty(nameof(LightController.Excludes));
    }

    public override void OnInspectorGUI ()
    {
        serializedObject.Update();

        foreach(SerializedProperty param in DefaultParameters.Copy())
        {
            EditorGUILayout.PropertyField(param);
        }

        EditorGUILayout.Separator();
        EditorGUILayout.PropertyField(Excludes);

        serializedObject.ApplyModifiedProperties();
    }
}
