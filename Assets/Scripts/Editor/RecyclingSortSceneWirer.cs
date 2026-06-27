using UnityEngine;
using UnityEditor;
using System.IO;

public static class RecyclingSortSceneWirer
{
    private const string PrefabRoot = "Assets/Prefabs/Mini/RecyclingSort";

    // ── 원형 스프라이트 + 마찰 재질 설정 ─────────────────────
    [MenuItem("Tools/RecyclingSort/Fix Trash Circle And Friction")]
    public static void FixTrashCircleAndFriction()
    {
        // 1. 원형 텍스처 생성
        string circlePath = PrefabRoot + "/WhiteCircle.png";
        if (AssetDatabase.LoadAssetAtPath<Sprite>(circlePath) == null)
        {
            int size = 64;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
            float r = size / 2f;
            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    float dx = x - r + 0.5f, dy = y - r + 0.5f;
                    tex.SetPixel(x, y, (dx * dx + dy * dy <= r * r) ? Color.white : Color.clear);
                }
            tex.Apply();
            File.WriteAllBytes(circlePath, tex.EncodeToPNG());
            AssetDatabase.ImportAsset(circlePath);
            var imp = (TextureImporter)AssetImporter.GetAtPath(circlePath);
            imp.textureType = TextureImporterType.Sprite;
            imp.spriteImportMode = SpriteImportMode.Single;
            imp.spritePixelsPerUnit = 64;
            imp.alphaIsTransparency = true;
            imp.SaveAndReimport();
        }
        var circleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(circlePath);

        // 2. 마찰 재질 생성
        string matPath = PrefabRoot + "/TrashFriction.physicsmaterial2d";
        var mat = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(matPath);
        if (mat == null)
        {
            mat = new PhysicsMaterial2D("TrashFriction");
            mat.friction  = 0.8f;
            mat.bounciness = 0.05f;
            AssetDatabase.CreateAsset(mat, matPath);
        }

        // 3. 쓰레기 프리팹 4종에 원형 스프라이트 + 마찰 적용
        string[] trashNames = { "Trash_Paper", "Trash_Plastic", "Trash_Glass", "Trash_Metal" };
        foreach (var name in trashNames)
        {
            string path = PrefabRoot + "/" + name + ".prefab";
            var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (go == null) { Debug.LogWarning($"{name} prefab not found"); continue; }

            using (var scope = new PrefabUtility.EditPrefabContentsScope(path))
            {
                var root = scope.prefabContentsRoot;
                var sr   = root.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = circleSprite;

                var col = root.GetComponent<CircleCollider2D>();
                if (col != null) col.sharedMaterial = mat;

                var rb = root.GetComponent<Rigidbody2D>();
                if (rb != null) rb.sharedMaterial = mat;
            }
        }

        // 4. 시소 Plank 콜라이더에도 마찰 적용 (씬에서)
        var seesawParent = GameObject.Find("Seesaws");
        if (seesawParent != null)
        {
            for (int i = 0; i < seesawParent.transform.childCount; i++)
            {
                var plank = seesawParent.transform.GetChild(i).Find("Plank");
                if (plank == null) continue;
                var col = plank.GetComponent<BoxCollider2D>();
                if (col != null) col.sharedMaterial = mat;
            }
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(seesawParent.gameObject.scene);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RecyclingSortSceneWirer] 원형 스프라이트 + 마찰 적용 완료!");
    }

    [MenuItem("Tools/RecyclingSort/Fix Sprite PPU")]
    public static void FixSpritePPU()
    {
        string spritePath = "Assets/Prefabs/Mini/RecyclingSort/WhiteSquare.png";
        var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        if (importer == null) { Debug.LogError("WhiteSquare.png not found!"); return; }

        importer.spritePixelsPerUnit = 32; // 32x32 텍스처 → 1x1 유닛
        importer.SaveAndReimport();
        AssetDatabase.Refresh();
        Debug.Log("[RecyclingSortSceneWirer] WhiteSquare PPU → 32 완료. 이제 콜라이더와 스프라이트 크기가 일치합니다.");
    }

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
