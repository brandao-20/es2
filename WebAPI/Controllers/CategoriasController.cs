using Microsoft.AspNetCore.Mvc;
using WebAPI.Entities;
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
        public async Task<ActionResult<ApiResponse<IEnumerable<Categoria>>>> GetAll()
        {
            try
            {
                var categorias = await _categoriaRepository.GetAllAsync();
                return Ok(new ApiResponse<IEnumerable<Categoria>>
                {
                    Success = true,
                    Message = "Categorias carregadas com sucesso.",
                    StatusCode = 200,
                    Data = categorias
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<IEnumerable<Categoria>>
                {
                    Success = false,
                    Message = $"Erro ao carregar as categorias: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApiResponse<Categoria>>> GetById(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<Categoria>
                    {
                        Success = false,
                        Message = "ID da categoria inválido.",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var categoria = await _categoriaRepository.GetByIdAsync(id);
                if (categoria == null)
                {
                    return NotFound(new ApiResponse<Categoria>
                    {
                        Success = false,
                        Message = "Categoria não encontrada.",
                        StatusCode = 404,
                        Data = null
                    });
                }

                return Ok(new ApiResponse<Categoria>
                {
                    Success = true,
                    Message = "Categoria encontrada com sucesso.",
                    StatusCode = 200,
                    Data = categoria
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<Categoria>
                {
                    Success = false,
                    Message = $"Erro ao buscar a categoria: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPost]
        public async Task<ActionResult<ApiResponse<Categoria>>> Create(Categoria categoria)
        {
            try
            {
                if (categoria == null || string.IsNullOrEmpty(categoria.Nome))
                {
                    return BadRequest(new ApiResponse<Categoria>
                    {
                        Success = false,
                        Message = "Dados da categoria inválidos.",
                        StatusCode = 400,
                        Data = null
                    });
                }

                await _categoriaRepository.AddAsync(categoria);
                return CreatedAtAction(nameof(GetById), new { id = categoria.CategoriaId }, new ApiResponse<Categoria>
                {
                    Success = true,
                    Message = "Categoria criada com sucesso.",
                    StatusCode = 201,
                    Data = categoria
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<Categoria>
                {
                    Success = false,
                    Message = $"Erro ao criar a categoria: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Update(int id, Categoria categoria)
        {
            try
            {
                if (id <= 0 || id != categoria.CategoriaId)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID da categoria inválido ou não corresponde ao objeto fornecido.",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var existingCategoria = await _categoriaRepository.GetByIdAsync(id);
                if (existingCategoria == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Categoria não encontrada.",
                        StatusCode = 404,
                        Data = null
                    });
                }

                await _categoriaRepository.UpdateAsync(categoria);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Categoria atualizada com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (KeyNotFoundException)
            {
                return NotFound(new ApiResponse<object>
                {
                    Success = false,
                    Message = "Categoria não encontrada.",
                    StatusCode = 404,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao atualizar a categoria: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult<ApiResponse<object>>> Delete(int id)
        {
            try
            {
                if (id <= 0)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "ID da categoria inválido.",
                        StatusCode = 400,
                        Data = null
                    });
                }

                var categoria = await _categoriaRepository.GetByIdAsync(id);
                if (categoria == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Success = false,
                        Message = "Categoria não encontrada.",
                        StatusCode = 404,
                        Data = null
                    });
                }

                await _categoriaRepository.DeleteAsync(categoria);
                return Ok(new ApiResponse<object>
                {
                    Success = true,
                    Message = "Categoria excluída com sucesso.",
                    StatusCode = 200,
                    Data = null
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ApiResponse<object>
                {
                    Success = false,
                    Message = $"Erro ao excluir a categoria: {ex.Message}",
                    StatusCode = 500,
                    Data = null
                });
            }
        }
    }
}
