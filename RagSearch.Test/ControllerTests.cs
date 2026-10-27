using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using RagSearch.Api.Controllers;
using System.Reflection;
using System.Text.Json;

namespace RagSearch.Test;

public class ControllerTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public ControllerTests()
    {
        _factory = new WebApplicationFactory<Program>() { };
    }

    [Theory]
    [InlineData("api/Health")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // arrange
        var client = _factory.CreateClient();

        // act
        var response = await client.GetAsync(url);

        //assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(expected: "application/json; charset=utf-8", actual: response.Content.Headers.ContentType.ToString());
    }

    [Fact]
    public async Task HealthTest()
    {
        // arrange
        var healthController = new HealthController();
        var versionInfo = Assembly.GetEntryAssembly()?
            .GetCustomAttribute<AssemblyInformationalVersionAttribute>()?
            .InformationalVersion;

        // act
        var result = await healthController.Index();

        //assert

        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result);
        Assert.Equivalent(actionResult.Value, new { Status = "Healthy", BuildVersion = versionInfo });

    }
}
