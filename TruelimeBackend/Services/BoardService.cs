using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Driver;
using TruelimeBackend.Models;

namespace TruelimeBackend.Services
{
    public class BoardService
    {
        private readonly IMongoCollection<Board> boards;

        public BoardService(DatabaseSettings.IDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            boards = database.GetCollection<Board>(settings.BoardsCollectionName);
        }

        public Board Create(Board board)
        {
            board.Lanes = new List<Lane>();
            boards.InsertOne(board);
            return board;
        }

        public List<Board> Get() =>
            boards.Find(board => true).ToList();

        public Board Get(string id) =>
            boards.Find<Board>(board => board.Id == id).FirstOrDefault();

        public void Update(string id, Board boardIn) =>
            boards.ReplaceOne(board => boardIn.Id == id, boardIn);

        public void Remove(Board boardIn) =>
            boards.DeleteOne(board => board.Id == boardIn.Id);

        public void Remove(string id) =>
            boards.DeleteOne(board => board.Id == id);

        public async Task AddLane(string id, Lane laneIn)
        {
            var filter = Builders<Board>.Filter.Eq(board => board.Id, id);
            var update = Builders<Board>.Update.Push(board => board.Lanes, laneIn);

            await boards.FindOneAndUpdateAsync(filter, update);
        }

        public async Task RemoveLane(string id, Lane laneIn) {
            var filter = Builders<Board>.Filter.Eq(board => board.Id, id);
            var update = Builders<Board>.Update.PullFilter("Lanes", Builders<Lane>.Filter.Eq(lane => lane.Id, laneIn.Id));

            await boards.FindOneAndUpdateAsync(filter, update);
        }
    }
}