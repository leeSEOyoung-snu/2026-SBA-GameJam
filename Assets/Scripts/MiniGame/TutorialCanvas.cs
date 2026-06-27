using System;
using System.Globalization;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class TutorialCanvas : MonoBehaviour
{
    [SerializeField] private RectTransform bg;
    [SerializeField] private CanvasGroup innerGroup;
    [SerializeField] private TMP_Text gameTypeTxt;
    [SerializeField] private TMP_Text titleTxt;
    [SerializeField] private TMP_Text descTxt;
    [SerializeField] private TMP_SpriteAsset keyIconSpriteAsset;
    [SerializeField, Range(50, 300)] private int keyIconSizePercent = 140;
    [SerializeField, Range(-1f, 1f)] private float keyIconHorizontalOffsetEm;
    [SerializeField, Range(-1f, 1f)] private float keyIconVerticalOffsetEm;
    [SerializeField] private Color keyIconColor = Color.white;
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
        titleTxt.text = content.GameTitleText;
        descTxt.spriteAsset = keyIconSpriteAsset;
        descTxt.text = ConvertKeyIconTags(content.GameDescText);

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

    private string ConvertKeyIconTags(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        return text
            .Replace("<SL>", FormatKeyIconTag("SL"))
            .Replace("<SR>", FormatKeyIconTag("SR"))
            .Replace("<L>", FormatKeyIconTag("L"))
            .Replace("<U>", FormatKeyIconTag("U"))
            .Replace("<R>", FormatKeyIconTag("R"))
            .Replace("<D>", FormatKeyIconTag("D"))
            .Replace("<Joy>", FormatKeyIconTag("Joy"));
    }

    private string FormatKeyIconTag(string iconName)
    {
        string horizontalOffset = Mathf.Abs(keyIconHorizontalOffsetEm).ToString("0.###", CultureInfo.InvariantCulture);
        string offset = keyIconVerticalOffsetEm.ToString("0.###", CultureInfo.InvariantCulture);
        string icon = $"<voffset={offset}em><size={keyIconSizePercent}%><sprite name=\"{iconName}\" color=#{ColorUtility.ToHtmlStringRGBA(keyIconColor)}></size></voffset>";

        if (keyIconHorizontalOffsetEm > 0f)
            return $"<space={horizontalOffset}em>{icon}";

        if (keyIconHorizontalOffsetEm < 0f)
            return $"{icon}<space={horizontalOffset}em>";

        return icon;
    }

    private void ClearConditions()
    {
        for (int i = conditionsRoot.childCount - 1; i >= 0; i--)
            Destroy(conditionsRoot.GetChild(i).gameObject);

        _currentConditions = null;
    }
}
