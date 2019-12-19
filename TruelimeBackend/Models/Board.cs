using System.Collections.Generic;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TruelimeBackend.Models
{
    public class Board
    {
        [BsonId]
        [BsonRepresentation((BsonType.ObjectId))]
        public string Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Description")]
        public string Description { get; set; }

        [BsonElement("Owner")]
        public BoardUser Owner { get; set; }

        public List<string> Colors { get; set; }

        public List<Lane> Lanes { get; set; }
    }
}