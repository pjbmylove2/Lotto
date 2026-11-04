using System.Globalization;
using System.Text.Json;

namespace LottoCore;

/// <summary>동행복권 회차 조회 주소에서 당첨번호를 가져온다. 비공식 주소라 실패할 수 있다.</summary>
public sealed class DhLotteryApiSource
{
    private const string UrlFormat =
        "https://dhlottery.co.kr/lt645/selectPstLt645Info.do?srchLtEpsd={0}&_={1}";

    private readonly HttpClient _http;

    public DhLotteryApiSource(HttpClient http) => _http = http;

    /// <summary>해당 회차를 가져온다. 없거나 형식이 다르면 null.</summary>
    public async Task<Draw?> GetDrawAsync(int round, CancellationToken ct)
    {
        if (round < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(round));
        }

        string url = string.Format(CultureInfo.InvariantCulture, UrlFormat,
            round, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        using HttpResponseMessage res = await _http.GetAsync(url, ct);
        if (!res.IsSuccessStatusCode)
        {
            return null;
        }

        string body = await res.Content.ReadAsStringAsync(ct);
        try
        {
            using JsonDocument doc = JsonDocument.Parse(body);
            JsonElement root = doc.RootElement;

            JsonElement list = root.GetProperty("data").GetProperty("list");
            foreach (JsonElement item in list.EnumerateArray())
            {
                // 요청한 회차가 있는지 확인해 다른 회차를 저장하지 않는다.
                if (item.GetProperty("ltEpsd").GetInt32() != round)
                {
                    continue;
                }

                int[] nums = new int[6];
                for (int i = 0; i < 6; i++)
                {
                    nums[i] = item.GetProperty($"tm{i + 1}WnNo").GetInt32();
                }

                int bonus = item.GetProperty("bnsWnNo").GetInt32();
                if (nums.Any(n => n < 1 || n > 45) || nums.Distinct().Count() != 6
                    || bonus < 1 || bonus > 45 || nums.Contains(bonus))
                {
                    return null;
                }

                Array.Sort(nums);
                if (!DateOnly.TryParseExact(item.GetProperty("ltRflYmd").GetString(),
                    "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,
                    out DateOnly date))
                {
                    return null;
                }

                return new Draw(round, date, nums, bonus);
            }

            return null; // 미추첨 회차는 빈 목록으로 응답한다.
        }
        catch (Exception ex) when (ex is JsonException or KeyNotFoundException
                                       or FormatException or InvalidOperationException)
        {
            return null; // 점검 페이지(HTML) 등 예상과 다른 응답
        }
    }

    /// <summary>이 조회 방식이 지금 동작하는지 1회차로 시험한다.</summary>
    public async Task<bool> IsAvailableAsync(CancellationToken ct)
    {
        try
        {
            return await GetDrawAsync(1, ct) is not null;
        }
        catch (HttpRequestException)
        {
            return false; // 네트워크 오류
        }
        catch (TaskCanceledException) when (!ct.IsCancellationRequested)
        {
            return false; // 사용자가 취소한 것이 아니라 시간 초과
        }
    }
}
