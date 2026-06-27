using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ResultCanvas : MonoBehaviour
{
    [SerializeField] private CanvasGroup gameEndGroup;
    [SerializeField] private TMP_Text gameEndTxt;
    [SerializeField] private RectTransform resultBg;
    [SerializeField] private CanvasGroup resultInnerGroup;
    [SerializeField] private float gameEndPunchDuration = 0.25f;
    [SerializeField] private float gameEndFadeDuration = 0.45f;
    [SerializeField] private float tweenDuration = 0.35f;
    [SerializeField] private Ease punchEase = Ease.OutBack;
    [SerializeField] private Ease panelEase = Ease.OutBack;
    [SerializeField] private List<TextMeshProUGUI> deltaByPlayer;
    [SerializeField] private TextMeshProUGUI nightmareDelta;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI miniGameTypeText;

    private const float OpenHeight = 6000f;
    private Vector2 _bgOpenSize = new(200f, OpenHeight);

    private void Awake()
    {
        if (resultBg != null)
            _bgOpenSize = new Vector2(resultBg.sizeDelta.x, OpenHeight);

        SetHidden();
        SetPanelOpened(false);
        gameObject.SetActive(false);
    }

    public Sequence PlayGameEnd()
    {
        gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (gameEndGroup == null)
            return sequence;

        Transform gameEndTransform = gameEndGroup.transform;
        gameEndTransform.DOKill();
        gameEndGroup.DOKill();
        if (gameEndTxt != null)
            gameEndTxt.text = "게임 종료!";
        gameEndGroup.gameObject.SetActive(true);
        gameEndTransform.localScale = Vector3.one * 2f;
        gameEndGroup.alpha = 1f;

        sequence.Append(gameEndTransform.DOScale(Vector3.one, gameEndPunchDuration).SetEase(punchEase));
        sequence.Append(gameEndGroup.DOFade(0f, gameEndFadeDuration).SetEase(Ease.InQuad));
        sequence.OnComplete(() => gameEndGroup.gameObject.SetActive(false));
        return sequence;
    }

    public Sequence OpenResult(Dictionary<StateTypes, int> delta)
    {
        gameObject.SetActive(true);
        SetPanelOpened(false);
        KillPanelTweens();
        SetResultContent(delta);

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (resultBg != null)
            sequence.Append(resultBg.DOSizeDelta(_bgOpenSize, tweenDuration).SetEase(panelEase));

        if (resultInnerGroup != null)
            sequence.Append(resultInnerGroup.DOFade(1f, tweenDuration).SetEase(panelEase));

        return sequence;
    }

    public Sequence CloseResult()
    {
        KillPanelTweens();

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (resultInnerGroup != null)
            sequence.Append(resultInnerGroup.DOFade(0f, tweenDuration).SetEase(panelEase));

        if (resultBg != null)
            sequence.Append(resultBg.DOSizeDelta(new Vector2(_bgOpenSize.x, 0f), tweenDuration).SetEase(panelEase));

        sequence.OnComplete(() => gameObject.SetActive(false));
        return sequence;
    }

    protected virtual void SetResultContent(Dictionary<StateTypes, int> delta)
    {
        for (int i = 1; i < 5; i++)
        {
            if (!delta.TryGetValue((StateTypes)i, out int deltaValue))
                deltaByPlayer[i - 1].text = "0";
            else
                deltaByPlayer[i - 1].text = deltaValue.ToString();
        }
        
        nightmareDelta.text = delta.TryGetValue(StateTypes.Nightmare, out int nightmareValue) ? nightmareValue.ToString() : "0";

        titleText.text = MiniGameManager.Instance.ResultContainer.GameTitle;
        switch (MiniGameManager.Instance.ResultContainer.Type)
        {
            case MiniGameTypes.SoloBattle:
                miniGameTypeText.text = "< 개인전 >";
                break;
            
            case MiniGameTypes.OneVsThree:
                miniGameTypeText.text = "< 1 vs. 3 >";
                break;
            
            case MiniGameTypes.TwoVsTwo:
                miniGameTypeText.text = "< 2 vs. 2 >";
                break;
            
            case MiniGameTypes.AffectionBattle:
                miniGameTypeText.text = "< 호감도전 >";
                break;
            
            case MiniGameTypes.Cooperative:
                miniGameTypeText.text = "< 협동전 >";
                break;
        }
    }

    private void KillPanelTweens()
    {
        if (resultBg != null)
            resultBg.DOKill();

        if (resultInnerGroup != null)
            resultInnerGroup.DOKill();
    }

    private void SetPanelOpened(bool isOpened)
    {
        if (resultBg != null)
            resultBg.sizeDelta = new Vector2(_bgOpenSize.x, isOpened ? OpenHeight : 0f);

        if (resultInnerGroup != null)
            resultInnerGroup.alpha = isOpened ? 1f : 0f;
    }

    private void SetHidden()
    {
        if (gameEndGroup == null)
            return;

        gameEndGroup.alpha = 0f;
        gameEndGroup.transform.localScale = Vector3.one;
        gameEndGroup.gameObject.SetActive(false);
    }
}
