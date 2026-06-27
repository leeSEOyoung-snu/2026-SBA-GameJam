using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StateChangeEvent : IBoardEvent
{
    private const int DeltaMin = -10;
    private const int DeltaMax = 10;

    private static readonly StateTypes[] EligibleStates =
    {
        StateTypes.Courage,
        StateTypes.Wisdom,
        StateTypes.Recovery,
        StateTypes.Love,
    };

    private readonly MainSceneManager _sceneManager;

    public StateChangeEvent(MainSceneManager sceneManager)
    {
        _sceneManager = sceneManager;
    }

    public IEnumerator Execute()
    {
        StateTypes target = EligibleStates[Random.Range(0, EligibleStates.Length)];
        int delta = Random.Range(DeltaMin, DeltaMax + 1);

        _sceneManager.StateContainer.ApplyDeltaStats(new Dictionary<StateTypes, int> { { target, delta } });

        Debug.Log($"[StateChange] {target} {(delta >= 0 ? "+" : "")}{delta}");

        yield return _sceneManager.RefreshStatesUI();
        yield return _sceneManager.RefreshAffectionUI();
    }
}
