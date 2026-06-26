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
        _stateContainer = new StateContainer();
    }

    private void OnStateChanged(Dictionary<StateTypes, int> deltaStates)
    {
        _stateContainer.ApplyDeltaStats(deltaStates);
        Debug.Log(_stateContainer);
    }
}
