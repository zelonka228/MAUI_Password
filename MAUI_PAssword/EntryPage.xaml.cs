using MAUI_PAssword.Models;
using MAUI_PAssword.Services;

namespace MAUI_PAssword;

public partial class EntryPage : ContentPage
{
    readonly Vault _vault;
    readonly Account _entry;
    readonly bool _isNew;
    readonly bool[] _sets = [true, true, true, true];
    static readonly string[] SetNames = ["a–z", "A–Z", "0–9", "!@#$"];

    public EntryPage(Vault vault, Account? entry)
    {
        InitializeComponent();
        _vault = vault;
        _isNew = entry is null;
        _entry = entry ?? new Account();

        Heading.Text = _isNew ? "Новий запис" : "Запис";
        NameBox.Text = _entry.Name;
        UrlBox.Text = _entry.Url;
        PasswordBox.Text = _entry.Password;
        TagsBox.Text = string.Join(", ", _entry.Tags);
        DeleteButton.IsVisible = !_isNew;

        RefreshOptions();
        UpdateEntropy();
        RefreshSuggestions();
    }

    void RefreshOptions()
    {
        Options.Clear();
        for (int i = 0; i < SetNames.Length; i++)
        {
            int index = i;
            Options.Add(Chip.Create(SetNames[i], _sets[i], () =>
            {
                // хоча б один набір має лишитись
                if (_sets[index] && _sets.Count(x => x) == 1)
                    return;

                _sets[index] = !_sets[index];
                RefreshOptions();
                UpdateEntropy();
            }));
        }
    }

    List<string> ParseTags() =>
        (TagsBox.Text ?? "")
            .Split([',', ';'], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Distinct(StringComparer.CurrentCultureIgnoreCase)
            .ToList();

    void RefreshSuggestions()
    {
        var current = ParseTags();
        TagSuggestions.Clear();
        foreach (var tag in _vault.Tags)
        {
            var name = tag;
            var selected = current.Contains(tag, StringComparer.CurrentCultureIgnoreCase);
            TagSuggestions.Add(Chip.Create(tag, selected, () => ToggleTag(name)));
        }
    }

    void ToggleTag(string tag)
    {
        var tags = ParseTags();
        var existing = tags.FindIndex(t => string.Equals(t, tag, StringComparison.CurrentCultureIgnoreCase));
        if (existing >= 0)
            tags.RemoveAt(existing);
        else
            tags.Add(tag);

        TagsBox.Text = string.Join(", ", tags);
    }

    void OnTagsChanged(object? sender, TextChangedEventArgs e) => RefreshSuggestions();

    void OnToggleClicked(object? sender, EventArgs e)
    {
        PasswordBox.IsPassword = !PasswordBox.IsPassword;
        ToggleButton.Text = PasswordBox.IsPassword ? "Показати" : "Сховати";
    }

    void OnLengthChanged(object? sender, ValueChangedEventArgs e)
    {
        if (LengthLabel is null || EntropyLabel is null)
            return;

        LengthLabel.Text = ((int)e.NewValue).ToString();
        UpdateEntropy();
    }

    void UpdateEntropy()
    {
        var bits = PasswordGenerator.Entropy((int)LengthSlider.Value, _sets[0], _sets[1], _sets[2], _sets[3]);
        EntropyLabel.Text = $"{bits:0} біт";
    }

    void OnGenerateClicked(object? sender, EventArgs e)
    {
        var password = PasswordGenerator.Generate(
            (int)LengthSlider.Value, _sets[0], _sets[1], _sets[2], _sets[3]);

        if (password.Length == 0)
            return;

        PasswordBox.Text = password;
        PasswordBox.IsPassword = false;
        ToggleButton.Text = "Сховати";
    }

    async void OnSaveClicked(object? sender, EventArgs e)
    {
        var name = NameBox.Text?.Trim() ?? "";
        var password = PasswordBox.Text ?? "";

        if (name.Length == 0 || password.Length == 0)
        {
            await DisplayAlertAsync("Не збережено", "Вкажіть назву і пароль.", "OK");
            return;
        }

        _entry.Name = name;
        _entry.Url = UrlBox.Text?.Trim() ?? "";
        _entry.Password = password;
        _entry.Tags = ParseTags();

        await _vault.SaveAsync(_entry);
        await Navigation.PopAsync();
    }

    async void OnDeleteClicked(object? sender, EventArgs e)
    {
        if (!await DisplayAlertAsync("Видалити запис", $"Видалити «{_entry.Name}»?", "Видалити", "Скасувати"))
            return;

        await _vault.DeleteAsync(_entry);
        await Navigation.PopAsync();
    }

    async void OnBackClicked(object? sender, EventArgs e) => await Navigation.PopAsync();
}
