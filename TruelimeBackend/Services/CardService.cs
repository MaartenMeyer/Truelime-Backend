using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TruelimeBackend.Models;

namespace TruelimeBackend.Services
{
    public class CardService
    {
        private readonly IMongoCollection<Card> cards;

        public CardService(DatabaseSettings.IDatabaseSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            cards = database.GetCollection<Card>(settings.CardsCollectionName);
        }

        public async Task<Card> Create(Card cardIn)
        {
            await cards.InsertOneAsync(cardIn);
            return cardIn;
        }

        public List<Card> Get() =>
            cards.Find(card => true).ToList();

        public Card Get(string id) =>
            cards.Find<Card>(card => card.Id == id).FirstOrDefault();

        public async Task<Card> Update(string id, Card cardIn) {
            var filter = Builders<Card>.Filter.Eq(card => card.Id, id);
            var update = Builders<Card>.Update
                .Set("Title", cardIn.Title)
                .Set("Message", cardIn.Message);

            await cards.FindOneAndUpdateAsync(filter, update);

            return cards.Find<Card>(card => card.Id == id).FirstOrDefault();
        }

        public void Remove(string id) =>
            cards.DeleteOne(card => card.Id == id);

    }
}