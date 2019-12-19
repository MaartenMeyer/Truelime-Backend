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

        [BsonElement("Message")]
        public string Message { get; set; }

        [BsonElement("Type")]
        public string Type { get; set; }

        [BsonElement("Author")]
        public string Author { get; set; }

        public int Rating { get; set; }

        public string Color { get; set; }
    }
}