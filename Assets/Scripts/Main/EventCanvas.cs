using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class EventCanvas : MonoBehaviour
{
    [Serializable]
    private struct PlayerEventSprites
    {
        public int playerId;
        public Sprite first;
        public Sprite second;
    }

    [Serializable]
    private struct StateIconVisual
    {
        public StateTypes type;
        public Sprite icon;
        public Color color;
    }

    [Serializable]
    private struct AffectionStealCountView
    {
        public CanvasGroup countGroup;
        public TextMeshProUGUI signText;
        public TextMeshProUGUI amountText;

        public void Reset()
        {
            if (countGroup != null)
                countGroup.alpha = 0f;
        }

        public void Set(string sign, int amount)
        {
            if (signText != null)
                signText.text = sign;
            if (amountText != null)
                amountText.text = Mathf.Abs(amount).ToString();
            if (countGroup != null)
                countGroup.alpha = 1f;
        }
    }

    [Header("References")]
    [SerializeField] private CanvasGroup affectionStealMainEvent;
    [SerializeField] private CanvasGroup stateChangeMainEvent;
    [SerializeField] private RectTransform eventImgArea;
    [SerializeField] private RectTransform eventType;
    [SerializeField] private TextMeshProUGUI eventTypeTxt;
    [SerializeField] private Image eventImg;
    [SerializeField] private Image stateIconImage;
    [SerializeField] private TextMeshProUGUI stateSignText;
    [SerializeField] private TextMeshProUGUI stateAmountText;
    [SerializeField] private GameObject affectionStealInputDesc;
    [SerializeField] private RectTransform affectionStealThiefPortrait;
    [SerializeField] private AffectionStealCountView affectionStealThiefCount;
    [SerializeField] private AffectionStealCountView affectionStealTargetCount;

    [Header("Positions")]
    [SerializeField] private float hiddenPosX = 1300f;
    [SerializeField] private float eventTypeOpenPosX = 730f;
    [SerializeField] private float eventImgAreaOpenPosX = 430f;

    [Header("Open Timing")]
    [SerializeField] private float displayDuration = 3f;
    [SerializeField] private float eventTypeMoveDuration = 0.2f;
    [SerializeField] private float eventImgAreaMoveDelay = 0.1f;
    [SerializeField] private float eventImgAreaMoveDuration = 0.2f;
    [SerializeField] private float mainEventFadeDelay = 1f;
    [SerializeField] private float mainEventFadeDuration = 0.2f;
    [SerializeField] private Ease openEase = Ease.OutQuad;
    [SerializeField] private float affectionStealResultRevealDelay = 0.2f;
    [SerializeField] private float affectionStealResultDisplayDuration = 2f;
    [SerializeField] private float affectionStealPunchScale = 0.2f;
    [SerializeField] private float affectionStealPunchDuration = 0.25f;

    [Header("Close Timing")]
    [SerializeField] private float closeDuration = 0.2f;
    [SerializeField] private Ease closeEase = Ease.InQuad;

    [Header("Text")]
    [SerializeField] private string affectionStealText = "호감도 뺏기!";
    [SerializeField] private string stateChangeText = "상태 변화!";
    [SerializeField] private string nightmarePitText = "악몽 구덩이!";

    [Header("Images")]
    [SerializeField] private List<PlayerEventSprites> affectionStealSprites = new();
    [SerializeField] private List<StateIconVisual> stateIconVisuals = new();
    [SerializeField] private Sprite nightmarePitSprite;
    [SerializeField] private float affectionFrameInterval = 0.18f;

    private readonly Dictionary<int, Sprite[]> _affectionStealSpritesByPlayerId = new();
    private readonly Dictionary<StateTypes, StateIconVisual> _stateIconVisualsByType = new();
    private Sequence _activeSequence;
    private Sequence _affectionImageSequence;
    private CanvasGroup _currentMainEvent;
    private Vector3 _affectionStealThiefPortraitBaseScale = Vector3.one;

    public float DisplayDuration => displayDuration;

    private void Awake()
    {
        if (affectionStealThiefPortrait != null)
            _affectionStealThiefPortraitBaseScale = affectionStealThiefPortrait.localScale;

        InitAffectionSpriteMap();
        InitStateIconMap();
        ResetView();
    }

    private void OnDestroy()
    {
        KillSequences();
    }

    public Sequence Open(CellType cellType, StateContainer stateContainer, IBoardEvent boardEvent)
    {
        ApplyEventData(cellType, stateContainer, boardEvent);
        ResetView();
        KillActiveSequence();

        _activeSequence = DOTween.Sequence();
        _activeSequence.Append(eventType.DOAnchorPosX(eventTypeOpenPosX, eventTypeMoveDuration).SetEase(openEase));
        _activeSequence.Insert(eventImgAreaMoveDelay,
            eventImgArea.DOAnchorPosX(eventImgAreaOpenPosX, eventImgAreaMoveDuration).SetEase(openEase));
        if (_currentMainEvent != null)
        {
            _currentMainEvent.interactable = true;
            _currentMainEvent.blocksRaycasts = true;
            _activeSequence.Insert(mainEventFadeDelay, _currentMainEvent.DOFade(1f, mainEventFadeDuration));
        }

        return _activeSequence;
    }

    public Sequence Close()
    {
        KillActiveSequence();
        KillAffectionImageSequence();

        _activeSequence = DOTween.Sequence();
        if (_currentMainEvent != null)
            _activeSequence.Join(_currentMainEvent.DOFade(0f, closeDuration));
        _activeSequence.Join(eventImgArea.DOAnchorPosX(hiddenPosX, closeDuration).SetEase(closeEase));
        _activeSequence.Join(eventType.DOAnchorPosX(hiddenPosX, closeDuration).SetEase(closeEase));
        _activeSequence.OnComplete(() => SetMainEventHidden(_currentMainEvent));

        return _activeSequence;
    }

    public IEnumerator PlayAffectionStealResult(AffectionStealEvent affectionStealEvent)
    {
        PunchAffectionStealThiefPortrait();

        if (affectionStealInputDesc != null)
            affectionStealInputDesc.SetActive(false);

        yield return new WaitForSeconds(affectionStealResultRevealDelay);

        int stealAmount = affectionStealEvent != null ? affectionStealEvent.StealAmount : 0;
        affectionStealThiefCount.Set("+", stealAmount);
        affectionStealTargetCount.Set("-", stealAmount);

        yield return new WaitForSeconds(affectionStealResultDisplayDuration);
    }

    private void ApplyEventData(CellType cellType, StateContainer stateContainer, IBoardEvent boardEvent)
    {
        KillAffectionImageSequence();

        switch (cellType)
        {
            case CellType.AffectionSteal:
                SelectMainEvent(affectionStealMainEvent);
                eventTypeTxt.text = affectionStealText;
                ApplyAffectionStealImage(stateContainer, boardEvent);
                break;
            case CellType.StateChange:
                SelectMainEvent(stateChangeMainEvent);
                eventTypeTxt.text = stateChangeText;
                eventImg.sprite = GameManager.Instance.GetHechiSpriteOnMiniGame();
                ApplyStateChangeData(boardEvent);
                break;
            case CellType.NightmarePit:
                SelectMainEvent(stateChangeMainEvent);
                eventTypeTxt.text = nightmarePitText;
                eventImg.sprite = nightmarePitSprite;
                ApplyStateChangeData(boardEvent);
                break;
        }
    }

    private void ApplyStateChangeData(IBoardEvent boardEvent)
    {
        StateTypes stateType;
        int delta;

        if (boardEvent is StateChangeEvent stateChangeEvent)
        {
            stateType = stateChangeEvent.Target;
            delta = stateChangeEvent.Delta;
        }
        else if (boardEvent is NightmarePitEvent nightmarePitEvent)
        {
            stateType = nightmarePitEvent.Target;
            delta = nightmarePitEvent.Delta;
        }
        else
        {
            return;
        }

        stateSignText.text = delta >= 0 ? "+" : "-";
        stateAmountText.text = Mathf.Abs(delta).ToString();

        if (!_stateIconVisualsByType.TryGetValue(stateType, out StateIconVisual visual))
        {
            stateIconImage.sprite = null;
            return;
        }

        stateIconImage.sprite = visual.icon;
        stateSignText.color = visual.color;
        stateAmountText.color = visual.color;
    }

    private void ApplyAffectionStealImage(StateContainer stateContainer, IBoardEvent boardEvent)
    {
        int playerId = boardEvent is AffectionStealEvent affectionStealEvent
            ? affectionStealEvent.ThiefId
            : GetLowestAffectionPlayerId(stateContainer.AffectionById);
        if (!_affectionStealSpritesByPlayerId.TryGetValue(playerId, out Sprite[] sprites) || sprites.Length == 0)
        {
            eventImg.sprite = null;
            return;
        }

        eventImg.sprite = sprites[0];

        if (sprites.Length < 2)
            return;

        int frame = 0;
        _affectionImageSequence = DOTween.Sequence()
            .AppendCallback(() =>
            {
                eventImg.sprite = sprites[frame];
                frame = (frame + 1) % sprites.Length;
            })
            .AppendInterval(affectionFrameInterval)
            .SetLoops(-1);
    }

    private int GetLowestAffectionPlayerId(Dictionary<int, int> affectionById)
    {
        int minAffection = affectionById.Min(pair => pair.Value);
        List<int> lowestPlayerIds = affectionById
            .Where(pair => pair.Value == minAffection)
            .Select(pair => pair.Key)
            .ToList();

        return lowestPlayerIds[Random.Range(0, lowestPlayerIds.Count)];
    }

    private void InitAffectionSpriteMap()
    {
        _affectionStealSpritesByPlayerId.Clear();

        foreach (PlayerEventSprites sprites in affectionStealSprites)
        {
            List<Sprite> validSprites = new();
            if (sprites.first != null) validSprites.Add(sprites.first);
            if (sprites.second != null) validSprites.Add(sprites.second);

            _affectionStealSpritesByPlayerId[sprites.playerId] = validSprites.ToArray();
        }
    }

    private void InitStateIconMap()
    {
        _stateIconVisualsByType.Clear();

        foreach (StateIconVisual visual in stateIconVisuals)
            _stateIconVisualsByType[visual.type] = visual;
    }

    private void ResetView()
    {
        SetMainEventHidden(affectionStealMainEvent);
        SetMainEventHidden(stateChangeMainEvent);
        eventImgArea.anchoredPosition = new Vector2(hiddenPosX, eventImgArea.anchoredPosition.y);
        eventType.anchoredPosition = new Vector2(hiddenPosX, eventType.anchoredPosition.y);
        ResetAffectionStealResultView();
    }

    private void SelectMainEvent(CanvasGroup mainEvent)
    {
        _currentMainEvent = mainEvent;
        SetMainEventHidden(affectionStealMainEvent);
        SetMainEventHidden(stateChangeMainEvent);
    }

    private static void SetMainEventHidden(CanvasGroup mainEvent)
    {
        if (mainEvent == null)
            return;

        mainEvent.alpha = 0f;
        mainEvent.interactable = false;
        mainEvent.blocksRaycasts = false;
    }

    private void ResetAffectionStealResultView()
    {
        if (affectionStealInputDesc != null)
            affectionStealInputDesc.SetActive(true);

        affectionStealThiefCount.Reset();
        affectionStealTargetCount.Reset();

        if (affectionStealThiefPortrait != null)
        {
            affectionStealThiefPortrait.DOKill();
            affectionStealThiefPortrait.localScale = _affectionStealThiefPortraitBaseScale;
        }
    }

    private void PunchAffectionStealThiefPortrait()
    {
        if (affectionStealThiefPortrait == null)
            return;

        affectionStealThiefPortrait.DOKill();
        affectionStealThiefPortrait.localScale = _affectionStealThiefPortraitBaseScale;
        affectionStealThiefPortrait.DOPunchScale(
            Vector3.one * affectionStealPunchScale,
            affectionStealPunchDuration,
            8,
            1f);
    }

    private void KillSequences()
    {
        KillActiveSequence();
        KillAffectionImageSequence();
    }

    private void KillActiveSequence()
    {
        if (_activeSequence != null && _activeSequence.IsActive())
            _activeSequence.Kill();
        _activeSequence = null;
    }

    private void KillAffectionImageSequence()
    {
        if (_affectionImageSequence != null && _affectionImageSequence.IsActive())
            _affectionImageSequence.Kill();
        _affectionImageSequence = null;
    }
}
