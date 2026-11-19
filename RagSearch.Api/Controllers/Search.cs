using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RagSearch.Api.Search;

namespace RagSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Search(SearchService searchService) : Controller
    {
        private readonly SearchService _searchService = searchService;

        [HttpGet]
        [EnableRateLimiting("")]
        public async Task<IActionResult> Index([FromQuery] string q, [FromQuery] int top = 5, [FromBody] SearchFilter? searchFilter = default, CancellationToken cancellationToken = default)
        {
            var errorDict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(q))
                errorDict.Add("q", "Parameter 'q' cannot be empty.");
            else if (q.Length > 500)
                errorDict.Add("q", "Parameter 'q' has a max length of 500 chars.");

            if (top > 20)
                errorDict.Add("top", "Parameter top cannot be more than 20.");
            else if (top < 0)
                errorDict.Add("top", "Parameter top cannot be negative.");

            if (errorDict.Keys.Count != 0)
                return BadRequest(errorDict);

            return Ok(await _searchService.SearchAsync(q, top, cancellationToken, searchFilter: searchFilter));
        }
    }
}
