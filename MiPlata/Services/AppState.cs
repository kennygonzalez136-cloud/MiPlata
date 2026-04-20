using MiPlata.Models;

namespace MiPlata.Services
{
    public class AppState
    {
        // Current user stored for the circuit/session
        public Usuario? CurrentUser { get; private set; }

        public void SetUser(Usuario user)
        {
            CurrentUser = user;
        }

        public void Clear()
        {
            CurrentUser = null;
        }
    }
}
