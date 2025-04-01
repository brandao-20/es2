using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriasController : ControllerBase
    {
        private readonly ICategoriaRepository _categoriaRepository;

        public CategoriasController(ICategoriaRepository categoriaRepository)
        {
            _categoriaRepository = categoriaRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetAll()
        {
            return Ok(await _categoriaRepository.GetAllAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Categoria>> GetById(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null) return NotFound();
            return categoria;
        }

        [HttpPost]
        public async Task<ActionResult<Categoria>> Create(Categoria categoria)
        {
            await _categoriaRepository.AddAsync(categoria);
            return CreatedAtAction(nameof(GetById), new { id = categoria.CategoriaId }, categoria);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Categoria categoria)
        {
            if (id != categoria.CategoriaId) return BadRequest();

            try
            {
                await _categoriaRepository.UpdateAsync(categoria);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var categoria = await _categoriaRepository.GetByIdAsync(id);
            if (categoria == null) return NotFound();

            await _categoriaRepository.DeleteAsync(categoria);
            return NoContent();
        }
    }
}
