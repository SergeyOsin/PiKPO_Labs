using FoodSelection.Model;
using FoodSelection.Metrics;
using FoodSelection.Models;
using FoodSelection.Services;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;

namespace FoodSelection.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FoodProductController : ControllerBase
{
    private readonly IFoodProductService _foodProductService;
    private readonly MetricService _metrics;

    public FoodProductController(IFoodProductService foodProductService, MetricService metrics)
    {
        _foodProductService = foodProductService;
        _metrics = metrics;
    }

    [HttpGet]
    public Task<ActionResult<List<FoodProductResponseDto>>> GetAll() =>
        _metrics.TrackOperationAsync<ActionResult<List<FoodProductResponseDto>>>("list", async () =>
            Ok(await _foodProductService.GetAllAsync()));

    [HttpGet("{id}")]
    public Task<ActionResult<FoodProductResponseDto>> GetById(string id) =>
        _metrics.TrackOperationAsync<ActionResult<FoodProductResponseDto>>("by_id", async () =>
    {
        if (!ObjectId.TryParse(id, out _))
            return BadRequest("Incorrect ID-format!");

        var product = await _foodProductService.GetByIdAsync(id);

        if (product == null)
            return NotFound($"Product with ID {id} not found");

        return Ok(product);
    });

    [HttpGet("filter")]
    public Task<ActionResult<List<FoodProductResponseDto>>> Filter(
        [FromQuery] FoodProductFilterDto filter) =>
        _metrics.TrackOperationAsync<ActionResult<List<FoodProductResponseDto>>>("filter", async () =>
            Ok(await _foodProductService.FilterAsync(filter)));

    [HttpPost]
    public Task<ActionResult<FoodProductResponseDto>> Create([FromBody] FoodProductCreateDto createDto) =>
        _metrics.TrackOperationAsync<ActionResult<FoodProductResponseDto>>("create", async () =>
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _foodProductService.CreateAsync(createDto);

            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        });

    [HttpPut("{id}")]
    public Task<IActionResult> Update(string id, [FromBody] FoodProductResponseDto updateDto) =>
        _metrics.TrackOperationAsync<IActionResult>("update", async () =>
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Incorrect ID-format!");

            var result = await _foodProductService.UpdateAsync(id, updateDto);
            return result ? NoContent() : NotFound($"Product with ID {id} not found");
        });

    [HttpDelete("{id}")]
    public Task<IActionResult> Delete(string id) =>
        _metrics.TrackOperationAsync<IActionResult>("delete", async () =>
        {
            if (!ObjectId.TryParse(id, out _))
                return BadRequest("Incorrect ID-format!");

            var result = await _foodProductService.DeleteAsync(id);

            return result ? Ok(new { message = "Product successfully deleted" }) : NotFound($"Product with ID {id} not found");
        });

    [HttpDelete("DeleteAll")]
    public Task<IActionResult> DeleteAll() =>
        _metrics.TrackOperationAsync<IActionResult>("delete_all", async () =>
        {
            await _foodProductService.DeleteAllAsync();
            return Ok(new { message = "All products are DELETED!" });
        });
}