using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using MauiPlayer;

namespace MauiPlayer
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder.UseMauiApp<App>().ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            })
            .ConfigureMauiHandlers(handlers => handlers.AddHandler(typeof(VideoPlayer), typeof(VideoHandler)));
#if DEBUG
            builder.Logging.AddDebug();
#endif
            var app = builder.Build();
            return app;
        }
    }
}
