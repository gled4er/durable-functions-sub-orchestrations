using System;
using System.Configuration;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json;

namespace DurableFunc
{
    public static class Temperature
    {
        [FunctionName("Temperature")]
        public static async Task<double> Run([ActivityTrigger] string cityName)
        {   
            var apiKey = ConfigurationManager.AppSettings["WeatherApiKey"];
            double temperture;
            using (var httpClient = new HttpClient())
            {
                var uri = $"https://api.openweathermap.org/data/2.5/weather?q={cityName}&APPID={apiKey}&units=metric";
                var data = await httpClient.GetStringAsync(new Uri(uri));
                var weatherData = JsonConvert.DeserializeObject<WeatherData>(data);
                temperture = weatherData.Main.Temp;
            }

            return temperture;
        }
    }

    public class WeatherData
    {
        public WeatherDataMain Main { get; set; }
    }

    public class WeatherDataMain
    {
        public double Temp { get; set; }
    }
}
