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
        public DateTime? ResponseDateTime { get; set; }   // <-- Nullable

        public TimeSpan TimeTaken { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        // ✅ Your DB has ResponseTimeMs (int)
        public int? ResponseTimeMs { get; set; }

        // ✅ Optional extra fields (exists in your JSON)
        public int? HttpStatus { get; set; }
        public string? Source { get; set; }
        public string? Endpoint { get; set; }
        public string? Application { get; set; }
        public string? Environment { get; set; }
        public string? RequestedBy { get; set; }

        // ⚠️ TimeSpan can cause issues in Mongo queries (keep nullable)
     
     
    }
}
