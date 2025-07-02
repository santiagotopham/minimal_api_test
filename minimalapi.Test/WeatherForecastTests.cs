using Microsoft.AspNetCore.Http.HttpResults;
using minimalapi.Api.WeatherForecasts;
using Moq;
using RichardSzalay.MockHttp;

namespace minimalapi.Test
{
    public class WeatherForecastTests
    {
        private string sampleResponse =
        """
            {
              "generationtime_ms": 0.01704692840576172,
              "utc_offset_seconds": 0,
              "timezone": "GMT",
              "timezone_abbreviation": "GMT",
              "elevation": 38,
              "current_units": {
                "time": "iso8601",
                "interval": "seconds",
                "temperature_2m": "°C"
              },
              "current": {
                "time": "2024-07-03T09:30",
                "interval": 900,
                "temperature_2m": 16.3
              }
            }
        """;

        [Test]
        public async Task It_returns_the_expected_temperature()
        {
            var latitude = 22.4;
            var longitude = 23.5;

            // Arrange

            // Mock external dependency to Open-Meteo Forecasts
            var mockHttp = new MockHttpMessageHandler();
            mockHttp.When(WeatherForecastService.BuildUrl(latitude, longitude).ToString())
                .Respond("application/json", sampleResponse);

            // Mock CreateClient in IHttpClientFactory, and ensure it uses the mockHttp client
            var clientFactory = new Mock<IHttpClientFactory>();
            clientFactory.Setup(x => x.CreateClient(It.IsAny<string>())).Returns(mockHttp.ToHttpClient());

            // Act
            var forecast = await WeatherForecastEndpoints.GetForecast(
                latitude,
                longitude,
                new WeatherForecastService(clientFactory.Object)
            );

            // Assert
            Assert.That(forecast.Result, Is.TypeOf<Ok<WeatherForecastResult>>());

            var okResult = (Ok<WeatherForecastResult>)forecast.Result;

            Assert.Multiple(() =>
            {
                Assert.That(okResult.Value, Is.Not.Null);
                Assert.That(okResult.Value?.TemperatureC, Is.EqualTo(16.3));
            });
        }
    }
}
