using System;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    private JoyConInputManager _inputManager;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            Init();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Init()
    {
        _inputManager = GetComponentInChildren<JoyConInputManager>();
        _inputManager.Init();
    }
    
    public IPlayerInputReader GetPlayerInputReader(int playerId)
        => _inputManager.GetPlayerInputReader(playerId);
}
