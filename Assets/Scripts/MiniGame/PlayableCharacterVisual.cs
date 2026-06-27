using System;
using System.Collections;
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
    [SerializeField] private Color damagedColor = new Color32(0xE0, 0x87, 0x87, 0xFF);
    [SerializeField] private float damagedBlinkDuration = 0.08f;
    [SerializeField] private int damagedBlinkCount = 2;
    public int PlayerId { get; private set; }
    public EffectManager Effects { get; private set; }

    private Coroutine _damagedVfxRoutine;

    public void Init(int playerId, EffectManager effects)
    {
        Effects = effects;
        PlayerId = playerId;
        ApplyPlayerSprite(playerId);
    }

    public void ApplyPlayerSprite(int playerId)
    {
        PlayerId = playerId;

        if (!TryGetSprite(playerId, out var sprite))
        {
            Debug.LogWarning($"[{nameof(PlayableCharacterVisual)}] Player {playerId}에 할당된 스프라이트가 없습니다.", this);
            return;
        }

        spriteRenderer.sprite = sprite;
    }

    public void DamagedVFX()
    {
        if (_damagedVfxRoutine != null)
            StopCoroutine(_damagedVfxRoutine);

        _damagedVfxRoutine = StartCoroutine(DamagedVFXRoutine());
    }

    private bool TryGetSprite(int playerId, out Sprite sprite)
    {
        if (playerSprites != null)
        {
            foreach (var entry in playerSprites)
            {
                if (entry == null || entry.playerId != playerId)
                {
                    continue;
                }

                sprite = entry.sprite;
                return sprite != null;
            }
        }

        sprite = null;
        return false;
    }

    private IEnumerator DamagedVFXRoutine()
    {
        var originColor = spriteRenderer.color;

        for (var i = 0; i < damagedBlinkCount; i++)
        {
            spriteRenderer.color = damagedColor;
            yield return new WaitForSeconds(damagedBlinkDuration);
            spriteRenderer.color = originColor;
            yield return new WaitForSeconds(damagedBlinkDuration);
        }

        _damagedVfxRoutine = null;
    }
}
