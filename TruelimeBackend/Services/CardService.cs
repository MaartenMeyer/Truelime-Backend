using System.Collections.Generic;
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

        public Card Create(Card card)
        {
            cards.InsertOne(card);
            return card;
        }

        public List<Card> Get() =>
            cards.Find(card => true).ToList();

        public Card Get(string id) =>
            cards.Find<Card>(card => card.Id == id).FirstOrDefault();

        public void Update(string id, Card cardIn) =>
            cards.ReplaceOne(card => card.Id == id, cardIn);

        public void Remove(Card cardIn) =>
            cards.DeleteOne(card => card.Id == cardIn.Id);

        public void Remove(string id) =>
            cards.DeleteOne(card => card.Id == id);

    }
}