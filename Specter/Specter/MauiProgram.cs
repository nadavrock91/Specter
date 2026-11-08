using Microsoft.Extensions.Logging;

using CommunityToolkit.Maui;
using Specter.Services;
using Specter.Views;

namespace Specter
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            builder.Services.AddSingleton<DatabaseService>();
            builder.Services.AddSingleton<AuthService>();
            builder.Services.AddSingleton<BackupService>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<HomePage>();

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
