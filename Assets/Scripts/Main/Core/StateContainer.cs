using System;
using System.Collections.Generic;
using UnityEngine;

public class StateContainer
{
    // 공통 스탯 (모든 플레이어 공유)
    public Dictionary<StateTypes, int> CommonStats { get; private set; }
    public Dictionary<int, int> AffectionById { get; private set; }

    // 플레이어별 호감도 (index 0 = Player 1)
    private const int PlayerCount = 4;
    
    public StateContainer()
    {
        CommonStats = new Dictionary<StateTypes, int>();
        foreach (StateTypes state in Enum.GetValues(typeof(StateTypes)))
            CommonStats[state] = 0;

        AffectionById = new() { { 1, 0 }, { 2, 0 }, { 3, 0 }, { 4, 0 } }; // 전부 0으로 초기화
    }
    
    // 공통 스탯 업데이트 + 플레이어 Id와 대응하는 스탯 delta를 affection에 반영
    // 플레이어 Id 1~4 ↔ StateTypes 값 1~4 (Courage/Wisdom/Recovery/Love)
    // Nightmare(0)은 공통 스탯에만 반영
    public void ApplyDeltaStats(Dictionary<StateTypes, int> deltaStates)
    {
        foreach (var (state, delta) in deltaStates)
        {
            // 1. 공통 스탯 합산
            CommonStats[state] += delta;

            // 2. StateTypes 값이 플레이어 Id(1~4)에 해당하면 해당 플레이어 affection에도 반영
            int stateValue = (int)state;
            if (stateValue >= 1 && stateValue <= PlayerCount)
                AffectionById[stateValue] += delta;
        }

        LogStats();
    }

    // CommonStats와 무관하게 순수 호감도만 직접 이전
    public void TransferAffection(int fromPlayerId, int toPlayerId, int amount)
    {
        AffectionById[fromPlayerId] -= amount;
        AffectionById[toPlayerId]   += amount;
        LogStats();
    }

    private void LogStats()
    {
        foreach (var (state, value) in CommonStats)
            Debug.Log($"[Stats] {state}: {value}");

        for (int i = 0; i < PlayerCount; i++)
            Debug.Log($"[Affection] Player {i + 1}: {AffectionById[i + 1]}");
    }

    public override string ToString()
    {
        string stats = "Common Stats:\n";
        foreach (var (state, value) in CommonStats)
            stats += $"{state}: {value}\n";

        stats += "Player Affections:\n";
        for (int i = 0; i < PlayerCount; i++)
            stats += $"Player {i + 1}: {AffectionById[i + 1]}\n";

        return stats;
    }
}
