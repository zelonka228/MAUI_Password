using MAUI_PAssword.Services;
using Microsoft.Extensions.Logging;

namespace MAUI_PAssword
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if WINDOWS
            // Прибираємо власну рамку WinUI TextBox: рамку малює Border у XAML
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoBorder", (handler, _) =>
            {
                var box = handler.PlatformView;
                var none = new Microsoft.UI.Xaml.Thickness(0);
                var clear = new Microsoft.UI.Xaml.Media.SolidColorBrush(Microsoft.UI.Colors.Transparent);
                box.BorderThickness = none;
                foreach (var key in new[] { "", "PointerOver", "Focused", "Disabled" })
                {
                    box.Resources[$"TextControlBorderThemeThickness{key}"] = none;
                    box.Resources[$"TextControlBackground{key}"] = clear;
                    box.Resources[$"TextControlBorderBrush{key}"] = clear;
                }
            });
#endif

            builder.Services.AddSingleton<Vault>();
            builder.Services.AddTransient<MainPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
