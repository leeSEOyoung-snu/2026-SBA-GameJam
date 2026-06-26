using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HechiChaseGame : MonoBehaviour
{
    [SerializeField] private MiniGameCharacter hechiPrefab;
    [SerializeField] private MiniGameCharacter playerPrefab;
    [SerializeField] private Vector3[] playerInitPos; // 4개 필요
    [SerializeField] private Vector3 hechiInitPos; 
    [SerializeField] private float timeLimit = 60f;
    [SerializeField] private float moveSpeed = 5f;

    [Header("결과 델타 (해치 승리)")]
    [SerializeField] private int hechiWinNightmare = 10;

    [Header("결과 델타 (플레이어 승리)")]
    [SerializeField] private int playerWinCourage = 5;
    [SerializeField] private int playerWinLove    = 3;

    private readonly List<MiniGameCharacter> _normalPlayers = new();
    private bool _gameOver;

    private void Start()
    {
        SpawnCharacters();
        StartCoroutine(TimerRoutine());
    }

    private void SpawnCharacters()
    {
        int hechiPlayerId = Random.Range(1, 5); // 1~4 중 랜덤
        Debug.Log($"[HechiChase] 해치: Player {hechiPlayerId}");

        int playerCnt = 0;
        for (int i = 1; i <= 4; i++)
        {
            bool isHechi = i == hechiPlayerId;
            var prefab = isHechi ? hechiPrefab : playerPrefab;
            var pos = isHechi ? hechiInitPos : playerInitPos[playerCnt++];
            var character = Instantiate(prefab, pos, Quaternion.identity);
            character.Init(i, isHechi, moveSpeed, OnPlayerEliminated);

            if (!isHechi)
                _normalPlayers.Add(character);
        }
    }

    private void OnPlayerEliminated(MiniGameCharacter character)
    {
        _normalPlayers.Remove(character);
        Debug.Log($"[HechiChase] Player {character.PlayerId} 제거 / 남은 플레이어: {_normalPlayers.Count}");

        if (_normalPlayers.Count == 0)
            EndGame(hechiWins: true);
    }

    private IEnumerator TimerRoutine()
    {
        float remaining = timeLimit;

        while (remaining > 0f && !_gameOver)
        {
            remaining -= Time.deltaTime;
            yield return null;
        }

        if (!_gameOver)
            EndGame(hechiWins: false);
    }

    private void EndGame(bool hechiWins)
    {
        if (_gameOver) return;
        _gameOver = true;

        Dictionary<StateTypes, int> delta;

        if (hechiWins)
        {
            Debug.Log("[HechiChase] 해치 승리!");
            delta = new Dictionary<StateTypes, int>
            {
                { StateTypes.Nightmare, hechiWinNightmare }
            };
        }
        else
        {
            Debug.Log("[HechiChase] 플레이어 승리!");
            delta = new Dictionary<StateTypes, int>
            {
                { StateTypes.Courage, playerWinCourage },
                { StateTypes.Love,    playerWinLove    }
            };
        }

        GameManager.Instance.QuitMiniGame(delta);
    }
}
