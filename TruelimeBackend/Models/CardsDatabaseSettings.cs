namespace TruelimeBackend.Models
{
    public class CardsDatabaseSettings : CardsDatabaseSettings.ICardsDatabaseSettings {
        public string CardsCollectionName { get; set; }
        public string ConnectionString { get; set; }
        public string DatabaseName { get; set; }

        public interface ICardsDatabaseSettings
        {
            string CardsCollectionName { get; set; }
            string ConnectionString { get; set; }
            string DatabaseName { get; set; }
        }
    }
}