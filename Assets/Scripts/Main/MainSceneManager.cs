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
        gameLoop.Init(board, piece);
        gameLoop.OnGameEnd += HandleGameEnd;
        _stateContainer = new StateContainer();
    }

    private void HandleGameEnd()
    {
        Debug.Log("[MainSceneManager] 게임 종료!");
        // TODO: 엔딩 로직 연결
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

        // TODO: 절망 어떻게 처리하죠
        // 동점자 처리
        if (possibleHighestStateTypes.Count > 1)
        {
            if (possibleHighestStateTypes.Contains(StateTypes.Nightmare))
                _resolvedHighestState = StateTypes.Nightmare;
            else
            {
                yield return evolutionDecidingCanvasManager.DecideEvolutionStateType(possibleHighestStateTypes);
                _resolvedHighestState = evolutionDecidingCanvasManager.DecidedStateType;
            }
        }
        else
        {
            _resolvedHighestState = possibleHighestStateTypes[0];
        }
    }
}
