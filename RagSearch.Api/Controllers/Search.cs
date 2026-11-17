using Microsoft.AspNetCore.Mvc;
using RagSearch.Api.Search;

namespace RagSearch.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class Search(SearchService searchService) : Controller
    {
        private readonly SearchService _searchService = searchService;

        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] string q, [FromQuery] int top = 5, SearchFilter? searchFilter = null, CancellationToken cancellationToken = default)
        {
            var errorDict = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(q))
                errorDict.Add("q", "Parameter 'q' cannot be empty.");

            if (top > 20)
                errorDict.Add("top", "Parameter top cannot be more than 20.");
            else if (top < 0)
                errorDict.Add("top", "Parameter top cannot be negative.");

            if (errorDict.Keys.Any())
                return BadRequest(errorDict);

            return Ok(await _searchService.SearchAsync(q, top, searchFilter: searchFilter, cancellationToken));
        }
    }
}
