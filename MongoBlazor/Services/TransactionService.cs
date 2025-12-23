using Microsoft.Extensions.Options;
using MongoBlazor.Model;
using MongoDB.Driver;

namespace MongoBlazor.Services
{
    public class TransactionService
    {
        private readonly IMongoCollection<TransactionData> _collection;

        public TransactionService(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            var database = client.GetDatabase(settings.Value.Database);
            _collection = database.GetCollection<TransactionData>(settings.Value.Collection);
        }

        public async Task<List<TransactionData>> SearchAsync(
            string trackingId,
            DateTime? fromDate,
            DateTime? toDate,
            string status)
        {
            var filter = Builders<TransactionData>.Filter.Empty;

            if (!string.IsNullOrWhiteSpace(trackingId))
                filter &= Builders<TransactionData>.Filter.Regex(
                    x => x.TrackingId, new MongoDB.Bson.BsonRegularExpression(trackingId, "i"));

            if (fromDate.HasValue)
                filter &= Builders<TransactionData>.Filter.Gte(x => x.Timestamp, fromDate.Value);

            if (toDate.HasValue)
                filter &= Builders<TransactionData>.Filter.Lte(x => x.Timestamp, toDate.Value);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                filter &= Builders<TransactionData>.Filter.Eq(x => x.Status, status);

            return await _collection.Find(filter)
                .SortByDescending(x => x.Timestamp)
                .Limit(5000)
                .ToListAsync();
        }
    }
}
