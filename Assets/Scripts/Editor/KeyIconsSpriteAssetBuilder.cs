using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore;

public static class KeyIconsSpriteAssetBuilder
{
    private const string TexturePath = "Assets/Sprites/UI/Icon/KeyIcons.png";
    private const string SpriteAssetPath = "Assets/Sprites/UI/Icon/KeyIcons Sprite Asset.asset";

    [MenuItem("Tools/GameJam/Rebuild KeyIcons Sprite Asset")]
    public static TMP_SpriteAsset Rebuild()
    {
        Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(TexturePath);
        if (texture == null)
        {
            Debug.LogError($"KeyIcons texture not found: {TexturePath}");
            return null;
        }

        TMP_SpriteAsset spriteAsset = AssetDatabase.LoadAssetAtPath<TMP_SpriteAsset>(SpriteAssetPath);
        if (spriteAsset == null)
        {
            spriteAsset = ScriptableObject.CreateInstance<TMP_SpriteAsset>();
            AssetDatabase.CreateAsset(spriteAsset, SpriteAssetPath);
        }

        Material material = spriteAsset.material;
        if (material == null)
        {
            material = new Material(Shader.Find("TextMeshPro/Sprite"));
            material.name = "KeyIcons Sprite Material";
            AssetDatabase.AddObjectToAsset(material, spriteAsset);
        }

        material.mainTexture = texture;
        spriteAsset.name = "KeyIcons Sprite Asset";
        spriteAsset.hashCode = TMP_TextUtilities.GetSimpleHashCode(spriteAsset.name);
        spriteAsset.spriteSheet = texture;
        spriteAsset.material = material;

        spriteAsset.spriteGlyphTable.Clear();
        spriteAsset.spriteCharacterTable.Clear();

        Sprite[] sprites = AssetDatabase.LoadAllAssetsAtPath(TexturePath)
            .OfType<Sprite>()
            .OrderByDescending(sprite => sprite.rect.y)
            .ThenBy(sprite => sprite.rect.x)
            .ToArray();

        for (int i = 0; i < sprites.Length; i++)
            AddSprite(spriteAsset, sprites[i], i);

        spriteAsset.UpdateLookupTables();
        EditorUtility.SetDirty(spriteAsset);
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        AssetDatabase.ImportAsset(SpriteAssetPath);

        return spriteAsset;
    }

    private static void AddSprite(TMP_SpriteAsset spriteAsset, Sprite sprite, int index)
    {
        TMP_SpriteGlyph glyph = new()
        {
            index = (uint)index,
            metrics = new GlyphMetrics(sprite.rect.width, sprite.rect.height, -sprite.pivot.x, sprite.rect.height - sprite.pivot.y, sprite.rect.width),
            glyphRect = new GlyphRect(sprite.rect),
            scale = 1f,
            sprite = sprite
        };

        TMP_SpriteCharacter character = new(0xFFFE, glyph)
        {
            name = sprite.name,
            scale = 1f
        };

        spriteAsset.spriteGlyphTable.Add(glyph);
        spriteAsset.spriteCharacterTable.Add(character);
    }
}
