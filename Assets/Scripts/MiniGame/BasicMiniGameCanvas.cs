using DG.Tweening;
using TMPro;
using UnityEngine;

public class BasicMiniGameCanvas : MonoBehaviour
{
    [SerializeField] private TMP_Text gameStartTxt;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private void Awake()
    {
        SetHidden();
    }

    public Sequence PlayGameStart()
    {
        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (gameStartTxt == null)
            return sequence;

        gameStartTxt.transform.DOKill();
        DOTween.Kill(gameStartTxt);
        gameStartTxt.gameObject.SetActive(true);
        gameStartTxt.transform.localScale = Vector3.one * 2f;
        Color color = gameStartTxt.color;
        color.a = 1f;
        gameStartTxt.color = color;

        sequence.Append(gameStartTxt.transform.DOScale(Vector3.one, punchDuration).SetEase(ease));
        sequence.Append(DOTween.To(() => gameStartTxt.color.a, SetGameStartAlpha, 0f, fadeDuration).SetTarget(gameStartTxt).SetEase(Ease.InQuad).SetUpdate(true));
        sequence.OnComplete(SetHidden);
        return sequence;
    }

    private void SetGameStartAlpha(float alpha)
    {
        Color color = gameStartTxt.color;
        color.a = alpha;
        gameStartTxt.color = color;
    }

    private void SetHidden()
    {
        if (gameStartTxt == null)
            return;

        Color color = gameStartTxt.color;
        color.a = 0f;
        gameStartTxt.color = color;
        gameStartTxt.transform.localScale = Vector3.one;
        gameStartTxt.gameObject.SetActive(false);
    }
}
