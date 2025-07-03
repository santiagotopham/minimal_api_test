using System.Text.Json;
using System.Text.Json.Serialization;

namespace minimalapi.Api.WeatherForecasts
{
    public class WeatherForecastService(IHttpClientFactory httpClientFactory)
    {
        public class OpenMeteoForecastResponse
        {
            [JsonPropertyName("longitude")]
            public double Longitude { get; set; }

            [JsonPropertyName("latitude")]
            public double Latitude { get; set; }

            [JsonPropertyName("current")]
            public OpenMeteoForecastCurrent Current { get; set; }
        }

        public class OpenMeteoForecastCurrent
        {
            [JsonPropertyName("temperature_2m")]
            public double Temperature { get; set; }
        }

        public async Task<OpenMeteoForecastResponse> GetForecast(double latitude, double longitude)
        {
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                BuildUrl(latitude, longitude)
            );

            var httpClient = httpClientFactory.CreateClient("WeatherForecast");
            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
            if (!httpResponseMessage.IsSuccessStatusCode)
            {
                throw new Exception("Failed to get forecast");
            }

            await using var response = await httpResponseMessage.Content.ReadAsStreamAsync();
            var forecastResponse = await JsonSerializer.DeserializeAsync<OpenMeteoForecastResponse>(response);
            if (forecastResponse == null)
            {
                throw new Exception("Error deserializing forecast response");
            }

            return forecastResponse;
        }

        public static Uri BuildUrl(double latitude, double longitude)
        {
            return new Uri($"https://api.open-meteo.com/v1/forecast?latitude={latitude}\u0026longitude={longitude}\u0026current=temperature_2m");
        }

        public async Task<OpenMeteoForecastResponse> GetForecastByTimezone(string timezone)
        {
            // Here, imagine we use an API or internal logic to resolve the timezone to a pair of coordinates
            var coordinates = TimezoneToCoordinates(timezone);
            return await GetForecast(coordinates.Latitude, coordinates.Longitude);
        }

        private (double Latitude, double Longitude) TimezoneToCoordinates(string timezone)
        {
            // Simulate timezone to coordinates lookup - mapping major cities to their timezones
            return timezone.ToUpperInvariant() switch
            {
                "UTC" => (0.0, 0.0), // Prime Meridian
                "EST" or "AMERICA/NEW_YORK" => (40.7128, -74.0060), // New York
                "PST" or "AMERICA/LOS_ANGELES" => (34.0522, -118.2437), // Los Angeles
                "GMT" or "EUROPE/LONDON" => (51.5074, -0.1278), // London
                "CET" or "EUROPE/PARIS" => (48.8566, 2.3522), // Paris
                "JST" or "ASIA/TOKYO" => (35.6762, 139.6503), // Tokyo
                "CST" or "ASIA/SHANGHAI" => (31.2304, 121.4737), // Shanghai
                "AEST" or "AUSTRALIA/SYDNEY" => (-33.8688, 151.2093), // Sydney
                "CLT" or "AMERICA/SANTIAGO" => (-33.4489, -70.6693), // Santiago
                "BRT" or "AMERICA/SAO_PAULO" => (-23.5505, -46.6333), // São Paulo
                _ => throw new ArgumentException($"Unknown timezone: {timezone}. Supported timezones: UTC, EST, PST, GMT, CET, JST, CST, AEST, CLT, BRT")
            };
        }
    }
}
