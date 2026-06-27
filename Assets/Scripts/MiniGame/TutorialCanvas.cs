using System;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform bg;
    [SerializeField] private CanvasGroup innerGroup;
    [SerializeField] private TMP_Text gameTypeTxt;
    [SerializeField] private TMP_Text descTxt;
    [SerializeField] private Transform conditionsRoot;
    [SerializeField] private TutorialConditionView oneVsThreeConditionsPrefab;
    [SerializeField] private TutorialConditionView twoVsTwoConditionsPrefab;
    [SerializeField] private TutorialConditionView soloBattleConditionsPrefab;
    [SerializeField] private TutorialConditionView cooperativeConditionsPrefab;
    [SerializeField] private float tweenDuration = 0.35f;
    [SerializeField] private Ease ease = Ease.OutBack;

    private const float OpenHeight = 600f;
    private Vector2 _bgOpenSize = new(200f, OpenHeight);
    private TutorialConditionView _currentConditions;

    private void Awake()
    {
        if (bg != null)
            _bgOpenSize = new Vector2(bg.sizeDelta.x, OpenHeight);

        SetOpened(false, false);
    }

    public void SetTutorial(MiniGameResultContainer data, MiniGameProcessorBase processor)
    {
        MiniGameTutorialContent content = MiniGameTutorialContentConverter.Convert(data, processor);
        gameTypeTxt.text = content.GameTypeText;
        descTxt.text = content.GameTitleText;

        ClearConditions();
        _currentConditions = Instantiate(GetConditionsPrefab(data.Type), conditionsRoot);
        _currentConditions.SetTexts(content);
    }

    public Sequence Open()
    {
        gameObject.SetActive(true);
        KillTweens();

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (bg != null)
            sequence.Append(bg.DOSizeDelta(_bgOpenSize, tweenDuration).SetEase(ease));

        if (innerGroup != null)
            sequence.Append(innerGroup.DOFade(1f, tweenDuration).SetEase(ease));

        return sequence;
    }

    public Sequence Close()
    {
        KillTweens();

        Sequence sequence = DOTween.Sequence().SetUpdate(true);
        if (innerGroup != null)
            sequence.Append(innerGroup.DOFade(0f, tweenDuration).SetEase(ease));

        if (bg != null)
            sequence.Append(bg.DOSizeDelta(new Vector2(_bgOpenSize.x, 0f), tweenDuration).SetEase(ease));

        sequence.OnComplete(() => gameObject.SetActive(false));
        return sequence;
    }

    private void KillTweens()
    {
        if (bg != null)
            bg.DOKill();

        if (innerGroup != null)
            innerGroup.DOKill();
    }

    private void SetOpened(bool isOpened, bool applyActiveState = true)
    {
        if (bg != null)
            bg.sizeDelta = new Vector2(_bgOpenSize.x, isOpened ? OpenHeight : 0f);

        if (innerGroup != null)
            innerGroup.alpha = isOpened ? 1f : 0f;

        if (applyActiveState)
            gameObject.SetActive(isOpened);
    }

    private TutorialConditionView GetConditionsPrefab(MiniGameTypes type)
    {
        return type switch
        {
            MiniGameTypes.OneVsThree => oneVsThreeConditionsPrefab,
            MiniGameTypes.TwoVsTwo => twoVsTwoConditionsPrefab,
            MiniGameTypes.SoloBattle => soloBattleConditionsPrefab,
            MiniGameTypes.Cooperative => cooperativeConditionsPrefab,
            _ => throw new NotSupportedException($"Tutorial conditions prefab is not assigned for {type}."),
        };
    }

    private void ClearConditions()
    {
        for (int i = conditionsRoot.childCount - 1; i >= 0; i--)
            Destroy(conditionsRoot.GetChild(i).gameObject);

        _currentConditions = null;
    }
}
