namespace TruelimeBackend.Models
{
    public class DatabaseSettings : DatabaseSettings.IDatabaseSettings {
        public string BoardsCollectionName { get; set; }
        public string LanesCollectionName { get; set; }
        public string CardsCollectionName { get; set; }
        public string UsersCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public interface IDatabaseSettings
        {
            string BoardsCollectionName { get; set; }
            string LanesCollectionName { get; set; }
            string CardsCollectionName { get; set; }
            string UsersCollectionName { get; set; }
            string ConnectionString { get; set; }
            string DatabaseName { get; set; }
        }
    }
}