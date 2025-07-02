namespace minimalapi.Api.WeatherForecasts
{
    public static class ServiceCollectionExtensions
    {
        public static void AddWeatherForecastServices(this IServiceCollection services)
        {
            services.AddSingleton<WeatherForecastService>();
        }
    }
}
