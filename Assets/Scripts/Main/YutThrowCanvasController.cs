using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class YutThrowCanvasController : MonoBehaviour
{
    private const int PlayerCount = 4;
    private const float PlayerExitTargetY = -800f;
    private const float PlayerExitDuration = 0.3f;
    private const float RevealStartDelay = 0.5f;
    private const float RevealGrowDuration = 1.2f;
    private const float RevealShrinkDuration = 0.8f;
    private const float RevealFrameInterval = 0.08f;
    private const float RevealMaxRotation = 360f;
    private const float ActiveBgWobbleSpeed = 20f;
    private const float ActiveBgWobbleAmount = 0.5f;
    private const float ActiveBgWidth = 1500f;
    private const float BgWidthTweenDuration = 0.1f;
    private const float CharacterPunchDuration = 0.18f;
    private const int CharacterPunchVibrato = 8;
    private const float CharacterPunchElasticity = 0.8f;
    private static readonly Vector3 CharacterPunchScale = new(0.22f, 0.22f, 0f);
    private static readonly int SpeedId = Shader.PropertyToID("_Speed");
    private static readonly int WobbleAmountId = Shader.PropertyToID("_WobbleAmount");

    private static readonly Key[] TestThrowKeys =
    {
        Key.Z,
        Key.X,
        Key.C,
        Key.V
    };

    [Serializable]
    private class PlayerThrowView
    {
        public Image playerImage;
        public Sprite throwingSprite;
        public RectTransform yut;

        [NonSerialized] public Sprite StandingSprite;
        [NonSerialized] public Image YutImage;
        [NonSerialized] public Vector2 PlayerStartPosition;
        [NonSerialized] public Vector2 YutStartPosition;
        [NonSerialized] public Quaternion YutStartRotation;
        [NonSerialized] public Vector3 PlayerStartScale;
    }

    [SerializeField] private PlayerThrowView[] players = new PlayerThrowView[PlayerCount];
    [SerializeField] private Image bgImage;
    [SerializeField] private CanvasGroup descGroup;
    [SerializeField] private Sprite[] yutFrontSprites;
    [SerializeField] private Sprite[] yutBackSprites;
    [SerializeField] private float yutTargetY = 1000f;
    [SerializeField] private float yutFlyDuration = 0.45f;
    [SerializeField] private Ease yutFlyEase = Ease.OutCubic;
    [SerializeField] private float descFadeDuration = 0.15f;
    [SerializeField] private bool resetOnEnable = true;

    private bool _originalsCaptured;
    private bool _isRevealAnimating;
    private RectTransform _playersRoot;
    private Vector2 _playersRootStartPosition;
    private RectTransform _bgRect;
    private Material _bgMaterialInstance;
    private float _bgOriginalWidth;
    private float _bgOriginalSpeed;
    private float _bgOriginalWobbleAmount;

    private void Awake()
    {
        EnsurePlayerSlots();
        CaptureOriginals();
        PrepareBgMaterial();
    }

    private void OnEnable()
    {
        if (resetOnEnable)
            ResetView();
    }

    private void OnDisable()
    {
        RestoreBgWobble();
    }

    public void Open()
    {
        gameObject.SetActive(true);
        ResetView();
    }

    public IEnumerator PlayThrowRoutine(IPlayerInputReader[] inputReaders, Action<YutType> onComplete)
    {
        EnsurePlayerSlots();
        CaptureOriginals();
        ResetView();

        bool[] results = new bool[PlayerCount];
        bool[] hasThrown = new bool[PlayerCount];
        int thrownCount = 0;

        while (thrownCount < PlayerCount)
        {
            for (int i = 0; i < PlayerCount; i++)
            {
                if (hasThrown[i] || !IsThrowPressed(i, inputReaders))
                    continue;

                hasThrown[i] = true;
                results[i] = UnityEngine.Random.value > 0.5f;
                thrownCount++;
                PlayPlayerThrow(i);
            }

            yield return null;
        }

        if (yutFlyDuration > 0f)
            yield return new WaitForSeconds(yutFlyDuration);

        yield return FadeOutDesc();
        yield return PlayResultReveal(results);

        YutType yut = YutCalculator.Calculate(results);
        Debug.Log($"[YutCanvas] 결과: {yut} ({YutCalculator.ToMoveCount(yut)}칸 이동)");
        onComplete?.Invoke(yut);
    }

    public void ResetView()
    {
        EnsurePlayerSlots();
        CaptureOriginals();

        if (_playersRoot != null)
        {
            _playersRoot.DOKill();
            _playersRoot.anchoredPosition = _playersRootStartPosition;
        }

        for (int i = 0; i < players.Length; i++)
        {
            PlayerThrowView player = players[i];

            if (player.playerImage != null)
            {
                RectTransform playerRect = player.playerImage.rectTransform;
                playerRect.DOKill();
                playerRect.anchoredPosition = player.PlayerStartPosition;
                playerRect.localScale = player.PlayerStartScale;

                if (player.StandingSprite != null)
                    player.playerImage.sprite = player.StandingSprite;
            }

            if (player.yut != null)
            {
                player.yut.DOKill();
                player.yut.anchoredPosition = player.YutStartPosition;
                player.yut.localRotation = player.YutStartRotation;
                player.yut.localScale = Vector3.one;
            }
        }

        if (descGroup != null)
        {
            descGroup.DOKill();
            descGroup.alpha = 1f;
            descGroup.interactable = true;
            descGroup.blocksRaycasts = true;
        }

        RestoreBgWobble();
    }

    private void PlayPlayerThrow(int index)
    {
        PlayerThrowView player = players[index];

        if (player.playerImage != null && player.throwingSprite != null)
        {
            RectTransform playerTransform = player.playerImage.rectTransform;
            playerTransform.DOKill();
            playerTransform.localScale = player.PlayerStartScale;
            player.playerImage.sprite = player.throwingSprite;
            playerTransform.DOPunchScale(
                CharacterPunchScale,
                CharacterPunchDuration,
                CharacterPunchVibrato,
                CharacterPunchElasticity);
        }

        if (player.yut == null)
            return;

        player.yut.DOKill();
        Vector2 targetPosition = player.yut.anchoredPosition;
        targetPosition.y = yutTargetY;
        player.yut.DOAnchorPos(targetPosition, yutFlyDuration).SetEase(yutFlyEase);

        Debug.Log($"[YutCanvas] Player {index + 1} 윷 던짐");
    }

    private IEnumerator PlayResultReveal(bool[] results)
    {
        SetActiveBgWobble();
        yield return MovePlayersOut();

        for (int i = 0; i < players.Length; i++)
            PrepareYutForReveal(players[i]);

        yield return new WaitForSeconds(RevealStartDelay);

        _isRevealAnimating = true;
        Coroutine revealAnimation = StartCoroutine(AnimateRevealSprites());

        yield return ScaleYuts(Vector3.one * 2f, RevealGrowDuration, Ease.OutBack);
        RestoreBgWobble();
        yield return ScaleYuts(Vector3.one, RevealShrinkDuration, Ease.InOutCubic);

        _isRevealAnimating = false;
        if (revealAnimation != null)
            StopCoroutine(revealAnimation);

        SetFinalYutSprites(results);
        SetNativeYutSizes();
    }

    private IEnumerator MovePlayersOut()
    {
        if (_playersRoot != null)
        {
            _playersRoot.DOKill();
            yield return _playersRoot.DOAnchorPosY(PlayerExitTargetY, PlayerExitDuration)
                .SetEase(Ease.InQuad)
                .WaitForCompletion();
            yield break;
        }

        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < players.Length; i++)
        {
            PlayerThrowView player = players[i];
            if (player.playerImage == null)
                continue;

            RectTransform playerRect = player.playerImage.rectTransform;
            playerRect.DOKill();
            sequence.Join(playerRect.DOAnchorPosY(PlayerExitTargetY, PlayerExitDuration).SetEase(Ease.InQuad));
        }

        yield return sequence.WaitForCompletion();
    }

    private void PrepareYutForReveal(PlayerThrowView player)
    {
        if (player.yut == null)
            return;

        player.yut.DOKill();
        player.yut.anchoredPosition = player.YutStartPosition;
        player.yut.localScale = Vector3.zero;
        player.yut.localRotation = player.YutStartRotation;
        player.yut.gameObject.SetActive(true);
    }

    private IEnumerator ScaleYuts(Vector3 targetScale, float duration, Ease ease)
    {
        Sequence sequence = DOTween.Sequence();

        for (int i = 0; i < players.Length; i++)
        {
            RectTransform yut = players[i].yut;
            if (yut == null)
                continue;

            yut.DOKill();
            sequence.Join(yut.DOScale(targetScale, duration).SetEase(ease));
        }

        yield return sequence.WaitForCompletion();
    }

    private IEnumerator AnimateRevealSprites()
    {
        while (_isRevealAnimating)
        {
            for (int i = 0; i < players.Length; i++)
            {
                PlayerThrowView player = players[i];
                Sprite sprite = GetRandomRevealSprite();
                if (player.YutImage != null && sprite != null)
                    player.YutImage.sprite = sprite;

                if (player.yut != null)
                    player.yut.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, RevealMaxRotation));
            }

            yield return new WaitForSeconds(RevealFrameInterval);
        }
    }

    private void SetFinalYutSprites(bool[] results)
    {
        for (int i = 0; i < players.Length; i++)
        {
            PlayerThrowView player = players[i];
            if (player.yut != null)
            {
                player.yut.localScale = Vector3.one;
                player.yut.localEulerAngles = new Vector3(0f, 0f, UnityEngine.Random.Range(0f, RevealMaxRotation));
            }

            if (player.YutImage == null)
                continue;

            Sprite finalSprite = GetFinalYutSprite(results[i]);
            if (finalSprite != null)
                player.YutImage.sprite = finalSprite;
        }
    }

    private void SetNativeYutSizes()
    {
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i].YutImage == null)
                continue;

            players[i].YutImage.SetNativeSize();
        }
    }

    private IEnumerator FadeOutDesc()
    {
        if (descGroup == null)
            yield break;

        descGroup.interactable = false;
        descGroup.blocksRaycasts = false;
        descGroup.DOKill();

        if (descFadeDuration <= 0f)
        {
            descGroup.alpha = 0f;
            yield break;
        }

        yield return descGroup.DOFade(0f, descFadeDuration).WaitForCompletion();
    }

    private static bool IsThrowPressed(int playerIndex, IPlayerInputReader[] inputReaders)
    {
        Keyboard keyboard = Keyboard.current;
        if (keyboard != null && keyboard[TestThrowKeys[playerIndex]].wasPressedThisFrame)
            return true;

        if (inputReaders == null || playerIndex >= inputReaders.Length)
            return false;

        IPlayerInputReader input = inputReaders[playerIndex];
        return input != null && input.Swing;
    }

    private void CaptureOriginals()
    {
        if (_originalsCaptured)
            return;

        for (int i = 0; i < players.Length; i++)
        {
            PlayerThrowView player = players[i];
            if (player.playerImage != null)
            {
                RectTransform playerRect = player.playerImage.rectTransform;
                player.StandingSprite = player.playerImage.sprite;
                player.PlayerStartPosition = playerRect.anchoredPosition;
                player.PlayerStartScale = playerRect.localScale;

                if (_playersRoot == null)
                {
                    _playersRoot = playerRect.parent as RectTransform;
                    if (_playersRoot != null)
                        _playersRootStartPosition = _playersRoot.anchoredPosition;
                }
            }

            if (player.yut != null)
            {
                player.YutImage = player.yut.GetComponent<Image>();
                player.YutStartPosition = player.yut.anchoredPosition;
                player.YutStartRotation = player.yut.localRotation;
            }
        }

        _originalsCaptured = true;
    }

    private void EnsurePlayerSlots()
    {
        if (players == null || players.Length != PlayerCount)
            Array.Resize(ref players, PlayerCount);

        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] == null)
                players[i] = new PlayerThrowView();
        }
    }

    private void PrepareBgMaterial()
    {
        if (bgImage == null || bgImage.material == null || _bgMaterialInstance != null)
            return;

        _bgMaterialInstance = Instantiate(bgImage.material);
        _bgMaterialInstance.name = $"{bgImage.material.name} (Yut Runtime)";
        bgImage.material = _bgMaterialInstance;
        _bgRect = bgImage.rectTransform;
        _bgOriginalWidth = _bgRect.sizeDelta.x;

        if (_bgMaterialInstance.HasProperty(SpeedId))
            _bgOriginalSpeed = _bgMaterialInstance.GetFloat(SpeedId);

        if (_bgMaterialInstance.HasProperty(WobbleAmountId))
            _bgOriginalWobbleAmount = _bgMaterialInstance.GetFloat(WobbleAmountId);
    }

    private void SetActiveBgWobble()
    {
        PrepareBgMaterial();

        if (_bgMaterialInstance == null)
            return;

        if (_bgMaterialInstance.HasProperty(SpeedId))
            _bgMaterialInstance.SetFloat(SpeedId, ActiveBgWobbleSpeed);

        if (_bgMaterialInstance.HasProperty(WobbleAmountId))
            _bgMaterialInstance.SetFloat(WobbleAmountId, ActiveBgWobbleAmount);

        if (_bgRect != null)
        {
            _bgRect.DOKill();
            _bgRect.DOSizeDelta(new Vector2(ActiveBgWidth, _bgRect.sizeDelta.y), BgWidthTweenDuration);
        }
    }

    private void RestoreBgWobble()
    {
        if (_bgMaterialInstance == null)
            return;

        if (_bgMaterialInstance.HasProperty(SpeedId))
            _bgMaterialInstance.SetFloat(SpeedId, _bgOriginalSpeed);

        if (_bgMaterialInstance.HasProperty(WobbleAmountId))
            _bgMaterialInstance.SetFloat(WobbleAmountId, _bgOriginalWobbleAmount);

        if (_bgRect != null)
        {
            _bgRect.DOKill();
            _bgRect.DOSizeDelta(new Vector2(_bgOriginalWidth, _bgRect.sizeDelta.y), BgWidthTweenDuration);
        }
    }

    private Sprite GetFinalYutSprite(bool isFront)
    {
        Sprite sprite = GetRandomSprite(isFront ? yutFrontSprites : yutBackSprites);
        return sprite != null ? sprite : GetRandomRevealSprite();
    }

    private Sprite GetRandomRevealSprite()
    {
        int frontCount = CountAssignedSprites(yutFrontSprites);
        int backCount = CountAssignedSprites(yutBackSprites);
        int totalCount = frontCount + backCount;

        if (totalCount == 0)
            return null;

        int targetIndex = UnityEngine.Random.Range(0, totalCount);
        Sprite sprite = GetAssignedSpriteAt(yutFrontSprites, targetIndex);
        if (sprite != null)
            return sprite;

        return GetAssignedSpriteAt(yutBackSprites, targetIndex - frontCount);
    }

    private static Sprite GetRandomSprite(Sprite[] sprites)
    {
        if (sprites == null || sprites.Length == 0)
            return null;

        int startIndex = UnityEngine.Random.Range(0, sprites.Length);
        for (int i = 0; i < sprites.Length; i++)
        {
            Sprite sprite = sprites[(startIndex + i) % sprites.Length];
            if (sprite != null)
                return sprite;
        }

        return null;
    }

    private static int CountAssignedSprites(Sprite[] sprites)
    {
        if (sprites == null)
            return 0;

        int count = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] != null)
                count++;
        }

        return count;
    }

    private static Sprite GetAssignedSpriteAt(Sprite[] sprites, int assignedIndex)
    {
        if (sprites == null || assignedIndex < 0)
            return null;

        int currentIndex = 0;
        for (int i = 0; i < sprites.Length; i++)
        {
            if (sprites[i] == null)
                continue;

            if (currentIndex == assignedIndex)
                return sprites[i];

            currentIndex++;
        }

        return null;
    }
}
