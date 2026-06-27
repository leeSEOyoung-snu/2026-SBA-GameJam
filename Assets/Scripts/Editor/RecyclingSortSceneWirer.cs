using UnityEngine;
using UnityEditor;

public static class RecyclingSortSceneWirer
{
    [MenuItem("Tools/RecyclingSort/Wire Scene References")]
    public static void WireReferences()
    {
        var gameManagerObj = GameObject.Find("RecyclingGameManager");
        if (gameManagerObj == null) { Debug.LogError("RecyclingGameManager not found!"); return; }

        var recyclingGame = gameManagerObj.GetComponent<RecyclingGame>();
        if (recyclingGame == null) { Debug.LogError("RecyclingGame component not found!"); return; }

        var seesawParent = GameObject.Find("Seesaws");
        var binParent    = GameObject.Find("Bins");
        var spawnParent  = GameObject.Find("SpawnPoints");

        var so = new SerializedObject(recyclingGame);

        // 시소 4개
        var seesawsProp = so.FindProperty("seesaws");
        seesawsProp.arraySize = seesawParent.transform.childCount;
        for (int i = 0; i < seesawParent.transform.childCount; i++)
            seesawsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                seesawParent.transform.GetChild(i).GetComponent<RecyclingSeesaw>();

        // 쓰레기통 4개
        var binsProp = so.FindProperty("bins");
        binsProp.arraySize = binParent.transform.childCount;
        for (int i = 0; i < binParent.transform.childCount; i++)
            binsProp.GetArrayElementAtIndex(i).objectReferenceValue =
                binParent.transform.GetChild(i).GetComponent<RecyclingBin>();

        // 스폰포인트 3개
        var spawnProp = so.FindProperty("spawnPoints");
        spawnProp.arraySize = spawnParent.transform.childCount;
        for (int i = 0; i < spawnParent.transform.childCount; i++)
            spawnProp.GetArrayElementAtIndex(i).objectReferenceValue =
                spawnParent.transform.GetChild(i);

        so.ApplyModifiedProperties();
        EditorUtility.SetDirty(gameManagerObj);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameManagerObj.scene);

        Debug.Log($"[RecyclingSortSceneWirer] 완료 - 시소:{seesawParent.transform.childCount} 통:{binParent.transform.childCount} 스폰:{spawnParent.transform.childCount}");
    }
}
