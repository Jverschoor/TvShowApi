using MediatR;
using Microsoft.AspNetCore.Mvc;
using TvShows.Application.Shows.Commands.CreateShow;
using TvShows.Application.Shows.Commands.DeleteShow;
using TvShows.Application.Shows.Commands.UpdateShow;
using TvShows.Application.Shows.Queries.GetAllShows;
using TvShows.Application.Shows.Queries.GetShowById;
using TvShows.Application.Shows.Queries.SearchShows;

namespace TvShows.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ShowsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 50, CancellationToken cancellationToken = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 200);
        var result = await mediator.Send(new GetAllShowsQuery(page, pageSize), cancellationToken);
        return Ok(result);
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search([FromQuery] string name, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest("Name query parameter is required.");

        var shows = await mediator.Send(new SearchShowsQuery(name), cancellationToken);
        return Ok(shows);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id, CancellationToken cancellationToken)
    {
        var show = await mediator.Send(new GetShowByIdQuery(id), cancellationToken);
        return show is null ? NotFound() : Ok(show);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateShowCommand command, CancellationToken cancellationToken)
    {
        var show = await mediator.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = show.Id }, show);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateShowCommand command, CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest("Route id does not match command id.");

        var show = await mediator.Send(command, cancellationToken);
        return show is null ? NotFound() : Ok(show);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(new DeleteShowCommand(id), cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}
