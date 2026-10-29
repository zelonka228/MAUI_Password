namespace MAUI_PAssword.Services;

public static class Chip
{
    public static View Create(string text, bool selected, Action onTap)
    {
        var accent = Resource("AccentLight", "AccentDark");
        var onAccent = Resource("OnAccentLight", "OnAccentDark");

        var label = new Label
        {
            Text = text,
            FontSize = 12,
            VerticalTextAlignment = TextAlignment.Center,
        };
        if (selected)
            label.TextColor = onAccent;

        var border = new Border
        {
            Padding = new Thickness(10, 4),
            Margin = new Thickness(0, 0, 6, 6),
            StrokeShape = new Microsoft.Maui.Controls.Shapes.RoundRectangle { CornerRadius = 12 },
            Content = label,
        };
        if (selected)
        {
            border.BackgroundColor = accent;
            border.Stroke = accent;
        }

        border.GestureRecognizers.Add(new TapGestureRecognizer { Command = new Command(onTap) });
        return border;
    }

    static Color Resource(string light, string dark)
    {
        var key = Application.Current?.RequestedTheme == AppTheme.Dark ? dark : light;
        return (Color)Application.Current!.Resources[key];
    }
}
