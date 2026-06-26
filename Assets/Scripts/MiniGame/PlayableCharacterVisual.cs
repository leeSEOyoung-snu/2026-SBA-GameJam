using System;
using UnityEngine;

public class PlayableCharacterVisual : MonoBehaviour
{
    [Serializable]
    private class PlayerSpriteEntry
    {
        [Range(1, 4)] public int playerId = 1;
        public Sprite sprite;
    }

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private PlayerSpriteEntry[] playerSprites;

    public int PlayerId { get; private set; }

    private void Awake()
    {
        CacheComponents();
    }

    public void Init(int playerId)
    {
        PlayerId = playerId;
        ApplyPlayerSprite(playerId);
    }

    public void ApplyPlayerSprite(int playerId)
    {
        PlayerId = playerId;

        if (spriteRenderer == null)
            CacheComponents();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[{nameof(PlayableCharacterVisual)}] SpriteRenderer가 없습니다.", this);
            return;
        }

        if (!TryGetSprite(playerId, out var sprite))
        {
            Debug.LogWarning($"[{nameof(PlayableCharacterVisual)}] Player {playerId}에 할당된 스프라이트가 없습니다.", this);
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    private bool TryGetSprite(int playerId, out Sprite sprite)
    {
        if (playerSprites != null)
        {
            foreach (var entry in playerSprites)
            {
                if (entry == null || entry.playerId != playerId)
                    continue;

                sprite = entry.sprite;
                return sprite != null;
            }
        }

        sprite = null;
        return false;
    }

    private void CacheComponents()
    {
        if (spriteRenderer == null)
            spriteRenderer = GetComponentInChildren<SpriteRenderer>(true);
    }

    private void OnValidate()
    {
        CacheComponents();
    }
}
