using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 메뉴: Tools → RecyclingSort → Create All Prefabs
/// Assets/Prefabs/MiniGame/RecyclingSort/ 폴더에 프리팹을 자동 생성합니다.
/// </summary>
public static class RecyclingSortPrefabCreator
{
    private const string PrefabRoot = "Assets/Prefabs/Mini/RecyclingSort";

    [MenuItem("Tools/RecyclingSort/Create All Prefabs")]
    public static void CreateAll()
    {
        EnsureFolder(PrefabRoot);
        CreateTrashPrefabs();
        CreateSeesawPrefab();
        CreateBinPrefabs();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("[RecyclingSortPrefabCreator] 모든 프리팹 생성 완료!");
    }

    // ── 쓰레기 프리팹 4종 ──────────────────────────────────
    private static void CreateTrashPrefabs()
    {
        // 쓰레기 종류별 색상 (임시 색상, 나중에 스프라이트로 교체)
        var trashDefs = new (string name, RecyclingTrashType type, Color color)[]
        {
            ("Trash_Paper",   RecyclingTrashType.Paper,   new Color(0.9f, 0.85f, 0.6f)),
            ("Trash_Plastic", RecyclingTrashType.Plastic, new Color(0.4f, 0.8f, 1.0f)),
            ("Trash_Glass",   RecyclingTrashType.Glass,   new Color(0.5f, 1.0f, 0.5f)),
            ("Trash_Metal",   RecyclingTrashType.Metal,   new Color(0.8f, 0.8f, 0.8f)),
        };

        foreach (var def in trashDefs)
        {
            var go = new GameObject(def.name);

            // 시각
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color  = def.color;
            go.transform.localScale = new Vector3(0.5f, 0.5f, 1f);

            // 물리
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale        = 2f;
            rb.linearDamping       = 0.3f;
            rb.angularDamping      = 0.5f;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var col = go.AddComponent<CircleCollider2D>();
            col.radius = 0.25f;

            // 스크립트
            go.AddComponent<RecyclingTrash>();

            SavePrefab(go, def.name);
            Object.DestroyImmediate(go);
        }
    }

    // ── 시소 프리팹 ────────────────────────────────────────
    private static void CreateSeesawPrefab()
    {
        // 루트(피벗) — RecyclingSeesaw 스크립트 부착
        var pivot = new GameObject("Seesaw");
        pivot.AddComponent<RecyclingSeesaw>();

        // 자식(판) — 실제 물리 플랫폼
        var plank = new GameObject("Plank");
        plank.transform.SetParent(pivot.transform);
        plank.transform.localPosition = Vector3.zero;
        plank.transform.localScale    = new Vector3(3f, 0.2f, 1f);

        var sr = plank.AddComponent<SpriteRenderer>();
        sr.sprite = GetDefaultSprite();
        sr.color  = new Color(0.55f, 0.35f, 0.15f); // 갈색

        // Rigidbody2D — Kinematic (입력으로만 회전, 중력 없음)
        var rb = pivot.AddComponent<Rigidbody2D>();
        rb.bodyType    = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        // 충돌체는 자식(Plank)에
        var col = plank.AddComponent<BoxCollider2D>();
        col.size = Vector2.one; // localScale이 3×0.2 이므로 실제 크기 3×0.2

        SavePrefab(pivot, "Seesaw");
        Object.DestroyImmediate(pivot);
    }

    // ── 쓰레기통 프리팹 4종 ────────────────────────────────
    private static void CreateBinPrefabs()
    {
        var binDefs = new (string name, RecyclingTrashType type, Color color)[]
        {
            ("Bin_Paper",   RecyclingTrashType.Paper,   new Color(0.9f, 0.85f, 0.4f)),
            ("Bin_Plastic", RecyclingTrashType.Plastic, new Color(0.2f, 0.6f, 1.0f)),
            ("Bin_Glass",   RecyclingTrashType.Glass,   new Color(0.3f, 0.9f, 0.3f)),
            ("Bin_Metal",   RecyclingTrashType.Metal,   new Color(0.6f, 0.6f, 0.6f)),
        };

        foreach (var def in binDefs)
        {
            var go = new GameObject(def.name);

            // 시각 (통 몸통)
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = GetDefaultSprite();
            sr.color  = def.color;
            go.transform.localScale = new Vector3(1.5f, 1.8f, 1f);

            // RecyclingBin 스크립트
            var bin = go.AddComponent<RecyclingBin>();
            // AcceptedType은 SerializedObject로 설정
            var so = new SerializedObject(bin);
            so.FindProperty("acceptedType").enumValueIndex = (int)def.type;
            so.ApplyModifiedPropertiesWithoutUndo();

            // 입구 트리거 (통 위쪽)
            var entrance = new GameObject("Entrance");
            entrance.transform.SetParent(go.transform);
            entrance.transform.localPosition = new Vector3(0f, 0.6f, 0f);
            entrance.transform.localScale    = new Vector3(1f, 0.3f, 1f);

            var trigger = entrance.AddComponent<BoxCollider2D>();
            trigger.isTrigger = true;
            // localScale 보정: 부모가 1.5×1.8이므로 실제 크기를 1.5×0.3으로 맞춤
            trigger.size = new Vector2(1f / 1.5f, 0.3f / 1.8f);

            // 몸통 물리 콜라이더 (트리거 아님)
            var bodyCol = go.AddComponent<BoxCollider2D>();
            bodyCol.size   = Vector2.one;
            bodyCol.offset = Vector2.zero;

            SavePrefab(go, def.name);
            Object.DestroyImmediate(go);
        }
    }

    // ── 유틸 ──────────────────────────────────────────────
    private static Sprite GetDefaultSprite()
    {
        const string spritePath = "Assets/Prefabs/Mini/RecyclingSort/WhiteSquare.png";

        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
        if (existing != null) return existing;

        var tex = new Texture2D(32, 32);
        for (int y = 0; y < 32; y++)
            for (int x = 0; x < 32; x++)
                tex.SetPixel(x, y, Color.white);
        tex.Apply();
        File.WriteAllBytes(spritePath, tex.EncodeToPNG());
        AssetDatabase.ImportAsset(spritePath);

        var importer = (TextureImporter)AssetImporter.GetAtPath(spritePath);
        importer.textureType      = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.SaveAndReimport();

        return AssetDatabase.LoadAssetAtPath<Sprite>(spritePath);
    }

    private static void SavePrefab(GameObject go, string prefabName)
    {
        string path = $"{PrefabRoot}/{prefabName}.prefab";
        PrefabUtility.SaveAsPrefabAsset(go, path);
        Debug.Log($"  → 저장: {path}");
    }

    private static void EnsureFolder(string path)
    {
        // 경로를 단계별로 생성
        var parts  = path.Split('/');
        var current = parts[0];
        for (int i = 1; i < parts.Length; i++)
        {
            var next = current + "/" + parts[i];
            if (!AssetDatabase.IsValidFolder(next))
                AssetDatabase.CreateFolder(current, parts[i]);
            current = next;
        }
    }
}
