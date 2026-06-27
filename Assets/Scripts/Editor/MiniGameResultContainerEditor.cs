using UnityEditor;

[CustomEditor(typeof(MiniGameResultContainer))]
public class MiniGameResultContainerEditor : Editor
{
    private SerializedProperty type;
    private SerializedProperty gameTitle;
    private SerializedProperty desc;
    private SerializedProperty isTimeAttack;
    private SerializedProperty timeAttackSeconds;
    private SerializedProperty soloDelta;
    private SerializedProperty soloBattleTutorialText;
    private SerializedProperty oneVsThreeDelta;
    private SerializedProperty oneVsThreeTutorialText;
    private SerializedProperty twoVsTwoDelta;
    private SerializedProperty twoVsTwoTutorialText;
    private SerializedProperty cooperativeDelta;
    private SerializedProperty cooperativeTutorialText;

    private void OnEnable()
    {
        type = serializedObject.FindProperty("type");
        gameTitle = serializedObject.FindProperty("gameTitle");
        desc = serializedObject.FindProperty("desc");
        isTimeAttack = serializedObject.FindProperty("isTimeAttack");
        timeAttackSeconds = serializedObject.FindProperty("timeAttackSeconds");
        soloDelta = serializedObject.FindProperty("soloDelta");
        soloBattleTutorialText = serializedObject.FindProperty("soloBattleTutorialText");
        oneVsThreeDelta = serializedObject.FindProperty("oneVsThreeDelta");
        oneVsThreeTutorialText = serializedObject.FindProperty("oneVsThreeTutorialText");
        twoVsTwoDelta = serializedObject.FindProperty("twoVsTwoDelta");
        twoVsTwoTutorialText = serializedObject.FindProperty("twoVsTwoTutorialText");
        cooperativeDelta = serializedObject.FindProperty("cooperativeDelta");
        cooperativeTutorialText = serializedObject.FindProperty("cooperativeTutorialText");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(type);
        EditorGUILayout.PropertyField(gameTitle);
        EditorGUILayout.PropertyField(desc);
        EditorGUILayout.PropertyField(isTimeAttack);
        if (isTimeAttack.boolValue)
            EditorGUILayout.PropertyField(timeAttackSeconds);

        switch ((MiniGameTypes)type.enumValueIndex)
        {
            case MiniGameTypes.SoloBattle:
                EditorGUILayout.PropertyField(soloDelta);
                EditorGUILayout.PropertyField(soloBattleTutorialText);
                break;
            case MiniGameTypes.OneVsThree:
                EditorGUILayout.PropertyField(oneVsThreeDelta);
                EditorGUILayout.PropertyField(oneVsThreeTutorialText);
                break;
            case MiniGameTypes.TwoVsTwo:
                EditorGUILayout.PropertyField(twoVsTwoDelta);
                EditorGUILayout.PropertyField(twoVsTwoTutorialText);
                break;
            case MiniGameTypes.AffectionBattle:
                break;
            case MiniGameTypes.Cooperative:
                EditorGUILayout.PropertyField(cooperativeDelta);
                EditorGUILayout.PropertyField(cooperativeTutorialText);
                break;
        }

        serializedObject.ApplyModifiedProperties();
    }
}
