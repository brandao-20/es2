namespace ES2_TP_ComparadorPrecos_BACKEND.models;

public class Utilizador
{
    public int UtilizadorId { get; set; }
    public string Username { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Email { get; set; } = null!;
}