using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainSceneManager : MonoBehaviour
{
    [SerializeField] private Board board;
    [SerializeField] private MainGameLoop gameLoop;
    [SerializeField] private GamePiece piece;
    
    private StateContainer _stateContainer;

    private void OnEnable()
    {
        GameManager.Instance.OnStateChanged += OnStateChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.OnStateChanged -= OnStateChanged;
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

    private void OnStateChanged(Dictionary<StateTypes, int> deltaStates)
    {
        _stateContainer.ApplyDeltaStats(deltaStates);
        Debug.Log(_stateContainer);
    }
}
