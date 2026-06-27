using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private MainGameLoop gameLoop;
    [SerializeField] private GamePiece piece;
    [Space(10)]
    [SerializeField] private HatchUiCanvasManager hatchCanvasManager;
    [SerializeField] private PlayerUiCanvasManager playerCanvasManager;
    
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
}
