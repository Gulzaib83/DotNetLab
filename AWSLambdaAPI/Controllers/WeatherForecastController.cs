using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using AWSLambdaAPI.Models;
using ExternalEntities.Misc;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using NPOI.SS.UserModel;
using NPOI.Util;
using NPOI.XSSF.UserModel;
using Repository.Interfaces;
using System.Configuration;
using System.Data;
using System.IO;
using System.Net.Mime;

namespace AWSLambdaAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WeatherForecastController : ControllerBase
    {
        private static readonly string[] Summaries = new[]
        {
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    };

        private readonly ILogger<WeatherForecastController> _Logger;
        private readonly IOptions<DBSettings> _DbSettings;
        private readonly Repository.Interfaces.IToDoRepository _Repo;

        public WeatherForecastController(ILogger<WeatherForecastController> Logger, IOptions<DBSettings> DbSettings, IToDoRepository Repository)
        {
            _DbSettings = DbSettings;
            _Repo = Repository;
            _Logger = Logger;
        }

        [HttpGet(Name = "GetWeatherForecast")]
        public IEnumerable<WeatherForecast> Get()
        {
            return Enumerable.Range(1, 5).Select(index => new WeatherForecast
            {
                Date = DateTime.Now.AddDays(index),
                TemperatureC = Random.Shared.Next(-20, 55),
                Summary = Summaries[Random.Shared.Next(Summaries.Length)]
            })
            .ToArray();
        }

        
    }
}