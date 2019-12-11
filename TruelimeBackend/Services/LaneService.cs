using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TruelimeBackend.Models;

namespace TruelimeBackend.Services
{
    public class LaneService {
        private readonly IMongoCollection<Lane> lanes;

        public LaneService(DatabaseSettings.IDatabaseSettings settings) {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            lanes = database.GetCollection<Lane>(settings.LanesCollectionName);
        }

        public async Task<Lane> Create(Lane laneIn)
        {
            laneIn.Cards = new List<Card>();
            await lanes.InsertOneAsync(laneIn);
            return laneIn;
        }

        public List<Lane> Get() =>
            lanes.Find(lane => true).ToList();

        public Lane Get(string id) =>
            lanes.Find<Lane>(lane => lane.Id == id).FirstOrDefault();

        public void Update(string id, Lane laneIn) =>
            lanes.ReplaceOne(lane => lane.Id == id, laneIn);

        public void Remove(Lane laneIn) =>
            lanes.DeleteOne(lane => lane.Id == laneIn.Id);

        public void Remove(string id) =>
            lanes.DeleteOne(lane => lane.Id == id);

        public async Task<Lane> AddCard(string id, Card cardIn) {
            var filter = Builders<Lane>.Filter.Eq(lane => lane.Id, id);
            var update = Builders<Lane>.Update.Push(lane => lane.Cards, cardIn);

            await lanes.FindOneAndUpdateAsync(filter, update);

            return lanes.Find<Lane>(lane => lane.Id == id).FirstOrDefault();
        }

        public async Task RemoveCard(string id, Card cardIn) {
            var filter = Builders<Lane>.Filter.Eq(card => card.Id, id);
            var update = Builders<Lane>.Update.PullFilter("Cards", Builders<Card>.Filter.Eq(card => card.Id, cardIn.Id));

            await lanes.FindOneAndUpdateAsync(filter, update);
        }
    }
}