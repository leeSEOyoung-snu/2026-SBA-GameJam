using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoyConInputManager : MonoBehaviour
{
    public static JoyConInputManager Instance { get; private set; }
    
    [SerializeField] private List<SwingDetector> swingDetectors;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private List<PlayerInputProcessor> players;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else 
            Destroy(this);
    }

    private void OnEnable()
    {
        if (swingDetectors != null)
            swingDetectors.ForEach(s => s.OnSwing += HandleSwing);
    }

    private void OnDisable()
    {
        if (swingDetectors != null)
            swingDetectors.ForEach(s => s.OnSwing -= HandleSwing);
    }

    public IPlayerInputReader GetPlayerInputReader(int playerId)
    {
        return players[playerId];
    }

    private void HandleSwing(int playerId, char side, float magnitude)
    {
        //Debug.Log($"[JoyConInputProcessor {playerId}P - {side}] 스윙 감지! 세기={magnitude:F2}g");
        if (playerId == 1)
        {
            if (side == 'L')
                players[0].SwingDetected();
            else if (side == 'R')
                players[1].SwingDetected();
        }
        else if (playerId == 2)
        {
            if (side == 'L')
                players[2].SwingDetected();
            else if (side == 'R')
                players[3].SwingDetected();
        }
    }

    private void Update()
    {
        //Debug.Log(joyStick.action.ReadValue<Vector2>());
    }
    
    void Start()
    {
        // var joysticks = Joystick.all;
        // if (joysticks.Count < 2)
        // {
        //     Debug.LogWarning("조이스틱 2개 필요");
        //     return;
        // }

        // List<PlayerInput> players = new();
        // players.Add(PlayerInput.Instantiate(playerPrefab, controlScheme: "Joystick", pairWithDevice: joysticks[0])); // 1P
        // players.Add(PlayerInput.Instantiate(playerPrefab, controlScheme: "Joystick", pairWithDevice: joysticks[0])); // 2P
        // players.Add(PlayerInput.Instantiate(playerPrefab, controlScheme: "Joystick", pairWithDevice: joysticks[1])); // 3P
        // players.Add(PlayerInput.Instantiate(playerPrefab, controlScheme: "Joystick", pairWithDevice: joysticks[1])); // 4P
        //
        // foreach (PlayerInput player in players)
        // {
        //     player.transform.SetParent(transform);
        //     _playerInputs.Add(player.GetComponent<PlayerInputProcessor>());
        // }
    }
}
