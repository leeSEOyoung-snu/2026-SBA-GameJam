using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;

public class JoyConInputManager : MonoBehaviour
{
    [SerializeField] private List<SwingDetector> swingDetectors;
    [SerializeField] private List<PlayerInputProcessor> players;
    
    private JoyConMotion _joyConMotion;

    public void Init()
    {
        if (swingDetectors != null)
            swingDetectors.ForEach(s => s.OnSwing += HandleSwing);
        
        _joyConMotion = GetComponent<JoyConMotion>();
        _joyConMotion.Init();
        
        players.ForEach(s => s.Init());
    }

    private void OnDisable()
    {
        if (swingDetectors != null)
            swingDetectors.ForEach(s => s.OnSwing -= HandleSwing);
    }

    public IPlayerInputReader GetPlayerInputReader(int playerId)
    {
        return players[playerId-1];
    }

    public void SetActiveAllInput(bool active)
        => players.ForEach(p => p.SetActiveInput(active));

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
}
