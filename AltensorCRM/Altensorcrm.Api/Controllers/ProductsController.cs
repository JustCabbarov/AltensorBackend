using Altensorcrm.Contract.DTOs.Product;
using Altensorcrm.Contract.Services.Product;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Altensorcrm.Api.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PRODUCTS CONTROLLER] GetAll called at {DateTime.Now:HH:mm:ss}");
        var result = await _productService.GetAllAsync(cancellationToken);
        Console.WriteLine($"[PRODUCTS CONTROLLER] GetAll returned {result.Count} items.");
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _productService.GetByIdAsync(id, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductDto dto, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PRODUCTS CONTROLLER] Create called: Name='{dto.ProductName}', Code='{dto.ProductCode}', Rate={dto.StandardSellingRate}");
        var result = await _productService.CreateAsync(dto, cancellationToken);
        Console.WriteLine($"[PRODUCTS CONTROLLER] Product CREATED in DB with ID: {result.Id}");
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductDto dto, CancellationToken cancellationToken)
    {
        if (id != dto.Id)
        {
            return BadRequest("Route ID and DTO ID do not match.");
        }

        var result = await _productService.UpdateAsync(dto, cancellationToken);
        return Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        Console.WriteLine($"[PRODUCTS CONTROLLER] Delete requested for ID: {id}");
        var result = await _productService.DeleteAsync(id, cancellationToken);
        Console.WriteLine($"[PRODUCTS CONTROLLER] Product {id} DELETED from Database: {result}");
        return Ok(result);
    }

    [HttpPost("upload-image")]
    public async Task<IActionResult> UploadImage(IFormFile file, [FromServices] Microsoft.AspNetCore.Hosting.IWebHostEnvironment env)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("File is empty.");
        }

        var webRoot = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var uploadsFolder = Path.Combine(webRoot, "uploads", "products");

        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var extension = Path.GetExtension(file.FileName);
        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        var fileUrl = $"/uploads/products/{uniqueFileName}";
        return Ok(new { url = fileUrl, fileName = file.FileName });
    }
}
