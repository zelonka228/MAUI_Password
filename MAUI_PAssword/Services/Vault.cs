using System.Text.Json;
using System.Text.Json.Serialization;
using MAUI_PAssword.Models;

namespace MAUI_PAssword.Services;

[JsonSerializable(typeof(List<Account>))]
internal partial class VaultJson : JsonSerializerContext;

public class Vault
{
    readonly string _path = Path.Combine(FileSystem.AppDataDirectory, "vault.dat");
    bool _loaded;

    public List<Account> Items { get; private set; } = [];

    public IEnumerable<string> Tags =>
        Items.SelectMany(e => e.Tags)
             .Distinct(StringComparer.CurrentCultureIgnoreCase)
             .OrderBy(t => t, StringComparer.CurrentCultureIgnoreCase);

    public async Task LoadAsync()
    {
        if (_loaded)
            return;

        if (File.Exists(_path))
        {
            var raw = await File.ReadAllBytesAsync(_path);
            var json = await Protector.UnprotectAsync(raw);
            Items = JsonSerializer.Deserialize(json, VaultJson.Default.ListAccount) ?? [];
        }

        _loaded = true;
    }

    public async Task SaveAsync(Account entry)
    {
        var index = Items.FindIndex(e => e.Id == entry.Id);
        if (index >= 0)
            Items[index] = entry;
        else
            Items.Add(entry);

        await WriteAsync();
    }

    public async Task DeleteAsync(Account entry)
    {
        Items.RemoveAll(e => e.Id == entry.Id);
        await WriteAsync();
    }

    async Task WriteAsync()
    {
        var json = JsonSerializer.SerializeToUtf8Bytes(Items, VaultJson.Default.ListAccount);
        await File.WriteAllBytesAsync(_path, await Protector.ProtectAsync(json));
    }
}
