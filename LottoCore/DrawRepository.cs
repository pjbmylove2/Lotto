namespace LottoCore;

/// <summary>캐시를 읽고, 새 회차만 받아 추가한 뒤 다시 저장한다.</summary>
public sealed class DrawRepository
{
    private readonly DrawCache _cache;
    private readonly DhLotteryApiSource _api;
    private readonly AnalysisOptions _opt;

    public DrawRepository(DrawCache cache, DhLotteryApiSource api, AnalysisOptions opt)
    {
        _cache = cache;
        _api = api;
        _opt = opt;
    }

    /// <summary>캐시의 마지막 회차 다음부터 받을 수 없을 때까지 요청한다.</summary>
    public async Task<List<Draw>> UpdateAsync(IProgress<int>? progress, CancellationToken ct)
    {
        List<Draw> draws = await _cache.LoadAsync(ct);
        int next = draws.Count == 0 ? 1 : draws.Max(d => d.Round) + 1;
        int added = 0;

        try
        {
            while (true)
            {
                Draw? d = await _api.GetDrawAsync(next, ct);
                if (d is null)
                {
                    break; // 아직 추첨 전이거나 응답 실패
                }

                draws.Add(d);
                added++;
                progress?.Report(d.Round);
                next++;

                await Task.Delay(_opt.RequestDelayMs, ct); // 서버 부담 줄이기
            }
        }
        finally
        {
            // 중간에 취소·오류가 나도 받은 만큼은 저장한다
            if (added > 0)
            {
                await _cache.SaveAsync(draws, CancellationToken.None);
            }
        }
        return draws;
    }

    /// <summary>CSV 데이터를 캐시에 합친다. 같은 회차는 CSV 쪽으로 덮어쓴다.</summary>
    public async Task<List<Draw>> ImportCsvAsync(string csvPath, CancellationToken ct)
    {
        Dictionary<int, Draw> merged = (await _cache.LoadAsync(ct)).ToDictionary(d => d.Round);
        foreach (Draw d in await CsvDrawSource.LoadAsync(csvPath, ct))
        {
            merged[d.Round] = d;
        }

        List<Draw> list = merged.Values.OrderBy(d => d.Round).ToList();
        await _cache.SaveAsync(list, ct);
        return list;
    }
}
