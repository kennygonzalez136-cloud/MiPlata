namespace MiPlata.Models;

public class Usuario
{
    public int Id { get; set; }
    public string Identificacion { get; set; }
    public string NombreCompleto { get; set; }
    public string Celular { get; set; }
    public string Username { get; set; }
    public string Password { get; set; }
    public int IntentosFallidos { get; set; }
    public bool Bloqueado { get; set; }
}


