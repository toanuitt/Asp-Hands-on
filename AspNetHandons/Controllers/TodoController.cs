using Microsoft.AspNetCore.Mvc;
using AspNetHandons.ExternalApis;

namespace AspNetHandons.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TodoController : ControllerBase
{
    private readonly IJsonTodo _api;

    public TodoController(IJsonTodo api)
    {
        _api = api;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var result = await _api.GetTodo(id);
        return Ok(result);
    }
}