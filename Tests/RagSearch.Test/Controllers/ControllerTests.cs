using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using RagSearch.Api.Controllers;
using System.Text.Json;

namespace RagSearch.Test.Controllers;

public class ControllerTests
{
    private readonly WebApplicationFactory<Program> _factory;

    public ControllerTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(b=> 
        {
            b.UseEnvironment("Testing"); 
            b.UseSetting("EntraId:TenantId", "00000000-0000-0000-0000-000000000000");
            b.UseSetting("OpenAI:Uri", "https://fake.openai.azure.com/");
            b.UseSetting("Cosmos:Endpoint", "https://fake.documents.azure.com:443/");
        });
    }

    [Theory]
    [InlineData("api/Health")]
    public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
    {
        // arrange
        var client = _factory.CreateClient();

        // act
        var response = await client.GetAsync(url, TestContext.Current.CancellationToken);

        //assert
        response.EnsureSuccessStatusCode();
        Assert.Equal(expected: "application/json; charset=utf-8", actual: response.Content.Headers.ContentType?.ToString());
    }

    [Fact]
    public async Task HealthTest()
    {
        // arrange
        var healthController = new HealthController();

        // act
        var result = await healthController.Index();

        //assert
        var actionResult = Assert.IsAssignableFrom<OkObjectResult>(result);
        Assert.Equal(expected: 200, actual: actionResult.StatusCode);
        var response = JsonSerializer.SerializeToElement(actionResult.Value);
        Assert.Equal(expected: "Healthy", actual: response.GetProperty("Status").GetString());
        Assert.NotEmpty(response.GetProperty("BuildVersion").GetString() ?? string.Empty);
    }
}
