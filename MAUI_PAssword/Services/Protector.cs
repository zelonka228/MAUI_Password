using System.Security.Cryptography;

namespace MAUI_PAssword.Services;

// Шифрування файлу сховища.
// Windows: DPAPI (ключ прив'язаний до облікового запису користувача).
// Інші платформи: AES-GCM, ключ лежить у SecureStorage.
public static class Protector
{
#if WINDOWS
    public static Task<byte[]> ProtectAsync(byte[] data) =>
        Task.FromResult(ProtectedData.Protect(data, null, DataProtectionScope.CurrentUser));

    public static Task<byte[]> UnprotectAsync(byte[] data) =>
        Task.FromResult(ProtectedData.Unprotect(data, null, DataProtectionScope.CurrentUser));
#else
    const string KeyName = "vault-key";

    public static async Task<byte[]> ProtectAsync(byte[] data)
    {
        var key = await GetKeyAsync();
        var nonce = RandomNumberGenerator.GetBytes(12);
        var tag = new byte[16];
        var cipher = new byte[data.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Encrypt(nonce, data, cipher, tag);

        return [.. nonce, .. tag, .. cipher];
    }

    public static async Task<byte[]> UnprotectAsync(byte[] data)
    {
        var key = await GetKeyAsync();
        var nonce = data[..12];
        var tag = data[12..28];
        var cipher = data[28..];
        var plain = new byte[cipher.Length];

        using var aes = new AesGcm(key, tag.Length);
        aes.Decrypt(nonce, cipher, tag, plain);

        return plain;
    }

    static async Task<byte[]> GetKeyAsync()
    {
        var stored = await SecureStorage.Default.GetAsync(KeyName);
        if (stored is not null)
            return Convert.FromBase64String(stored);

        var key = RandomNumberGenerator.GetBytes(32);
        await SecureStorage.Default.SetAsync(KeyName, Convert.ToBase64String(key));
        return key;
    }
#endif
}
