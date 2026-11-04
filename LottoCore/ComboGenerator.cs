namespace LottoCore;

/// <summary>이월수 + 후보군으로 6개 조합을 게임 수만큼 만든다.</summary>
public static class ComboGenerator
{
    private const int MaxAttempts = 2000; // 게임 하나당 시도 횟수

    public static List<int[]> Generate(AnalysisResult a, AnalysisOptions opt, Random rng)
    {
        int need = 6 - a.CarryOvers.Count;
        if (a.Candidates.Count < need)
        {
            throw new InvalidOperationException($"후보군이 {a.Candidates.Count}개라 조합을 만들 수 없다. 출현 횟수 범위를 넓혀라.");
        }

        var games = new List<int[]>();
        int[] used = new int[46];

        for (int g = 0; g < opt.GameCount; g++)
        {
            int[]? best = null;
            int bestScore = int.MaxValue;

            for (int t = 0; t < MaxAttempts; t++)
            {
                int[] combo = a.CarryOvers
                    .Concat(a.Candidates.OrderBy(_ => rng.Next()).Take(need))
                    .OrderBy(n => n)
                    .ToArray();

                if (LottoRules.IsExcluded(combo))
                {
                    continue;
                }

                if (games.Any(x => x.SequenceEqual(combo)))
                {
                    continue; // 완전히 같은 조합 제외
                }

                int score = combo.Sum(n => used[n]); // 앞 게임과 겹치는 정도
                if (score < bestScore)
                {
                    best = combo;
                    bestScore = score;
                }
            }

            if (best is null)
            {
                break; // 규칙을 만족하는 조합을 못 찾음
            }

            games.Add(best);
            foreach (int n in best)
            {
                used[n]++;
            }
        }
        return games;
    }
}
