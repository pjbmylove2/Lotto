namespace LottoCore;

/// <summary>분석·생성·수집에 쓰는 설정값 모음.</summary>
public sealed class AnalysisOptions
{
    public int RecentRounds { get; set; } = 26;   // 분석할 최근 회차 수
    public int MinCount { get; set; } = 3;        // 후보군 출현 횟수 하한
    public int MaxCount { get; set; } = 4;        // 후보군 출현 횟수 상한
    public int CarryOverCount { get; set; } = 1;  // 이월수 개수
    public int GameCount { get; set; } = 5;       // 만들 게임 수
    public int? Seed { get; set; }                // 난수 시드(같은 결과 재현용, null이면 매번 다름)
    public int RequestDelayMs { get; set; } = 10; // 요청 간 대기 시간(ms)
    public string CachePath { get; set; } = "draws.json";
}
