using System.Collections.Generic;
using System.Collections.ObjectModel;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace TruelimeBackend.Models
{
    public class Lane
    {
        [BsonId]
        [BsonRepresentation((BsonType.ObjectId))]
        public string Id { get; set; }

        [BsonElement("Title")]
        public string Title { get; set; }

        public List<Card> Cards { get; set; }
    }
}