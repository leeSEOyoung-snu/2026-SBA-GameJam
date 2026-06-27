using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BasicMiniGameCanvas : MonoBehaviour
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    [SerializeField] private CanvasGroup gameStartGroup;
    [SerializeField] private ResultCanvas resultCanvas;
    [SerializeField] private TMP_Text gameStartTxt;
    [SerializeField] private TMP_Text timeTxt;
    [SerializeField] private TMP_Text countNumTxt;
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
        if (gameStartGroup == null)
            return sequence;

        Transform gameStartTransform = gameStartGroup.transform;
        gameStartTransform.DOKill();
        gameStartGroup.DOKill();
        gameStartTxt.text = "게임 시작!";
        gameStartGroup.gameObject.SetActive(true);
        gameStartTransform.localScale = Vector3.one * 2f;
        gameStartGroup.alpha = 1f;

        sequence.Append(gameStartTransform.DOScale(Vector3.one, punchDuration).SetEase(ease));
        sequence.Append(gameStartGroup.DOFade(0f, fadeDuration).SetEase(Ease.InQuad).SetUpdate(true));
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

    public void SetCount(int currentCount, int totalCount)
    {
        if (countNumTxt == null)
            return;

        countNumTxt.text = $"{Mathf.Max(0, currentCount)}/{Mathf.Max(0, totalCount)}";
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
        if (gameStartGroup == null)
            return;

        gameStartGroup.alpha = 0f;
        gameStartGroup.transform.localScale = Vector3.one;
        gameStartGroup.gameObject.SetActive(false);
    }
}
