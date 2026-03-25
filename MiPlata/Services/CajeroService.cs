using Microsoft.Data.SqlClient;
using MiPlata.Models;

public class CajeroService
{
    private string connectionString = "Server=localhost\\SQLEXPRESS;Database=MiplataBD;Trusted_Connection=True;TrustServerCertificate=True;";

    public bool TieneCuentas(int usuarioId)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "SELECT COUNT(*) FROM Cuentas WHERE UsuarioId=@u";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", usuarioId);

            int count = (int)cmd.ExecuteScalar();

            return count > 0;
        }
    }

    public string CrearCuenta(int usuarioId, string tipoCuenta)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string check = "SELECT COUNT(*) FROM Cuentas WHERE UsuarioId=@u AND TipoCuenta=@t";
            SqlCommand cmdCheck = new SqlCommand(check, conn);
            cmdCheck.Parameters.AddWithValue("@u", usuarioId);
            cmdCheck.Parameters.AddWithValue("@t", tipoCuenta);

            int existe = (int)cmdCheck.ExecuteScalar();

            if (existe > 0)
                return "Ya tienes una cuenta de este tipo";

            string query = @"INSERT INTO Cuentas (UsuarioId, TipoCuenta, Saldo)
                         VALUES (@u, @t, 0)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@u", usuarioId);
            cmd.Parameters.AddWithValue("@t", tipoCuenta);

            cmd.ExecuteNonQuery();

            return "Cuenta creada correctamente";
        }
    }
    // 🔹 REGISTRO
    public string Registrar(Usuario user)
    {
        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            // validar usuario único
            string check = "SELECT COUNT(*) FROM Usuarios WHERE Username=@user";
            SqlCommand cmdCheck = new SqlCommand(check, conn);
            cmdCheck.Parameters.AddWithValue("@user", user.Username);

            int existe = (int)cmdCheck.ExecuteScalar();

            if (existe > 0)
                return "El usuario ya existe";

            string query = @"INSERT INTO Usuarios 
            (Identificacion, NombreCompleto, Celular, Username, Password)
            VALUES (@id, @nombre, @cel, @user, @pass)";

            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@id", user.Identificacion);
            cmd.Parameters.AddWithValue("@nombre", user.NombreCompleto);
            cmd.Parameters.AddWithValue("@cel", user.Celular);
            cmd.Parameters.AddWithValue("@user", user.Username);
            cmd.Parameters.AddWithValue("@pass", user.Password);

            cmd.ExecuteNonQuery();

            return "Usuario registrado correctamente";
        }
    }

    // 🔹 LOGIN
    public Usuario Login(string username, string password, out string mensaje, out int intentosRestantes)
    {
        mensaje = "";
        intentosRestantes = 0;

        using (SqlConnection conn = new SqlConnection(connectionString))
        {
            conn.Open();

            string query = "SELECT * FROM Usuarios WHERE Username=@user";
            SqlCommand cmd = new SqlCommand(query, conn);
            cmd.Parameters.AddWithValue("@user", username);

            SqlDataReader reader = cmd.ExecuteReader();

            if (!reader.Read())
            {
                mensaje = "Usuario no existe";
                return null;
            }

            Usuario user = new Usuario
            {
                Id = (int)reader["Id"],
                Username = reader["Username"].ToString(),
                Password = reader["Password"].ToString(),
                IntentosFallidos = (int)reader["IntentosFallidos"],
                Bloqueado = (bool)reader["Bloqueado"]
            };

            reader.Close();

            if (user.Bloqueado)
            {
                mensaje = "Cuenta bloqueada";
                return null;
            }

            if (user.Password != password)
            {
                user.IntentosFallidos++;

                if (user.IntentosFallidos >= 3)
                {
                    string bloque = "UPDATE Usuarios SET IntentosFallidos=3, Bloqueado=1 WHERE Id=@id";
                    SqlCommand cmdBloq = new SqlCommand(bloque, conn);
                    cmdBloq.Parameters.AddWithValue("@id", user.Id);
                    cmdBloq.ExecuteNonQuery();

                    mensaje = "Cuenta bloqueada por intentos fallidos";
                    return null;
                }
                else
                {
                    string update = "UPDATE Usuarios SET IntentosFallidos=@i WHERE Id=@id";
                    SqlCommand cmdUp = new SqlCommand(update, conn);
                    cmdUp.Parameters.AddWithValue("@i", user.IntentosFallidos);
                    cmdUp.Parameters.AddWithValue("@id", user.Id);
                    cmdUp.ExecuteNonQuery();

                    intentosRestantes = 3 - user.IntentosFallidos;
                    mensaje = "Contraseña incorrecta";
                    return null;
                }
            }

            // login correcto
            string reset = "UPDATE Usuarios SET IntentosFallidos=0 WHERE Id=@id";
            SqlCommand cmdReset = new SqlCommand(reset, conn);
            cmdReset.Parameters.AddWithValue("@id", user.Id);
            cmdReset.ExecuteNonQuery();

            mensaje = "Login exitoso";
            return user;
        }
    }
}