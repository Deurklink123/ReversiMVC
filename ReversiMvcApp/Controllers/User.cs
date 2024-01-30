namespace ReversiMvcApp.Controllers
{
    public class User
    {
        private string Speler1Token;
        private string Omschrijving;

        public User(string v1, string v2)
        {
            Speler1Token = v1;
            Omschrijving = v2;
        }
    }
}