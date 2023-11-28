namespace Sample.WebApi.NET7.Controllers;

using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private static readonly string[] _summaries = new[]
    {
        "Freezing",
        "Bracing",
        "Chilly",
        "Cool",
        "Mild",
        "Warm",
        "Balmy",
        "Hot",
        "Sweltering",
        "Scorching"
    };

    public WeatherForecastController() { }

    [HttpGet]
    [SuppressMessage(
        "Security",
        "CA5394:Do not use insecure randomness",
        Justification = "As designed."
    )]
    public IEnumerable<WeatherForecast> Get() =>
        Enumerable
            .Range(1, 5)
            .Select(
                index =>
                    new WeatherForecast
                    {
                        Date = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(index)),
                        TemperatureC = Random.Shared.Next(-20, 55),
                        Summary = _summaries[Random.Shared.Next(_summaries.Length)]
                    }
            )
            .ToArray();
}
