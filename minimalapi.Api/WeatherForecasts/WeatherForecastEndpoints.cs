using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

namespace minimalapi.Api.WeatherForecasts
{
    public static class WeatherForecastEndpoints
    {
        public static void AddWeatherForecastEndpoints(this WebApplication app)
        {
            app.MapGet("/forecast", GetForecast)
                .WithName("GetWeatherForecast")
                .WithOpenApi(x => new OpenApiOperation(x)
                {
                    Summary = "Fetch the weather forecast for a given location"
                });
        }

        public static async Task<Results<Ok<WeatherForecastResult>, ProblemHttpResult>> GetForecast(
        double latitude,
        double longitude,
        WeatherForecastService weatherForecastService)
        {
            if (latitude < -90 || latitude > 90)
            {
                return TypedResults.Problem(
                    "The value for latitude is out of range. Must be between -90 and 90",
                    statusCode: StatusCodes.Status400BadRequest
                );
            }

            var forecast = await weatherForecastService.GetForecast(latitude, longitude);

            return TypedResults.Ok(new WeatherForecastResult
            {
                TemperatureC = forecast.Current.Temperature
            });
        }
    }
}
