namespace LottoCore;

/// <summary>한 회차의 당첨 정보. Numbers는 오름차순으로 정렬해 둔다.</summary>
public sealed record Draw(int Round, DateOnly Date, int[] Numbers, int Bonus);
