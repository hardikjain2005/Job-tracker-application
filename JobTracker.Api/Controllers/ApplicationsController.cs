using System.Security.Claims;
using JobTracker.Api.Dtos;
using JobTracker.Api.Models;
using JobTracker.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobTracker.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/applications")]
public class ApplicationsController(ApplicationService service) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<ActionResult<List<ApplicationResponse>>> GetAll([FromQuery] ApplicationStatus? status) =>
        await service.GetAllAsync(UserId, status);

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ApplicationResponse>> GetById(int id)
    {
        var result = await service.GetByIdAsync(UserId, id);
        return result is null ? NotFound() : result;
    }

    [HttpPost]
    public async Task<ActionResult<ApplicationResponse>> Create(ApplicationRequest request)
    {
        var created = await service.CreateAsync(UserId, request);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ApplicationResponse>> Update(int id, ApplicationRequest request)
    {
        var result = await service.UpdateAsync(UserId, id, request);
        return result is null ? NotFound() : result;
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id) =>
        await service.DeleteAsync(UserId, id) ? NoContent() : NotFound();
}
