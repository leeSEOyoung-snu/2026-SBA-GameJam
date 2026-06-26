using UnityEditor;

[CustomEditor(typeof(MiniGameResultContainer))]
public class MiniGameResultContainerEditor : Editor
{
    private SerializedProperty type;
    private SerializedProperty soloDelta;
    private SerializedProperty oneVsThreeDelta;
    private SerializedProperty twoVsTwoDelta;
    private SerializedProperty cooperativeDelta;

    private void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        soloDelta = serializedObject.FindProperty("soloDelta");
        oneVsThreeDelta = serializedObject.FindProperty("oneVsThreeDelta");
        twoVsTwoDelta = serializedObject.FindProperty("twoVsTwoDelta");
        cooperativeDelta = serializedObject.FindProperty("cooperativeDelta");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type);

        switch ((MiniGameTypes)type.enumValueIndex)
        {
            case MiniGameTypes.SoloBattle:
                EditorGUILayout.PropertyField(soloDelta);
                break;
            case MiniGameTypes.OneVsThree:
                EditorGUILayout.PropertyField(oneVsThreeDelta);
                break;
            case MiniGameTypes.TwoVsTwo:
                EditorGUILayout.PropertyField(twoVsTwoDelta);
                break;
            case MiniGameTypes.AffectionBattle:
                break;
            case MiniGameTypes.Cooperative:
                EditorGUILayout.PropertyField(cooperativeDelta);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
