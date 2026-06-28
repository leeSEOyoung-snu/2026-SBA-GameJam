using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Febucci.TextAnimatorForUnity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VictoryCanvasManager : MonoBehaviour
{
    [Serializable]
    private struct PlayerWinnerSprite
    {
        public int playerId;
        public Sprite sprite;
    }

    private const string DefaultFirstText = "이 게임의 승자는...!";
    private const string DefaultSecondText = "그리고 협동해서 키운 해치의 주인은..";

    [Header("Test")]
    [SerializeField] private bool playOnStartForTest = false;
    [SerializeField] private float testDelay = 0.5f;
    [SerializeField] private int[] testWinnerPlayerIds = { 1 };

    [Header("Sprites")]
    [SerializeField] private List<PlayerWinnerSprite> winnerSprites = new();

    [Header("Text")]
    [SerializeField] private string firstText = DefaultFirstText;
    [SerializeField] private string secondText = DefaultSecondText;
    [SerializeField] private float manualTypewriterCharacterInterval = 0.035f;

    [Header("Timing")]
    [SerializeField] private float bgOpenDuration = 0.2f;
    [SerializeField] private float maskDelay = 0.5f;
    [SerializeField] private float maskOpenDuration = 0.3f;
    [SerializeField] private float winnerFadeDuration = 0.2f;
    [SerializeField] private float nameCrownScaleDuration = 0.2f;
    [SerializeField] private float chrMoveDuration = 0.3f;
    [SerializeField] private float hatFadeDuration = 0.2f;

    [Header("Layout")]
    [SerializeField] private float bgOpenWidth = 1920f;
    [SerializeField] private Vector2 maskingStartSize = new(100f, 100f);
    [SerializeField] private Vector2 maskingOpenSize = new(900f, 900f);
    [SerializeField] private float chrFinalPosX = -365f;
    [SerializeField] private float hechiScale = 2.2f;

    private CanvasGroup _rootGroup;
    private RectTransform _bg;
    private RectTransform _masking;
    private CanvasGroup _winnerGroup;
    private Image _winnerImage;
    private Image _hechiImage;
    private RectTransform _name;
    private TMP_Text _nameText;
    private RectTransform _crown;
    private RectTransform _chr;
    private CanvasGroup _hatGroup;
    private TMP_Text _winnerText;
    private TypewriterComponent _typewriter;
    private Coroutine _playRoutine;
    private Vector2 _bgOriginalSize;
    private Vector2 _chrOriginalPosition;
    private bool _cachedBgOriginalSize;
    private bool _cachedChrOriginalPosition;
    private Vector3 _nameBaseScale = Vector3.one;
    private Vector3 _crownBaseScale = Vector3.one * 3f;
    private bool _cachedNameBaseScale;
    private bool _cachedCrownBaseScale;

    private void Awake()
    {
        CacheReferences();
        ResetVisuals();
    }

    private IEnumerator Start()
    {
        if (!playOnStartForTest)
            yield break;

        yield return new WaitForSeconds(testDelay);
        PlayVictory(testWinnerPlayerIds);
    }

    public void PlayVictory(int[] winnerPlayerIds, int winningAffection = 0)
    {
        if (_playRoutine != null)
            StopCoroutine(_playRoutine);

        _playRoutine = StartCoroutine(PlayVictoryRoutine(winnerPlayerIds, winningAffection));
    }

    public void Hide()
    {
        if (_playRoutine != null)
        {
            StopCoroutine(_playRoutine);
            _playRoutine = null;
        }

        KillTweens();
        ResetVisuals();
    }

    private IEnumerator PlayVictoryRoutine(int[] winnerPlayerIds, int winningAffection)
    {
        CacheReferences();
        KillTweens();
        ResetVisuals();
        SetWinnerName(winnerPlayerIds);
        SetVictorySprites(winnerPlayerIds);

        _rootGroup.alpha = 1f;
        _rootGroup.blocksRaycasts = true;

        if (_bg != null)
        {
            yield return _bg
                .DOSizeDelta(new Vector2(bgOpenWidth, _bg.sizeDelta.y), bgOpenDuration)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }

        yield return PlayTypewriter(string.IsNullOrWhiteSpace(firstText) ? DefaultFirstText : firstText);

        if (_masking != null)
            _masking.gameObject.SetActive(true);

        yield return new WaitForSeconds(maskDelay);

        if (_masking != null)
        {
            yield return _masking
                .DOSizeDelta(maskingOpenSize, maskOpenDuration)
                .SetEase(Ease.OutBack)
                .WaitForCompletion();
        }

        if (_winnerGroup != null)
        {
            yield return _winnerGroup
                .DOFade(1f, winnerFadeDuration)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }

        yield return ShowNameAndCrown();
        yield return PlayTypewriter(string.IsNullOrWhiteSpace(secondText) ? DefaultSecondText : secondText);

        if (_chr != null)
        {
            yield return _chr
                .DOAnchorPosX(chrFinalPosX, chrMoveDuration)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }

        if (_hatGroup != null)
        {
            yield return _hatGroup
                .DOFade(1f, hatFadeDuration)
                .SetEase(Ease.OutCubic)
                .WaitForCompletion();
        }

        _playRoutine = null;
    }

    private IEnumerator ShowNameAndCrown()
    {
        Sequence sequence = DOTween.Sequence();
        bool hasTween = false;

        if (_name != null)
        {
            _name.gameObject.SetActive(true);
            _name.localScale = _nameBaseScale * 2f;
            sequence.Join(_name.DOScale(_nameBaseScale, nameCrownScaleDuration).SetEase(Ease.OutBack));
            hasTween = true;
        }

        if (_crown != null)
        {
            _crown.gameObject.SetActive(true);
            _crown.localScale = _crownBaseScale * 2f;
            sequence.Join(_crown.DOScale(_crownBaseScale, nameCrownScaleDuration).SetEase(Ease.OutBack));
            hasTween = true;
        }

        if (!hasTween)
        {
            sequence.Kill();
            yield break;
        }

        yield return sequence.WaitForCompletion();
    }

    private IEnumerator PlayTypewriter(string text)
    {
        if (_winnerText == null)
            yield break;

        bool started = TryStartTextAnimatorTypewriter(text);
        if (started)
        {
            yield return null;

            while (_typewriter != null && _typewriter.IsShowingText)
                yield return null;

            yield break;
        }

        yield return PlayManualTypewriter(text);
    }

    private bool TryStartTextAnimatorTypewriter(string text)
    {
        if (_typewriter == null)
            return false;

        try
        {
            _typewriter.ShowText(text);
            _typewriter.StartShowingText(true);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"[VictoryCanvasManager] TMP Animator typewriter failed. Falling back to TMP reveal. {exception.Message}", this);
            return false;
        }
    }

    private IEnumerator PlayManualTypewriter(string text)
    {
        _winnerText.text = text;
        _winnerText.maxVisibleCharacters = 0;
        _winnerText.ForceMeshUpdate();

        int characterCount = _winnerText.textInfo.characterCount;
        for (int i = 0; i <= characterCount; i++)
        {
            _winnerText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(manualTypewriterCharacterInterval);
        }

        _winnerText.maxVisibleCharacters = int.MaxValue;
    }

    private void CacheReferences()
    {
        transform.localScale = Vector3.one;

        _rootGroup = GetOrAddCanvasGroup(gameObject);
        _bg = FindChildRect("BG");
        _masking = FindChildRect("Masking");
        GameObject winnerObject = FindChild("Winner");
        _winnerGroup = GetOrAddCanvasGroup(winnerObject);
        _winnerImage = winnerObject != null ? winnerObject.GetComponent<Image>() : null;
        _hechiImage = FindChild("HaTChi")?.GetComponent<Image>();
        _name = FindChildRect("Name");
        _nameText = FindChild("NameP")?.GetComponent<TMP_Text>();
        _crown = FindChildRect("Crown");
        _chr = FindChildRect("CHR");
        _hatGroup = GetOrAddCanvasGroup(FindChild("HAT"));

        GameObject winnerTextObject = FindChild("WinnertText");
        if (winnerTextObject != null)
        {
            _winnerText = winnerTextObject.GetComponent<TMP_Text>();
            winnerTextObject.TryGetComponent<TypewriterComponent>(out _typewriter);
        }

        if (_bg != null && !_cachedBgOriginalSize)
        {
            _bgOriginalSize = _bg.sizeDelta;
            _cachedBgOriginalSize = true;
        }

        if (_chr != null && !_cachedChrOriginalPosition)
        {
            _chrOriginalPosition = _chr.anchoredPosition;
            _cachedChrOriginalPosition = true;
        }

        if (_name != null && !_cachedNameBaseScale)
        {
            _nameBaseScale = _name.localScale;
            _cachedNameBaseScale = true;
        }

        if (_crown != null && !_cachedCrownBaseScale)
        {
            _crownBaseScale = _crown.localScale;
            _cachedCrownBaseScale = true;
        }
    }

    private void ResetVisuals()
    {
        transform.localScale = Vector3.one;
        _rootGroup.alpha = 0f;
        _rootGroup.interactable = false;
        _rootGroup.blocksRaycasts = false;

        if (_bg != null)
            _bg.sizeDelta = new Vector2(0f, _bgOriginalSize == Vector2.zero ? _bg.sizeDelta.y : _bgOriginalSize.y);

        if (_winnerText != null)
        {
            _winnerText.text = string.Empty;
            _winnerText.maxVisibleCharacters = int.MaxValue;
        }

        if (_masking != null)
        {
            _masking.gameObject.SetActive(false);
            _masking.sizeDelta = maskingStartSize;
        }

        if (_winnerGroup != null)
        {
            _winnerGroup.alpha = 0f;
            _winnerGroup.interactable = false;
            _winnerGroup.blocksRaycasts = false;
        }

        if (_name != null)
        {
            _name.gameObject.SetActive(false);
            _name.localScale = _nameBaseScale * 2f;
        }

        if (_crown != null)
        {
            _crown.gameObject.SetActive(false);
            _crown.localScale = _crownBaseScale * 2f;
        }

        if (_chr != null)
            _chr.anchoredPosition = _chrOriginalPosition;

        if (_hatGroup != null)
        {
            _hatGroup.alpha = 0f;
            _hatGroup.interactable = false;
            _hatGroup.blocksRaycasts = false;
        }
    }

    private void SetWinnerName(int[] winnerPlayerIds)
    {
        if (_nameText == null)
            return;

        int[] displayWinnerPlayerIds = winnerPlayerIds != null && winnerPlayerIds.Length > 0
            ? winnerPlayerIds
            : new[] { GetPrimaryWinnerPlayerId(winnerPlayerIds) };

        _nameText.text = displayWinnerPlayerIds.Length == 1
            ? $"{displayWinnerPlayerIds[0]}P"
            : string.Join(" / ", Array.ConvertAll(displayWinnerPlayerIds, id => $"{id}P"));
    }

    private void SetVictorySprites(int[] winnerPlayerIds)
    {
        int winnerPlayerId = GetPrimaryWinnerPlayerId(winnerPlayerIds);
        Sprite winnerSprite = GetWinnerSprite(winnerPlayerId);
        if (_winnerImage != null && winnerSprite != null)
            _winnerImage.sprite = winnerSprite;

        if (_hechiImage != null && GameManager.Instance != null)
        {
            Sprite hechiSprite = GameManager.Instance.GetHechiSpriteOnMiniGame();
            if (hechiSprite != null)
            {
                _hechiImage.sprite = hechiSprite;
                _hechiImage.SetNativeSize();
                _hechiImage.rectTransform.localScale = Vector3.one * hechiScale;
            }
        }
    }

    private int GetPrimaryWinnerPlayerId(int[] winnerPlayerIds)
    {
        if (winnerPlayerIds != null && winnerPlayerIds.Length > 0)
            return winnerPlayerIds[0];

        return testWinnerPlayerIds != null && testWinnerPlayerIds.Length > 0
            ? testWinnerPlayerIds[0]
            : 1;
    }

    private Sprite GetWinnerSprite(int winnerPlayerId)
    {
        foreach (PlayerWinnerSprite entry in winnerSprites)
        {
            if (entry.playerId == winnerPlayerId && entry.sprite != null)
                return entry.sprite;
        }

        return null;
    }

    private void KillTweens()
    {
        _bg?.DOKill();
        _masking?.DOKill();
        _winnerGroup?.DOKill();
        _name?.DOKill();
        _crown?.DOKill();
        _chr?.DOKill();
        _hatGroup?.DOKill();
    }

    private GameObject FindChild(string childName)
    {
        Transform[] children = GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == childName)
                return child.gameObject;
        }

        return null;
    }

    private RectTransform FindChildRect(string childName)
    {
        return FindChild(childName)?.GetComponent<RectTransform>();
    }

    private static CanvasGroup GetOrAddCanvasGroup(GameObject target)
    {
        if (target == null)
            return null;

        return target.TryGetComponent(out CanvasGroup canvasGroup)
            ? canvasGroup
            : target.AddComponent<CanvasGroup>();
    }
}
