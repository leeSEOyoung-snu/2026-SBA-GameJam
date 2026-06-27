using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EvolutionDecidingCanvasManager : MonoBehaviour
{
    private enum RpsChoice { None = -1, Scissors = 0, Rock = 1, Paper = 2 }

    public StateTypes DecidedStateType { get; private set; }

    private void Awake()
    {
        gameObject.SetActive(false);
    }

    public IEnumerator DecideEvolutionStateType(List<StateTypes> candidates)
    {
        gameObject.SetActive(true);

        // candidates의 StateTypes 값 == 플레이어 Id (1~4)
        List<int> playerIds = candidates.Select(s => (int)s).ToList();

        int winnerId = -1;
        while (winnerId == -1)
            yield return StartCoroutine(RunRpsRound(playerIds, result => { playerIds = result; }));

        // 남은 단 한 명이 승자
        winnerId = playerIds[0];
        DecidedStateType = (StateTypes)winnerId;

        gameObject.SetActive(false);
    }

    // 한 라운드 진행. 무승부면 무승부 플레이어 Id 목록을, 단독 승자면 그 Id만 담아 콜백
    private IEnumerator RunRpsRound(List<int> participantIds, System.Action<List<int>> onRoundEnd)
    {
        // TODO: 가위바위보 UI 연결
        
        var inputs = new IPlayerInputReader[5]; // index 1~4 사용
        for (int i = 1; i <= 4; i++)
            inputs[i] = GameManager.Instance.GetPlayerInputReader(i);

        var choices = new Dictionary<int, RpsChoice>();
        foreach (int id in participantIds)
            choices[id] = RpsChoice.None;

        // 모든 참가자가 선택할 때까지 대기
        while (choices.Values.Any(c => c == RpsChoice.None))
        {
            foreach (int id in participantIds)
            {
                if (choices[id] != RpsChoice.None) continue;
                var input = inputs[id];
                if (input.Left)  choices[id] = RpsChoice.Scissors;
                else if (input.Up)    choices[id] = RpsChoice.Rock;
                else if (input.Right) choices[id] = RpsChoice.Paper;
            }
            yield return null;
        }

        // 로그
        foreach (var (id, choice) in choices)
            Debug.Log($"[RPS] Player {id} → {choice}");

        onRoundEnd(ResolveRps(choices));
    }

    // 가위바위보 판정. 단독 승자면 그 Id만, 무승부면 무승부 참가자 Id 반환
    private static List<int> ResolveRps(Dictionary<int, RpsChoice> choices)
    {
        var usedChoices = choices.Values.Distinct().ToList();

        // 전원 같은 선택 → 전원 무승부
        if (usedChoices.Count == 1)
            return choices.Keys.ToList();

        // 세 가지 선택이 모두 나왔으면 → 전원 무승부 (삼파전)
        if (usedChoices.Count == 3)
            return choices.Keys.ToList();

        // 두 종류만 나온 경우: 이기는 쪽 판별
        RpsChoice a = usedChoices[0], b = usedChoices[1];
        RpsChoice winner = Beats(a, b) ? a : b;

        var winners = choices.Where(kv => kv.Value == winner).Select(kv => kv.Key).ToList();
        return winners;
    }

    // a가 b를 이기면 true
    private static bool Beats(RpsChoice a, RpsChoice b)
    {
        return (a == RpsChoice.Rock     && b == RpsChoice.Scissors) ||
               (a == RpsChoice.Scissors && b == RpsChoice.Paper)    ||
               (a == RpsChoice.Paper    && b == RpsChoice.Rock);
    }
}
