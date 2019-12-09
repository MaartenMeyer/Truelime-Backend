using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TruelimeBackend.Models
{
    public class Card
    {
        [BsonId]
        [BsonRepresentation((BsonType.ObjectId))]
        public string Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        [BsonElement("Content")]
        public string Content { get; set; }

        [BsonElement("Position")]
        public string Position { get; set; }
    }
}