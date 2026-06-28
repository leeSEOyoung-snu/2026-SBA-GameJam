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

    public StateTypes Target { get; }
    public int Delta { get; }

    public StateChangeEvent(MainSceneManager sceneManager)
    {
        _sceneManager = sceneManager;
        Target = EligibleStates[Random.Range(0, EligibleStates.Length)];
        Delta = Random.Range(DeltaMin, DeltaMax + 1);
    }

    public IEnumerator Execute()
    {
        _sceneManager.StateContainer.ApplyDeltaStats(new Dictionary<StateTypes, int> { { Target, Delta } });

        Debug.Log($"[StateChange] {Target} {(Delta >= 0 ? "+" : "")}{Delta}");

        yield return _sceneManager.RefreshStatesUI();
        yield return _sceneManager.RefreshAffectionUI();
    }
}
