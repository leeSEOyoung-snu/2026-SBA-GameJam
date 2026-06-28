using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private MainGameLoop gameLoop;
    [SerializeField] private GamePiece piece;
    [Space(10)]
    [SerializeField] private HatchUiCanvasManager hatchCanvasManager;
    [SerializeField] private PlayerUiCanvasManager playerCanvasManager;
    [SerializeField] private EvolutionDecidingCanvasManager evolutionDecidingCanvasManager;
    [SerializeField] private VictoryCanvasManager victoryCanvasManager;

    [Header("오디오")]
    [SerializeField] private AudioManager audioManager;
    [SerializeField] private AudioClip bgmClip;

    public int[] PlayerIdByRanking
    {
        get
        {
            var affectionById = _stateContainer.AffectionById;
            return affectionById
                .OrderByDescending(kv => kv.Value)
                .Select(kv => kv.Key)
                .ToArray();
        }
    }
    
    public StateContainer StateContainer => _stateContainer;
    public int[] WinnerPlayerIds { get; private set; } = Array.Empty<int>();
    public int WinningAffection { get; private set; }

    private StateContainer _stateContainer;

    private void OnEnable()
    {
        GameManager.Instance.OnMiniGameQuited += OnMiniGameQuited;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnMiniGameQuited -= OnMiniGameQuited;
    }

    private void Start()
    {
        if (victoryCanvasManager == null)
            victoryCanvasManager = FindAnyObjectByType<VictoryCanvasManager>(FindObjectsInactive.Include);

        gameLoop.Init(board, piece);
        gameLoop.OnGameEnd += HandleGameEnd;
        _stateContainer = new StateContainer();

        if (audioManager != null && bgmClip != null)
            audioManager.PlayBgm(bgmClip);
    }

    private void HandleGameEnd()
    {
        Debug.Log("[MainSceneManager] 게임 종료!");
        SettleWinners();
        victoryCanvasManager?.PlayVictory(WinnerPlayerIds, WinningAffection);
    }
    
    

    private void SettleWinners()
    {
        var affectionById = _stateContainer.AffectionById;
        WinningAffection = affectionById.Values.Max();
        WinnerPlayerIds = affectionById
            .Where(kv => kv.Value == WinningAffection)
            .OrderBy(kv => kv.Key)
            .Select(kv => kv.Key)
            .ToArray();

        string winners = string.Join(", ", WinnerPlayerIds.Select(id => $"{id}P"));
        string ranking = string.Join(" / ", affectionById
            .OrderByDescending(kv => kv.Value)
            .ThenBy(kv => kv.Key)
            .Select(kv => $"{kv.Key}P:{kv.Value}"));

        Debug.Log($"[MainSceneManager] 우승자: {winners} (호감도 {WinningAffection})");
        Debug.Log($"[MainSceneManager] 최종 순위: {ranking}");
    }

    private void OnMiniGameQuited(Dictionary<StateTypes, int> deltaStates)
    {
        _stateContainer.ApplyDeltaStats(deltaStates);
        //Debug.Log(_stateContainer);
        
        StartCoroutine(OnMiniGameQuitedCoroutine());
    }

    private IEnumerator OnMiniGameQuitedCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        yield return hatchCanvasManager.UpdateStates(_stateContainer.CommonStats);
        yield return new WaitForSeconds(0.2f);
        yield return playerCanvasManager.UpdateAffection(_stateContainer.AffectionById);
        
        GameManager.Instance.SetActiveAllInput(true);
        GameManager.Instance.IsMiniGameRunning = false;
    }
    
    public IEnumerator RefreshStatesUI()
    {
        yield return hatchCanvasManager.UpdateStates(_stateContainer.CommonStats);
    }

    public IEnumerator RefreshAffectionUI()
    {
        yield return playerCanvasManager.UpdateAffection(_stateContainer.AffectionById);
    }

    public IEnumerator GoGoEvolution()
    {
        yield return GetHighestState();
        StateTypes highestState = _resolvedHighestState;

        var hechiInfo = GameManager.Instance.OnEvolution(highestState);

        yield return hatchCanvasManager.EvolutionCoroutine(hechiInfo.sprite, hechiInfo.name);
    }

    private StateTypes _resolvedHighestState;

    public IEnumerator GetHighestState()
    {
        var currStates = _stateContainer.CommonStats;

        List<StateTypes> possibleHighestStateTypes = new();
        int possibleHighestState = -99;

        foreach (var state in currStates)
        {
            if (state.Value == possibleHighestState)
            {
                possibleHighestStateTypes.Add(state.Key);
            }
            else if (state.Value > possibleHighestState)
            {
                possibleHighestStateTypes.Clear();
                possibleHighestStateTypes.Add(state.Key);
                possibleHighestState = state.Value;
            }
        }

        // 동점자 처리
        if (possibleHighestStateTypes.Count > 1)
        {
            yield return evolutionDecidingCanvasManager.DecideEvolutionStateType(possibleHighestStateTypes);
            _resolvedHighestState = evolutionDecidingCanvasManager.DecidedStateType;
        }
        else
        {
            _resolvedHighestState = possibleHighestStateTypes[0];
        }
    }
}
