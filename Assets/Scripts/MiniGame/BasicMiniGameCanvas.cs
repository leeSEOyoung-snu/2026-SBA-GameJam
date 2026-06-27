using DG.Tweening;
using TMPro;
using UnityEngine;

public class BasicMiniGameCanvas : MonoBehaviour
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    [SerializeField] private TMP_Text gameStartTxt;
    [SerializeField] private CanvasGroup curtain;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float curtainFadeDuration = 0.1f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private void Awake()
    {
        SetHidden();
    }

    public Sequence OpenTutorial()
    {
        if (tutorialCanvas == null)
            return DOTween.Sequence();

        return tutorialCanvas.Open();
    }

    public Sequence CloseTutorial()
    {
        if (tutorialCanvas == null)
            return DOTween.Sequence();

        return tutorialCanvas.Close();
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

    public Tween HideCurtain()
    {
        if (curtain == null)
            return DOTween.Sequence();

        curtain.DOKill();
        return curtain.DOFade(0f, curtainFadeDuration);
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
