using System;
using System.Collections.Generic;
using System.Linq;

public readonly struct MiniGameTutorialContent
{
    public readonly string GameTypeText;
    public readonly string GameTitleText;
    public readonly string LeftPlayerText;
    public readonly string RightPlayerText;
    public readonly string LeftConditionText;
    public readonly string RightConditionText;

    public MiniGameTutorialContent(
        string gameTypeText,
        string gameTitleText,
        string leftPlayerText,
        string rightPlayerText,
        string leftConditionText,
        string rightConditionText)
    {
        GameTypeText = gameTypeText;
        GameTitleText = gameTitleText;
        LeftPlayerText = leftPlayerText;
        RightPlayerText = rightPlayerText;
        LeftConditionText = leftConditionText;
        RightConditionText = rightConditionText;
    }
}

public static class MiniGameTutorialContentConverter
{
    public static MiniGameTutorialContent Convert(MiniGameResultContainer data, MiniGameProcessorBase processor)
    {
        return data.Type switch
        {
            MiniGameTypes.SoloBattle => ConvertSoloBattle(data),
            MiniGameTypes.OneVsThree => ConvertOneVsThree(data, (OneVsThreeBase)processor),
            MiniGameTypes.TwoVsTwo => ConvertTwoVsTwo(data, (TwoVsTwoBase)processor),
            MiniGameTypes.Cooperative => ConvertCooperative(data),
            _ => throw new NotSupportedException($"Tutorial content is not supported for {data.Type}."),
        };
    }
    
    private static MiniGameTutorialContent ConvertSoloBattle(MiniGameResultContainer data)
    {
        MiniGameResultContainer.SoloBattleTutorialText text = data.SoloBattleTutorial;
        return new MiniGameTutorialContent(
            "개인전",
            FormatTitle(data.GameTitle),
            null,
            null,
            text.soloWinCondition,
            text.nightmareCondition);
    }

    private static MiniGameTutorialContent ConvertOneVsThree(MiniGameResultContainer data, OneVsThreeBase oneVsThree)
    {
        MiniGameResultContainer.OneVsThreeTutorialText text = data.OneVsThreeTutorial;
        int onePlayerId = oneVsThree.OnePlayerId;
        List<int> threePlayerIds = new() { 1, 2, 3, 4 };
        threePlayerIds.Remove(onePlayerId);

        return new MiniGameTutorialContent(
            "1vs3",
            FormatTitle(data.GameTitle),
            FormatPlayers(onePlayerId),
            FormatPlayers(threePlayerIds),
            text.oneWinCondition,
            text.threeWinCondition);
    }

    private static MiniGameTutorialContent ConvertTwoVsTwo(MiniGameResultContainer data, TwoVsTwoBase twoVsTwo)
    {
        MiniGameResultContainer.TwoVsTwoTutorialText text = data.TwoVsTwoTutorial;
        return new MiniGameTutorialContent(
            "2vs2",
            FormatTitle(data.GameTitle),
            $"{FormatPlayers(twoVsTwo.PlayerIdsTeam1)} VS. {FormatPlayers(twoVsTwo.PlayerIdsTeam2)}",
            "악몽 조건",
            text.twoWinCondition,
            text.nightmareCondition);
    }

    private static MiniGameTutorialContent ConvertCooperative(MiniGameResultContainer data)
    {
        MiniGameResultContainer.CooperativeTutorialText text = data.CooperativeTutorial;
        return new MiniGameTutorialContent(
            "협동전",
            FormatTitle(data.GameTitle),
            null,
            null,
            text.coopWinCondition,
            text.nightmareCondition);
    }

    private static string FormatTitle(string title)
    {
        return $"<wiggle>{title}</wiggle>";
    }

    private static string FormatPlayers(int playerId)
    {
        return $"{playerId}P";
    }

    private static string FormatPlayers(IEnumerable<int> playerIds)
    {
        return string.Join(",", playerIds.Select(FormatPlayers));
    }
}
