using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class PhotoManager : SoloBattleBase
{
    [Header("Game Settings")]
    [SerializeField] private int totalRounds = 10;
    [SerializeField] private int failThreshold = 20;
    [SerializeField] private float minWaitTime = 0.8f;
    [SerializeField] private float maxWaitTime = 2f;

    [Header("References")]
    [SerializeField] private PhotoHechi hechi;
    [SerializeField] private BasicMiniGameCanvas basicMiniGameCanvas;
    [SerializeField] private BasicPlayerCanvasManager basicPlayerCanvasManager;
    [SerializeField] private TMP_Text nightmareCountTxt;
    // 플레이어 1~4 대표 이미지 (인스펙터에서 순서대로 연결)
    [SerializeField] private Transform[] playerIcons = new Transform[4];

    [Header("Shutter VFX")]
    [SerializeField] private float punchScale = 3.6f;
    [SerializeField] private float punchDuration = 0.08f;
    [SerializeField] private float shrinkDuration = 0.15f;
    [SerializeField] private float cameraFlashDuration = 0.2f;

    [Space(10)] [SerializeField] private int nightmareDelta = 5;

    public override int NightmareDelta { get; protected set; } = 0;
    public override int RankPlayer1 { get; protected set; }
    public override int RankPlayer2 { get; protected set; }
    public override int RankPlayer3 { get; protected set; }
    public override int RankPlayer4 { get; protected set; }

    private IPlayerInputReader[] _inputs = new IPlayerInputReader[4];
    private int[] _successCounts = new int[4];
    private int[] _failCounts = new int[4];
    private Transform[] _cameraFlashes = new Transform[4];
    private Coroutine[] _cameraFlashCoroutines = new Coroutine[4];
    private bool[] _cameraFlashParentOriginalActive = new bool[4];
    private bool[] _hasActiveCameraFlash = new bool[4];

    private int _currentRound;
    private bool _hechiVisible;
    private bool _gameOver;

    private void Start()
    {
        for (int i = 0; i < 4; i++)
            _inputs[i] = GameManager.Instance.GetPlayerInputReader(i + 1);

        hechi.OnExited += OnHechiExited;
        hechi.gameObject.SetActive(false);
        if (basicMiniGameCanvas == null)
            basicMiniGameCanvas = FindAnyObjectByType<BasicMiniGameCanvas>(FindObjectsInactive.Include);
        if (basicPlayerCanvasManager == null)
            basicPlayerCanvasManager = FindAnyObjectByType<BasicPlayerCanvasManager>(FindObjectsInactive.Include);
        UpdateNightmareCount();
        UpdateHechiPassCount(0);

        StartCoroutine(InputRoutine());
        StartCoroutine(GameRoutine());
    }

    private IEnumerator InputRoutine()
    {
        while (!_gameOver)
        {
            for (int i = 0; i < 4; i++)
            {
                if (_inputs[i] != null && _inputs[i].Right)
                    OnPlayerShoot(i);
            }
            yield return null;
        }
    }

    private IEnumerator GameRoutine()
    {
        yield return new WaitForSecondsRealtime(6f);
        
        while (_currentRound < totalRounds && !_gameOver)
        {
            float wait = Random.Range(minWaitTime, maxWaitTime);
            yield return new WaitForSeconds(wait);

            _currentRound++;
            _hechiVisible = true;
            hechi.Launch();
            
            yield return new WaitUntil(() => !_hechiVisible || _gameOver);
        }

        EndGame();
    }

    private void OnPlayerShoot(int playerIndex)
    {
        PlayShutterVFX(playerIndex);

        if (hechi.IsInPhotoZone)
        {
            _successCounts[playerIndex]++;
            UpdatePlayerStackCount(playerIndex);
            PlayCameraFlash(playerIndex);
            Debug.Log($"[Player {playerIndex}]  success count: {_successCounts[playerIndex]}");
        }
        else
        {
            _failCounts[playerIndex]++;
            UpdateNightmareCount();
            Debug.Log($"[Player {playerIndex}]  fail count: {_failCounts[playerIndex]}");
        }
    }

    private void PlayShutterVFX(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= playerIcons.Length) return;
        Transform icon = playerIcons[playerIndex];
        if (icon == null) return;

        icon.DOKill();
        icon.localScale = Vector3.one * 3f;
        icon.DOScale(punchScale, punchDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => icon.DOScale(3f, shrinkDuration).SetEase(Ease.InQuad));
    }

    private void UpdatePlayerStackCount(int playerIndex)
    {
        if (basicPlayerCanvasManager == null)
            basicPlayerCanvasManager = FindAnyObjectByType<BasicPlayerCanvasManager>(FindObjectsInactive.Include);
        if (basicPlayerCanvasManager == null) return;

        basicPlayerCanvasManager.UpdateStackCnt(playerIndex + 1, _successCounts[playerIndex]);
    }

    private void UpdateNightmareCount()
    {
        if (nightmareCountTxt == null)
            nightmareCountTxt = FindNightmareCountText();
        if (nightmareCountTxt == null) return;

        int currentFailCount = Mathf.Clamp(_failCounts.Sum(), 0, failThreshold);
        nightmareCountTxt.text = $"{currentFailCount}/{failThreshold}";
    }

    private TMP_Text FindNightmareCountText()
    {
        TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (TMP_Text text in texts)
        {
            if (text.name != "Count")
                continue;

            Transform current = text.transform.parent;
            while (current != null)
            {
                if (current.name.Contains("Nightmare") || current.name.Contains("Nigmare"))
                    return text;

                current = current.parent;
            }
        }

        return null;
    }

    private void PlayCameraFlash(int playerIndex)
    {
        if (playerIndex < 0 || playerIndex >= _cameraFlashes.Length) return;

        Transform flash = GetCameraFlash(playerIndex);
        if (flash == null) return;

        if (_cameraFlashCoroutines[playerIndex] != null)
        {
            StopCoroutine(_cameraFlashCoroutines[playerIndex]);
            flash.gameObject.SetActive(false);
        }

        Transform flashParent = flash.parent;
        if (!_hasActiveCameraFlash[playerIndex])
        {
            _cameraFlashParentOriginalActive[playerIndex] =
                flashParent == null || flashParent.gameObject.activeSelf;
        }

        _cameraFlashCoroutines[playerIndex] = StartCoroutine(CameraFlashRoutine(playerIndex, flash));
    }

    private IEnumerator CameraFlashRoutine(int playerIndex, Transform flash)
    {
        Transform flashParent = flash.parent;
        _hasActiveCameraFlash[playerIndex] = true;

        if (flashParent != null)
            flashParent.gameObject.SetActive(true);

        flash.gameObject.SetActive(true);
        yield return new WaitForSeconds(cameraFlashDuration);
        flash.gameObject.SetActive(false);

        if (flashParent != null && !_cameraFlashParentOriginalActive[playerIndex])
            flashParent.gameObject.SetActive(false);

        _hasActiveCameraFlash[playerIndex] = false;
        _cameraFlashCoroutines[playerIndex] = null;
    }

    private Transform GetCameraFlash(int playerIndex)
    {
        Transform cachedFlash = _cameraFlashes[playerIndex];
        if (cachedFlash != null)
            return cachedFlash;

        CacheCameraFlashes();
        return _cameraFlashes[playerIndex];
    }

    private void CacheCameraFlashes()
    {
        if (basicPlayerCanvasManager == null)
            basicPlayerCanvasManager = FindAnyObjectByType<BasicPlayerCanvasManager>(FindObjectsInactive.Include);
        if (basicPlayerCanvasManager == null)
            return;

        Transform[] children = basicPlayerCanvasManager.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name != "Camera Flash")
                continue;

            int playerIndex = FindPlayerIndex(child);
            if (playerIndex >= 0 && playerIndex < _cameraFlashes.Length)
                _cameraFlashes[playerIndex] = child;
        }
    }

    private int FindPlayerIndex(Transform target)
    {
        Transform current = target;
        while (current != null)
        {
            string objectName = current.name;
            if (objectName.Length >= 2 &&
                objectName.EndsWith("P") &&
                int.TryParse(objectName[0].ToString(), out int playerId) &&
                playerId >= 1 &&
                playerId <= 4)
            {
                return playerId - 1;
            }

            current = current.parent;
        }

        return -1;
    }

    private void OnHechiExited()
    {
        _hechiVisible = false;
        UpdateHechiPassCount(_currentRound);
    }

    private void UpdateHechiPassCount(int passedCount)
    {
        if (basicMiniGameCanvas == null)
            return;

        basicMiniGameCanvas.SetCount(passedCount, totalRounds);
    }

    private void EndGame()
    {
        if (_gameOver) return;
        _gameOver = true;

        bool reachedFailThreshold = _failCounts.Sum() >= failThreshold;

        AssignRanks(reachedFailThreshold);
        MiniGameManager.Instance.QuitMiniGame();
    }

    private void AssignRanks(bool allFailedThreshold)
    {
        // 성공 수 기준 내림차순 정렬 → 순위 결정
        var ranked = Enumerable.Range(0, 4)
            .OrderByDescending(i => _successCounts[i])
            .ThenBy(i => _failCounts[i])
            .ToList();

        int[] ranks = new int[4];
        for (int r = 0; r < ranked.Count; r++)
            ranks[ranked[r]] = r + 1;

        RankPlayer1 = ranks[0];
        RankPlayer2 = ranks[1];
        RankPlayer3 = ranks[2];
        RankPlayer4 = ranks[3];

        // allFailedThreshold 조건에 따른 NightmareDelta 처리 (필요 시 확장)
        NightmareDelta = allFailedThreshold ? nightmareDelta : 0;
    }
}
