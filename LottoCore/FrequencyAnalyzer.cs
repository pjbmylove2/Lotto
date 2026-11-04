namespace LottoCore;

/// <summary>분석 결과. Counts는 인덱스 1~45를 번호로 쓴다(0번 칸은 비움).</summary>
public sealed record AnalysisResult(
    int FirstRound, int LastRound, int RecentCount,
    int[] Counts, List<int> Candidates, List<int> CarryOvers);

/// <summary>최근 N회 출현 횟수로 후보군과 이월수를 정한다.</summary>
public static class FrequencyAnalyzer
{
    public static AnalysisResult Analyze(IReadOnlyList<Draw> history, AnalysisOptions opt, Random rng)
    {
        if (history.Count == 0)
        {
            throw new InvalidOperationException("분석할 회차 데이터가 없다.");
        }

        List<Draw> recent = history.OrderBy(d => d.Round).TakeLast(opt.RecentRounds).ToList();

        int[] counts = new int[46];
        foreach (Draw d in recent)
        {
            foreach (int n in d.Numbers)
            {
                counts[n]++;
            }
        }

        Draw last = recent[^1]; // 마지막 요소
        List<int> carry = last.Numbers.OrderBy(_ => rng.Next())
                                      .Take(opt.CarryOverCount).ToList();

        List<int> candidates = Enumerable.Range(1, 45)
            .Where(n => counts[n] >= opt.MinCount && counts[n] <= opt.MaxCount)
            .Except(carry)
            .ToList();

        return new AnalysisResult(recent[0].Round, last.Round, recent.Count,
                                  counts, candidates, carry);
    }
}
