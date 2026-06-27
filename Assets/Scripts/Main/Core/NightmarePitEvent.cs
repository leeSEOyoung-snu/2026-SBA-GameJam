using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightmarePitEvent : IBoardEvent
{
    private const int DeltaMin = 5;
    private const int DeltaMax = 15;

    private readonly MainSceneManager _sceneManager;

    public NightmarePitEvent(MainSceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public IEnumerator Execute()
    {
        int delta = Random.Range(DeltaMin, DeltaMax + 1);

        _sceneManager.StateContainer.ApplyDeltaStats(new Dictionary<StateTypes, int> { { StateTypes.Nightmare, delta } });

        Debug.Log($"[NightmarePit] Nightmare +{delta}");

        yield return _sceneManager.RefreshStatesUI();
    }
}
