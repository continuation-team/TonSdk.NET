namespace TonSdk.Contracts;

public static class SmcUtils {
    public static ulong GenerateQueryId(int timeout, int? randomId = null) {
        var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        var random = randomId ?? new Random().Next(0, (int)Math.Pow(2, 30));

        return (ulong)((now + timeout) << 32) | (uint)random;
    }
}
