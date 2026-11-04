namespace LottoCore;

/// <summary>백테스트 결과. Hits 배열은 인덱스가 등수(0은 낙첨).</summary>
public sealed record BacktestResult(int Rounds, int TotalGames, int[] StrategyHits, int[] RandomHits);

/// <summary>과거 회차 시점마다 같은 방법으로 조합을 만들어 실제 결과와 비교한다.</summary>
public static class Backtester
{
    public static BacktestResult Run(IReadOnlyList<Draw> draws, AnalysisOptions opt,
                                     int testRounds, CancellationToken ct)
    {
        List<Draw> sorted = draws.OrderBy(d => d.Round).ToList();
        Random rng = opt.Seed is int s ? new Random(s) : new Random();

        int[] strategyHits = new int[6];
        int[] randomHits = new int[6];
        int rounds = 0, totalGames = 0;

        int start = Math.Max(opt.RecentRounds, sorted.Count - testRounds);
        for (int i = start; i < sorted.Count; i++)
        {
            ct.ThrowIfCancellationRequested();

            List<Draw> history = sorted.GetRange(0, i); // i번째 회차 직전까지만
            Draw target = sorted[i];

            List<int[]> games;
            try
            {
                AnalysisResult a = FrequencyAnalyzer.Analyze(history, opt, rng);
                games = ComboGenerator.Generate(a, opt, rng);
            }
            catch (InvalidOperationException)
            {
                continue; // 후보군 부족 등으로 조합을 못 만든 회차는 건너뜀
            }

            foreach (int[] g in games)
            {
                strategyHits[LottoRules.Rank(g, target)]++;
                randomHits[LottoRules.Rank(RandomCombo(rng), target)]++; // 같은 개수만큼
            }
            rounds++;
            totalGames += games.Count;
        }
        return new BacktestResult(rounds, totalGames, strategyHits, randomHits);
    }

    private static int[] RandomCombo(Random rng) =>
        Enumerable.Range(1, 45).OrderBy(_ => rng.Next()).Take(6).OrderBy(n => n).ToArray();
}
