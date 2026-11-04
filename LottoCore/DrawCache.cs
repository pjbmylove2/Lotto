using System.Text.Json;

namespace LottoCore;

/// <summary>회차 데이터를 JSON 파일로 저장하고 읽는다.</summary>
public sealed class DrawCache
{
    private static readonly JsonSerializerOptions JsonOpt = new() { WriteIndented = true };
    private readonly string _path;

    public DrawCache(string path) => _path = path;

    public async Task<List<Draw>> LoadAsync(CancellationToken ct)
    {
        if (!File.Exists(_path))
        {
            return new List<Draw>();
        }

        await using FileStream fs = File.OpenRead(_path);
        return await JsonSerializer.DeserializeAsync<List<Draw>>(fs, JsonOpt, ct)
               ?? new List<Draw>();
    }

    public async Task SaveAsync(IEnumerable<Draw> draws, CancellationToken ct)
    {
        await using FileStream fs = File.Create(_path);
        await JsonSerializer.SerializeAsync(fs, draws.OrderBy(d => d.Round).ToList(), JsonOpt, ct);
    }
}
