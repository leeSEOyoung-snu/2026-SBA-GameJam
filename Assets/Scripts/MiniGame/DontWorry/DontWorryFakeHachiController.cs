using System;
using UnityEngine;

// 가짜 해치 플레이어 컨트롤러
// MiniGameCharacterController + TopViewPhysics로 움직이고, 크로스헤어에 맞으면 제거됨
public class DontWorryFakeHachiController : MonoBehaviour
{
    public int PlayerId { get; private set; }

    private PlayableCharacterVisual _visual;
    private Action<DontWorryFakeHachiController> _onEliminated;

    private void Awake()
    {
        _visual = GetComponentInChildren<PlayableCharacterVisual>(true);
    }

    public void Init(int playerId, Action<DontWorryFakeHachiController> onEliminated)
    {
        PlayerId = playerId;
        _onEliminated = onEliminated;
    }

    public void Eliminate()
    {
        _visual.Effects.Spawn("Boom", transform.position);
        _onEliminated?.Invoke(this);
        Destroy(gameObject);
    }
}
