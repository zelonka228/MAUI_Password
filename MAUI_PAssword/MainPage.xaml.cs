using MAUI_PAssword.Models;
using MAUI_PAssword.Services;

namespace MAUI_PAssword;

public partial class MainPage : ContentPage
{
    const int ClipboardSeconds = 30;

    readonly Vault _vault;
    string? _tag;

    public MainPage(Vault vault)
    {
        InitializeComponent();
        _vault = vault;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vault.LoadAsync();
        Refresh();
    }

    void Refresh()
    {
        if (_tag is not null && !_vault.Tags.Contains(_tag, StringComparer.CurrentCultureIgnoreCase))
            _tag = null;

        var query = SearchBox.Text?.Trim() ?? "";
        var items = _vault.Items
            .Where(e => _tag is null || e.Tags.Contains(_tag, StringComparer.CurrentCultureIgnoreCase))
            .Where(e => query.Length == 0
                || e.Name.Contains(query, StringComparison.CurrentCultureIgnoreCase)
                || e.Url.Contains(query, StringComparison.CurrentCultureIgnoreCase))
            .OrderBy(e => e.Name, StringComparer.CurrentCultureIgnoreCase)
            .ToList();

        List.ItemsSource = items;
        EmptyLabel.IsVisible = items.Count == 0;

        TagBar.Clear();
        foreach (var tag in _vault.Tags)
        {
            var selected = string.Equals(tag, _tag, StringComparison.CurrentCultureIgnoreCase);
            var name = tag;
            TagBar.Add(Chip.Create(tag, selected, () =>
            {
                _tag = selected ? null : name;
                Refresh();
            }));
        }
    }

    void OnSearchChanged(object? sender, TextChangedEventArgs e) => Refresh();

    async void OnAddClicked(object? sender, EventArgs e) =>
        await Navigation.PushAsync(new EntryPage(_vault, null));

    async void OnSelected(object? sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is not Account entry)
            return;

        List.SelectedItem = null;
        await Navigation.PushAsync(new EntryPage(_vault, entry));
    }

    async void OnCopyClicked(object? sender, EventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Account entry)
            return;

        await Clipboard.Default.SetTextAsync(entry.Password);
        await ShowToast($"Скопійовано. Буфер очиститься через {ClipboardSeconds} с");

        await Task.Delay(TimeSpan.FromSeconds(ClipboardSeconds));
        if (await Clipboard.Default.GetTextAsync() == entry.Password)
            await Clipboard.Default.SetTextAsync("");
    }

    async Task ShowToast(string text)
    {
        Toast.Text = text;
        await Toast.FadeToAsync(1, 120);
        await Task.Delay(1800);
        await Toast.FadeToAsync(0, 300);
    }
}
