public enum YutType
{
    Do   = 1, // 도 - 1칸
    Gae  = 2, // 개 - 2칸
    Geol = 3, // 걸 - 3칸
    Yut  = 4, // 윷 - 4칸
    Mo   = 5  // 모 - 5칸
}

public static class YutCalculator
{
    // results[i] = true(앞면) / false(뒷면)
    // 앞면 개수로 도/개/걸/윷 결정, 0개면 모
    public static YutType Calculate(bool[] results)
    {
        int frontCount = 0;
        foreach (var r in results)
            if (r) frontCount++;

        return frontCount switch
        {
            0 => YutType.Mo,
            1 => YutType.Do,
            2 => YutType.Gae,
            3 => YutType.Geol,
            _ => YutType.Yut
        };
    }

    public static int ToMoveCount(YutType yut) => (int)yut;
}
