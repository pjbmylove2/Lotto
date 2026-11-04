using System.Text;

namespace LottoCore;

/// <summary>분석·조합 결과를 텍스트와 CSV 문자열로 만든다.</summary>
public static class ReportBuilder
{
    public static string BuildText(AnalysisResult a, IReadOnlyList<int[]> games)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[번호별 출현 횟수] 최근 {a.RecentCount}회 ({a.FirstRound}~{a.LastRound}회차)");
        for (int n = 1; n <= 45; n++)
        {
            sb.Append($"{n,2}번:{a.Counts[n],2}  ");
            if (n % 9 == 0)
            {
                sb.AppendLine(); // 한 줄에 9개씩
            }
        }
        sb.AppendLine();
        sb.AppendLine($"[후보군] {string.Join(", ", a.Candidates)}");
        sb.AppendLine($"[이월수] {string.Join(", ", a.CarryOvers)}");
        sb.AppendLine($"[최종 조합] {a.LastRound + 1}회차 대상");
        for (int i = 0; i < games.Count; i++)
        {
            sb.AppendLine($"{i + 1}게임: {string.Join(" ", games[i].Select(n => n.ToString("00")))}");
        }

        return sb.ToString();
    }

    public static string BuildCsv(AnalysisResult a, IReadOnlyList<int[]> games)
    {
        var sb = new StringBuilder();
        sb.AppendLine("대상회차,게임,번호1,번호2,번호3,번호4,번호5,번호6");
        for (int i = 0; i < games.Count; i++)
        {
            sb.AppendLine($"{a.LastRound + 1},{i + 1},{string.Join(",", games[i])}");
        }

        return sb.ToString();
    }
}
