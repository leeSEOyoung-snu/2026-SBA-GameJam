using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BasicMiniGameCanvas : MonoBehaviour
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    [SerializeField] private ResultCanvas resultCanvas;
    [SerializeField] private TMP_Text gameStartTxt;
    [SerializeField] private TMP_Text timeTxt;
    [SerializeField] private CanvasGroup curtain;
    [SerializeField] private float punchDuration = 0.25f;
    [SerializeField] private float fadeDuration = 0.45f;
    [SerializeField] private float curtainFadeDuration = 0.1f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private Coroutine _timeAttackCoroutine;

    private void Awake()
    {
        SetHidden();
    }

    public void SetTutorial(MiniGameResultContainer data, MiniGameProcessorBase processor)
    {
        tutorialCanvas.SetTutorial(data, processor);
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
        gameStartTxt.text = "게임 시작!";
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

    public Tween ShowCurtain()
    {
        if (curtain == null)
            return DOTween.Sequence();

        curtain.DOKill();
        return curtain.DOFade(1f, curtainFadeDuration).SetUpdate(true);
    }

    public void StartTimeAttack(int seconds)
    {
        StopTimeAttack();

        if (timeTxt == null)
            return;

        _timeAttackCoroutine = StartCoroutine(TimeAttackCoroutine(Mathf.Max(0, seconds)));
    }

    public void StopTimeAttack()
    {
        if (_timeAttackCoroutine == null)
            return;

        StopCoroutine(_timeAttackCoroutine);
        _timeAttackCoroutine = null;
    }

    private IEnumerator TimeAttackCoroutine(int seconds)
    {
        int remainingSeconds = seconds;
        SetTimeText(remainingSeconds);

        while (remainingSeconds > 0)
        {
            yield return new WaitForSeconds(1f);
            remainingSeconds--;
            SetTimeText(remainingSeconds);
        }

        _timeAttackCoroutine = null;
    }

    private void SetTimeText(int seconds)
    {
        timeTxt.text = seconds.ToString();
    }

    private void SetGameStartAlpha(float alpha)
    {
        Color color = gameStartTxt.color;
        color.a = alpha;
        gameStartTxt.color = color;
    }

    public Sequence PlayGameEnd()
    {
        if (resultCanvas == null)
            return DOTween.Sequence();

        return resultCanvas.PlayGameEnd();
    }

    public Sequence OpenResult(Dictionary<StateTypes, int> delta)
    {
        if (resultCanvas == null)
            return DOTween.Sequence();

        return resultCanvas.OpenResult(delta);
    }

    public Sequence CloseResult()
    {
        if (resultCanvas == null)
            return DOTween.Sequence();

        return resultCanvas.CloseResult();
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
