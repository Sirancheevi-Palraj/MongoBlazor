using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoBlazor.Model
{
    public class TransactionData
    {
        [BsonId]
        [BsonIgnoreIfNull]
        public ObjectId? Id { get; set; }

        public string TrackingId { get; set; }

        public string Status { get; set; }

        public string RequestXML { get; set; }

        public string ResponseXML { get; set; }

        public DateTime RequestDateTime { get; set; }

        public DateTime ResponseDateTime { get; set; }

        public TimeSpan TimeTaken { get; set; }

        public DateTime Timestamp { get; set; }

        public string Message { get; set; }
    }
}
