using System.Globalization;

namespace LottoCore;

/// <summary>CSV 파일에서 회차 데이터를 읽는다. 첫 줄은 헤더로 보고 건너뛴다.</summary>
public static class CsvDrawSource
{
    public static async Task<List<Draw>> LoadAsync(string path, CancellationToken ct)
    {
        string[] lines = await File.ReadAllLinesAsync(path, ct);
        var draws = new List<Draw>();

        foreach (string line in lines.Skip(1))
        {
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            string[] c = line.Split(',');
            int[] nums = c[2..8].Select(int.Parse).ToArray();
            Array.Sort(nums);

            draws.Add(new Draw(
                int.Parse(c[0]),
                DateOnly.ParseExact(c[1].Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture),
                nums,
                int.Parse(c[8])));
        }
        return draws;
    }
}
