namespace LottoCore;

/// <summary>조합 제외 규칙과 등수 판정.</summary>
public static class LottoRules
{
    /// <summary>정렬된 6개 번호가 제외 대상이면 true.</summary>
    public static bool IsExcluded(int[] sorted)
    {
        int run = 1;
        for (int i = 1; i < sorted.Length; i++)
        {
            run = sorted[i] == sorted[i - 1] + 1 ? run + 1 : 1;
            if (run >= 3)
            {
                return true; // 연속 번호 3개 이상
            }
        }

        if (sorted.GroupBy(n => n % 10).Any(g => g.Count() >= 3))
        {
            return true; // 같은 끝수
        }

        if (sorted.All(n => n <= 31))
        {
            return true;                             // 전부 31 이하
        }

        return false;
    }

    /// <summary>등수를 돌려준다. 0은 낙첨.</summary>
    public static int Rank(int[] combo, Draw draw)
    {
        int match = combo.Count(n => draw.Numbers.Contains(n));
        bool bonus = combo.Contains(draw.Bonus);

        return match switch
        {
            6 => 1,
            5 when bonus => 2,
            5 => 3,
            4 => 4,
            3 => 5,
            _ => 0
        };
    }
}
