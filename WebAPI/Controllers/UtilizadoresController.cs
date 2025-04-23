using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Linq;
using WebAPI.Entities;
using WebAPI.Helpers;
using WebAPI.Repositories;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UtilizadoresController : ControllerBase
    {
        private readonly IUtilizadorRepository _utilizadorRepository;
        private readonly ITipoUtilizadorRepository _tipoUtilizadorRepository;

        public UtilizadoresController(
            IUtilizadorRepository utilizadorRepository,
            ITipoUtilizadorRepository tipoUtilizadorRepository)
        {
            _utilizadorRepository = utilizadorRepository;
            _tipoUtilizadorRepository = tipoUtilizadorRepository;
        }

        [HttpGet("check")]
        public async Task<ActionResult<ApiResponse<CheckAvailabilityResponse>>> CheckAvailability([FromQuery] string username, [FromQuery] string email)
        {
            if (string.IsNullOrWhiteSpace(username) && string.IsNullOrWhiteSpace(email))
                return BadRequest(ApiResponse<CheckAvailabilityResponse>.ErrorResponse("Pelo menos um parâmetro (username ou email) deve ser fornecido.", "INVALID_PARAMETERS"));

            bool usernameExists = false;
            bool emailExists = false;

            if (!string.IsNullOrWhiteSpace(username))
            {
                var existingUsername = await _utilizadorRepository.FindAsync(u => u.Username.ToLower() == username.ToLower());
                usernameExists = existingUsername.Any();
            }

            if (!string.IsNullOrWhiteSpace(email))
            {
                var existingEmail = await _utilizadorRepository.FindAsync(u => u.Email.ToLower() == email.ToLower());
                emailExists = existingEmail.Any();
            }

            var response = new CheckAvailabilityResponse
            {
                UsernameExists = usernameExists,
                EmailExists = emailExists
            };

            return Ok(ApiResponse<CheckAvailabilityResponse>.SuccessResponse(response, "Verificação concluída."));
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<Utilizador>>> Register(Utilizador utilizador)
        {
            // Verificar duplicatas
            var usernameExists = await _utilizadorRepository.FindAsync(u => u.Username.ToLower() == utilizador.Username.ToLower());
            if (usernameExists.Any())
                return BadRequest(ApiResponse<Utilizador>.ErrorResponse("O nome de utilizador já está em uso.", "DUPLICATE_USERNAME"));

            var emailExists = await _utilizadorRepository.FindAsync(u => u.Email.ToLower() == utilizador.Email.ToLower());
            if (emailExists.Any())
                return BadRequest(ApiResponse<Utilizador>.ErrorResponse("O email já está em uso.", "DUPLICATE_EMAIL"));

            // Definir o tipo de utilizador
            var tiposExistentes = await _utilizadorRepository.GetAllAsync();
            if (!tiposExistentes.Any())
            {
                var adminTipo = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo.ToLower() == "admin")).FirstOrDefault();
                if (adminTipo == null)
                {
                    adminTipo = new TipoUtilizador { Tipo = "Admin" };
                    await _tipoUtilizadorRepository.AddAsync(adminTipo);
                }
                utilizador.TipoUtilizadorId = adminTipo.TipoUtilizadorId;
            }
            else
            {
                var userTipo = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo.ToLower() == "user")).FirstOrDefault();
                if (userTipo == null)
                {
                    userTipo = new TipoUtilizador { Tipo = "User" };
                    await _tipoUtilizadorRepository.AddAsync(userTipo);
                }
                utilizador.TipoUtilizadorId = userTipo.TipoUtilizadorId;
            }

            utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            utilizador.DataCriacao = DateTime.UtcNow;
            await _utilizadorRepository.AddAsync(utilizador);

            return CreatedAtAction(nameof(GetById), new { id = utilizador.UtilizadorId }, ApiResponse<Utilizador>.SuccessResponse(utilizador, "Utilizador criado com sucesso."));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Utilizador>> GetById(int id)
        {
            var user = await _utilizadorRepository.GetByIdWithDetailsAsync(id);
            if (user == null) return NotFound();
            return Ok(user);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Utilizador>>> GetAll([FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 5;

            var totalItems = await _utilizadorRepository.CountAsync();
            var skip = (page - 1) * pageSize;
            var users = await _utilizadorRepository.GetPagedWithDetailsAsync(skip, pageSize);

            Response.Headers["X-Total-Count"] = totalItems.ToString();
            return Ok(users);
        }

        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, Utilizador utilizador)
        {
            if (id != utilizador.UtilizadorId) return BadRequest();

            var existingUser = await _utilizadorRepository.GetByIdAsync(id);
            if (existingUser != null && utilizador.Password != existingUser.Password)
            {
                utilizador.Password = PasswordHelper.HashPassword(utilizador.Password);
            }

            try
            {
                await _utilizadorRepository.UpdateAsync(utilizador);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            return NoContent();
        }

        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> Delete(int id)
        {
            var userIdClaim = User.FindFirst("utilizadorId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int currentUserId) && id == currentUserId)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Não é possível remover a si próprio por este endpoint.", "INVALID_ACTION"));
            }

            var user = await _utilizadorRepository.GetByIdAsync(id);
            if (user == null) return NotFound(ApiResponse<object>.ErrorResponse("Utilizador não encontrado.", "NOT_FOUND"));

            await _utilizadorRepository.DeleteAsync(user);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Utilizador removido com sucesso."));
        }

        [HttpDelete("me")]
        [Authorize]
        public async Task<IActionResult> DeleteMyAccount()
        {
            var userIdClaim = User.FindFirst("utilizadorId");
            if (userIdClaim == null) return Unauthorized(ApiResponse<object>.ErrorResponse("Usuário não identificado.", "UNAUTHORIZED"));

            if (!int.TryParse(userIdClaim.Value, out int userId))
                return BadRequest(ApiResponse<object>.ErrorResponse("ID do usuário inválido.", "INVALID_USER_ID"));

            var user = await _utilizadorRepository.GetByIdAsync(userId);
            if (user == null) return NotFound(ApiResponse<object>.ErrorResponse("Utilizador não encontrado.", "NOT_FOUND"));

            await _utilizadorRepository.DeleteAsync(user);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Conta removida com sucesso."));
        }

        [HttpPost("admincreate")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<ActionResult<ApiResponse<Utilizador>>> AdminCreate(UserDto dto)
        {
            // Verificar duplicatas
            var usernameExists = await _utilizadorRepository.FindAsync(u => u.Username.ToLower() == dto.Username.ToLower());
            if (usernameExists.Any())
                return BadRequest(ApiResponse<Utilizador>.ErrorResponse("O nome de utilizador já está em uso.", "DUPLICATE_USERNAME"));

            var emailExists = await _utilizadorRepository.FindAsync(u => u.Email.ToLower() == dto.Email.ToLower());
            if (emailExists.Any())
                return BadRequest(ApiResponse<Utilizador>.ErrorResponse("O email já está em uso.", "DUPLICATE_EMAIL"));

            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo == roleDb)).FirstOrDefault();
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                await _tipoUtilizadorRepository.AddAsync(tipoEntity);
            }

            var user = new Utilizador
            {
                Username = dto.Username,
                Email = dto.Email,
                Password = PasswordHelper.HashPassword(dto.Password),
                TipoUtilizadorId = tipoEntity.TipoUtilizadorId,
                DataCriacao = DateTime.UtcNow
            };

            await _utilizadorRepository.AddAsync(user);
            return Ok(ApiResponse<Utilizador>.SuccessResponse(user, "Utilizador criado com sucesso."));
        }

        [HttpGet("adminget/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<ActionResult<ApiResponse<UserDto>>> AdminGetUser(int id)
        {
            var user = await _utilizadorRepository.GetByIdWithDetailsAsync(id);
            if (user == null) return NotFound(ApiResponse<UserDto>.ErrorResponse("Utilizador não encontrado.", "NOT_FOUND"));

            var roleDb = user.TipoUtilizador?.Tipo ?? "USER";
            var dto = new UserDto
            {
                UtilizadorId = user.UtilizadorId,
                Username = user.Username,
                Email = user.Email,
                Tipo = NormalizeRoleFromDb(roleDb)
            };
            return Ok(ApiResponse<UserDto>.SuccessResponse(dto, "Utilizador obtido com sucesso."));
        }

        [HttpPut("adminedit/{id:int}")]
        [Authorize(Roles = "Admin,UserManager")]
        public async Task<IActionResult> AdminEditUser(int id, UserDto dto)
        {
            var user = await _utilizadorRepository.GetByIdAsync(id);
            if (user == null) return NotFound(ApiResponse<object>.ErrorResponse("Utilizador não encontrado.", "NOT_FOUND"));

            // Verificar duplicatas (exceto para o próprio utilizador)
            var usernameExists = await _utilizadorRepository.FindAsync(u => u.Username.ToLower() == dto.Username.ToLower() && u.UtilizadorId != id);
            if (usernameExists.Any())
                return BadRequest(ApiResponse<object>.ErrorResponse("O nome de utilizador já está em uso.", "DUPLICATE_USERNAME"));

            var emailExists = await _utilizadorRepository.FindAsync(u => u.Email.ToLower() == dto.Email.ToLower() && u.UtilizadorId != id);
            if (emailExists.Any())
                return BadRequest(ApiResponse<object>.ErrorResponse("O email já está em uso.", "DUPLICATE_EMAIL"));

            user.Username = dto.Username;
            user.Email = dto.Email;

            if (!string.IsNullOrEmpty(dto.NewPassword))
            {
                user.Password = PasswordHelper.HashPassword(dto.NewPassword);
            }

            var roleDb = dto.Tipo.ToUpper();
            var tipoEntity = (await _tipoUtilizadorRepository.FindAsync(t => t.Tipo == roleDb)).FirstOrDefault();
            if (tipoEntity == null)
            {
                tipoEntity = new TipoUtilizador { Tipo = roleDb };
                await _tipoUtilizadorRepository.AddAsync(tipoEntity);
            }
            user.TipoUtilizadorId = tipoEntity.TipoUtilizadorId;

            await _utilizadorRepository.UpdateAsync(user);
            return Ok(ApiResponse<object>.SuccessResponse(null, "Utilizador atualizado com sucesso."));
        }

        private string NormalizeRoleFromDb(string roleDb)
        {
            switch (roleDb.ToUpper())
            {
                case "ADMIN": return "Admin";
                case "USER_MANAGER": return "UserManager";
                default: return "User";
            }
        }
    }

    public class UserDto
    {
        public int UtilizadorId { get; set; }
        public string Username { get; set; } = "";
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public string Tipo { get; set; } = "User";
        public string? NewPassword { get; set; }
    }

    public class CheckAvailabilityResponse
    {
        public bool UsernameExists { get; set; }
        public bool EmailExists { get; set; }
    }
}
