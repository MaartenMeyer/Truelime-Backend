namespace TruelimeBackend.Models
{
    public class BoardUser
    {
        public BoardUser(string id, string username)
        {
            Id = id;
            Username = username;
        }

        public string Id { get; set; }
        public string Username { get; set; }
    }
}