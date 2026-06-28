using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightmarePitEvent : IBoardEvent
{
    private const int DeltaMin = 5;
    private const int DeltaMax = 15;

    private readonly MainSceneManager _sceneManager;

    public StateTypes Target => StateTypes.Nightmare;
    public int Delta { get; }

    public NightmarePitEvent(MainSceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        Delta = Random.Range(DeltaMin, DeltaMax + 1);
    }

    public IEnumerator Execute()
    {
        _sceneManager.StateContainer.ApplyDeltaStats(new Dictionary<StateTypes, int> { { Target, Delta } });

        Debug.Log($"[NightmarePit] Nightmare +{Delta}");

        yield return _sceneManager.RefreshStatesUI();
    }
}
