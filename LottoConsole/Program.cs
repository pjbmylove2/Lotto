using System.Text;
using LottoCore;

Console.OutputEncoding = Encoding.UTF8;

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true; // 프로그램 강제 종료 대신
    cts.Cancel();    // 취소 신호 전달
};

var opt = new AnalysisOptions();
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(10) };
http.DefaultRequestHeaders.UserAgent.ParseAdd("LottoStudy/1.0");

var api = new DhLotteryApiSource(http);
var cache = new DrawCache(opt.CachePath);
var repo = new DrawRepository(cache, api, opt);

try
{
    // 1. 데이터 수집
    List<Draw> draws;
    if (await api.IsAvailableAsync(cts.Token))
    {
        Console.WriteLine("동행복권 조회 사용 가능. 새 회차를 받는다.");
        var progress = new Progress<int>(r => Console.Write($"\r{r}회차 수신"));
        draws = await repo.UpdateAsync(progress, cts.Token);
        Console.WriteLine();
    }
    else if (File.Exists("draws.csv"))
    {
        Console.WriteLine("조회 실패. draws.csv를 읽는다.");
        draws = await repo.ImportCsvAsync("draws.csv", cts.Token);
    }
    else
    {
        Console.WriteLine("조회 실패, CSV 없음. 기존 캐시만 사용한다.");
        draws = await cache.LoadAsync(cts.Token);
    }

    if (draws.Count < opt.RecentRounds)
    {
        Console.WriteLine($"데이터가 {draws.Count}회차뿐이다. 최소 {opt.RecentRounds}회차가 필요하다.");
        return;
    }

    // 2~4. 분석과 조합 생성
    Random rng = opt.Seed is int s ? new Random(s) : new Random();
    AnalysisResult analysis = FrequencyAnalyzer.Analyze(draws, opt, rng);
    List<int[]> games = ComboGenerator.Generate(analysis, opt, rng);

    // 5. 출력과 저장
    string text = ReportBuilder.BuildText(analysis, games);
    Console.WriteLine(text);
    await File.WriteAllTextAsync("result.txt", text, Encoding.UTF8, cts.Token);
    await File.WriteAllTextAsync("result.csv", ReportBuilder.BuildCsv(analysis, games),
                                 Encoding.UTF8, cts.Token);
    Console.WriteLine("result.txt, result.csv 저장 완료");

    // 6. 백테스트 (최근 100회차, 값은 자유롭게 조정)
    BacktestResult bt = await Task.Run(() => Backtester.Run(draws, opt, 100, cts.Token), cts.Token);
    Console.WriteLine($"\n[백테스트] {bt.Rounds}회차, 총 {bt.TotalGames}게임");
    Console.WriteLine("등수   전략   무작위");
    for (int rank = 1; rank <= 5; rank++)
    {
        Console.WriteLine($"{rank}등   {bt.StrategyHits[rank],4}   {bt.RandomHits[rank],4}");
    }

    Console.WriteLine($"낙첨   {bt.StrategyHits[0],4}   {bt.RandomHits[0],4}");
}
catch (OperationCanceledException)
{
    Console.WriteLine("\n사용자가 작업을 취소했다.");
}
catch (HttpRequestException ex)
{
    Console.WriteLine($"\n네트워크 오류: {ex.Message}");
}
catch (InvalidOperationException ex)
{
    Console.WriteLine($"\n분석 오류: {ex.Message}");
}
