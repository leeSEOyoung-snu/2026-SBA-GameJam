using DG.Tweening;
using UnityEngine;

// 플레이어별 손 이미지. 스윙 감지 시 쓰다듬는 모션 재생.
public class BokBokHandVisual : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("쓰다듬기 모션")]
    [SerializeField] private float strokeDistance = 0.4f;   // 아래 방향 이동 거리
    [SerializeField] private float strokeDownDuration = 0.12f;
    [SerializeField] private float strokeUpDuration   = 0.18f;
    [SerializeField] private Ease  strokeDownEase = Ease.OutQuad;
    [SerializeField] private Ease  strokeUpEase   = Ease.InOutSine;

    [Header("플레이어별 스프라이트")]
    [SerializeField] private Sprite[] playerSprites; // index 0~3 = player 1~4

    private Vector3 _originLocalPos;
    private Sequence _seq;

    private void Awake()
    {
        _originLocalPos = transform.localPosition;
    }

    public void SetPlayer(int playerIndex)
    {
        if (spriteRenderer == null) return;
        if (playerSprites != null && playerIndex < playerSprites.Length)
            spriteRenderer.sprite = playerSprites[playerIndex];
    }

    public void PlayStroke()
    {
        _seq?.Kill(complete: false);

        float downTarget = _originLocalPos.y - strokeDistance;
        float currentY   = transform.localPosition.y;

        // 현재 위치 기준 남은 거리 비율로 duration 조정 → 속도 일정하게 유지
        float downDist   = Mathf.Abs(currentY - downTarget);
        float totalDist  = strokeDistance;
        float downT      = strokeDownDuration * Mathf.Clamp01(downDist / totalDist);

        _seq = DOTween.Sequence();
        _seq.Append(transform.DOLocalMoveY(downTarget,          Mathf.Max(downT, 0.04f)).SetEase(strokeDownEase));
        _seq.Append(transform.DOLocalMoveY(_originLocalPos.y,   strokeUpDuration).SetEase(strokeUpEase));
    }
}
