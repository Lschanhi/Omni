using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Omnimarket.Api.Models.Entidades;
using Omnimarket.Api.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Omnimarket.Api.Models.Dtos.Produtos;
using Omnimarket.Api.Services;
using Omni.Models.Dtos.Produtos;


namespace Omnimarket.Api.Controllers
{
   [ApiController]
[Route("api/[controller]")]
public class ProdutoController : ControllerBase
{
    private readonly IProdutoService _service;

    public ProdutoController(IProdutoService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> Get(int id)
    {
        var produto = await _service.GetByIdAsync(id);

        if (produto == null)
            return NotFound();

        return Ok(produto);
    }

    [HttpPost]
    public async Task<IActionResult> Post(ProdutoCriacaoDto dto)
    {
        int usuarioId = 1; // depois vem do JWT

        var produto = await _service.CreateAsync(dto, usuarioId);

        return CreatedAtAction(nameof(Get), new { id = produto.Id }, produto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Put(int id, ProdutoAtualizarDto dto)
    {
        var updated = await _service.UpdateAsync(id, dto);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await _service.DeleteAsync(id);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("filtro")]
    public async Task<IActionResult> GetPaged([FromQuery] ProdutoFiltroDto filtro)
    {
    var result = await _service.GetPagedAsync(filtro);
    return Ok(result);
    }
}

    internal interface IProdutoService
    {
    }
}
