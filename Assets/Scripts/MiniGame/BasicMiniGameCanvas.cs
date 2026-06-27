using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class BasicMiniGameCanvas : MonoBehaviour
{
    [SerializeField] private TutorialCanvas tutorialCanvas;
    [SerializeField] private CanvasGroup gameStartGroup;
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
        if (gameStartGroup == null)
            return sequence;

        Transform gameStartTransform = gameStartGroup.transform;
        gameStartTransform.DOKill();
        gameStartGroup.DOKill();
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

    private void SetHidden()
    {
        if (gameStartGroup == null)
            return;

        gameStartGroup.alpha = 0f;
        gameStartGroup.transform.localScale = Vector3.one;
        gameStartGroup.gameObject.SetActive(false);
    }
}
