using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;

public class HechiSpriteContainerSetup
{
    // Chart name -> (Main sprite name in Hechi Main.aseprite, MiniGame sprite name in Hechi MiniGame.aseprite)
    // condition1 = column (세로줄), condition2 = row (가로줄)
    // Courage=1, Wisdom=2, Love=4, Recovery=3, Nightmare=0

    [MenuItem("Tools/Setup Hechi Sprite Container")]
    public static void Setup()
    {
        const string mainPath = "Assets/Sprites/HatchSprites/Hechi Main.aseprite";
        const string miniGamePath = "Assets/Sprites/HatchSprites/Hechi MiniGame.aseprite";

        var mainSprites = AssetDatabase.LoadAllAssetsAtPath(mainPath)
            .OfType<Sprite>()
            .ToDictionary(s => s.name, s => s);
        var miniGameSprites = AssetDatabase.LoadAllAssetsAtPath(miniGamePath)
            .OfType<Sprite>()
            .ToDictionary(s => s.name, s => s);

        Debug.Log($"[Setup] Main sprites: {string.Join(", ", mainSprites.Keys)}");
        Debug.Log($"[Setup] MiniGame sprites: {string.Join(", ", miniGameSprites.Keys)}");

        // (conditionCnt, condition1, condition2, hechiName, mainSpriteName, miniGameSpriteName)
        var entries = new (int cnt, int c1, int c2, string name, string main, string mini)[]
        {
            // conditionCnt=0 (KiCHi)
            (0, 0, 0, "키치", "KiCHi", "KiCHi"),

            // conditionCnt=1 (column header 5개)
            (1, 1, 0, "알치", "HachiEgg", "Egg"),
            (1, 2, 0, "요거치", "HacHiYogurt", "Yogurt"),
            (1, 4, 0, "꽃치", "HacHiFlower", "Flower"),
            (1, 3, 0, "아기치", "HatchChild", "Child"),
            (1, 0, 0, "똥치", "HacHiPoop", "Ddong"),

            // conditionCnt=2, condition2=Courage(1) row
            (2, 1, 1, "건담치", "HacHiGundam", "Gundam"),
            (2, 2, 1, "야쿠르치", "HacHiYogurtCar", "Cart"),
            (2, 4, 1, "샌즈치", "HatcHiSans", "Sans"),
            (2, 3, 1, "기가채치", "HacHiGigachad", "GiGaChad"),
            (2, 0, 1, "폭발치", "HacHiBoom", "Boom"),

            // conditionCnt=2, condition2=Wisdom(2) row
            (2, 1, 2, "유니콘치", "HatchUnicon", "Unicorn"),
            (2, 2, 2, "빙수치", "HacHiBingsu", "Bingsu"),
            (2, 4, 2, "요정치", "HacHiFairy", "Fairy"),
            (2, 3, 2, "페페치", "HacHiPepe", "PePe"),
            (2, 0, 2, "쿠마치", "HacHiMonokuma", "Monokuma"),

            // conditionCnt=2, condition2=Love(4) row
            (2, 1, 4, "용치", "HacHiDragon", "Dragon"),
            (2, 2, 4, "김치", "HacHiKimchi", "Kimchi"),
            (2, 4, 4, "칵테치", "HachiCocktail", "Cocktail"),
            (2, 3, 4, "미쿠치", "HacHiMiku", "Miku"),
            (2, 0, 4, "기저귀치", "HacHiDiaper", "Diaper"),

            // conditionCnt=2, condition2=Recovery(3) row
            (2, 1, 3, "디노치", "HachiDino", "Dino"),
            (2, 2, 3, "치킨치", "HacHiChicken", "Chicken"),
            (2, 4, 3, "배치", "HacHiPear", "Pear"),
            (2, 3, 3, "페이커치", "HacHiFaker", "Faker"),
            (2, 0, 3, "망가치", "HacHiBroken", "Broken"),

            // conditionCnt=2, condition2=Nightmare(0) row
            (2, 1, 0, "후라이치", "HachiEggFry", "Fry"),
            (2, 2, 0, "쓰레기치", "HacHiTrash", "Trash"),
            (2, 4, 0, "시든꽃치", "HacHiFlowerDie", "Flower Die"),
            (2, 3, 0, "멘헤라치", "HacHiMenhera", "Menhera"),
            (2, 0, 0, "스카치", "SkaCHi", "SkaCHi"),
        };

        var prefabPath = "Assets/Prefabs/GameManager.prefab";
        var prefabAsset = AssetDatabase.LoadMainAssetAtPath(prefabPath) as GameObject;
        if (prefabAsset == null)
        {
            Debug.LogError("[Setup] GameManager.prefab not found");
            return;
        }

        using (var scope = new PrefabUtility.EditPrefabContentsScope(prefabPath))
        {
            var root = scope.prefabContentsRoot;
            var container = root.GetComponentInChildren<HechiSpriteContainer>(true);
            if (container == null)
            {
                Debug.LogError("[Setup] HechiSpriteContainer not found in prefab");
                return;
            }

            var so = new SerializedObject(container);
            var listProp = so.FindProperty("hechiSpriteData");
            listProp.ClearArray();

            for (int i = 0; i < entries.Length; i++)
            {
                var e = entries[i];
                listProp.InsertArrayElementAtIndex(i);
                var elem = listProp.GetArrayElementAtIndex(i);

                elem.FindPropertyRelative("conditionCnt").intValue = e.cnt;
                elem.FindPropertyRelative("condition1").intValue = e.c1;
                elem.FindPropertyRelative("condition2").intValue = e.c2;
                elem.FindPropertyRelative("hechiName").stringValue = e.name;

                if (mainSprites.TryGetValue(e.main, out var mainSprite))
                    elem.FindPropertyRelative("mainSprite").objectReferenceValue = mainSprite;
                else
                    Debug.LogWarning($"[Setup] Main sprite not found: '{e.main}'");

                if (miniGameSprites.TryGetValue(e.mini, out var miniSprite))
                    elem.FindPropertyRelative("miniGameSprite").objectReferenceValue = miniSprite;
                else
                    Debug.LogWarning($"[Setup] MiniGame sprite not found: '{e.mini}'");
            }

            so.ApplyModifiedProperties();
            Debug.Log("[Setup] HechiSpriteContainer setup complete! 31 entries written.");
        }
    }
}
