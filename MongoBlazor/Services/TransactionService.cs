using MongoBlazor.Model;
using MongoDB.Driver;

namespace MongoBlazor.Services
{
    public class TransactionService
    {
        private readonly IMongoCollection<TransactionData> _collection;

        public TransactionService(IConfiguration config)
        {
            var section = config.GetSection("MongoDB");
            var client = new MongoClient(section["ConnectionString"]);
            var db = client.GetDatabase(section["Database"]);
            _collection = db.GetCollection<TransactionData>(section["Collection"]);
        }

        public async Task<(List<TransactionData>, int)> GetTransactionsAsync(
     string search, DateTime? start, DateTime? end,
     string status, int page, int pageSize,
     string sortField = "RequestDateTime",
     bool sortDesc = true)
        {
            var f = Builders<TransactionData>.Filter;
            var filter = f.Empty;

            if (!string.IsNullOrWhiteSpace(search))
                filter &= f.Regex(x => x.TrackingId,
                    new MongoDB.Bson.BsonRegularExpression(search, "i"));

            if (start.HasValue)
                filter &= f.Gte(x => x.RequestDateTime, start.Value);

            if (end.HasValue)
                filter &= f.Lte(x => x.RequestDateTime, end.Value);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                filter &= f.Eq(x => x.Status, status);

            var total = await _collection.CountDocumentsAsync(filter);

            var query = _collection.Find(filter);

            query = (sortField, sortDesc) switch
            {
                ("TrackingId", true) => query.SortByDescending(x => x.TrackingId),
                ("TrackingId", false) => query.SortBy(x => x.TrackingId),

                ("Status", true) => query.SortByDescending(x => x.Status),
                ("Status", false) => query.SortBy(x => x.Status),

                ("ResponseDateTime", true) => query.SortByDescending(x => x.ResponseDateTime),
                ("ResponseDateTime", false) => query.SortBy(x => x.ResponseDateTime),

                ("Timestamp", true) => query.SortByDescending(x => x.Timestamp),
                ("Timestamp", false) => query.SortBy(x => x.Timestamp),

                _ when sortDesc => query.SortByDescending(x => x.RequestDateTime),
                _ => query.SortBy(x => x.RequestDateTime)
            };

            var records = await query
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            return (records, (int)total);
        }


        public async Task<List<TransactionData>> GetAllForExportAsync(
            string trackingId, DateTime? start, DateTime? end, string status)
        {
            var f = Builders<TransactionData>.Filter;
            var filter = f.Empty;

            if (!string.IsNullOrWhiteSpace(trackingId))
                filter &= f.Regex(x => x.TrackingId, new MongoDB.Bson.BsonRegularExpression(trackingId, "i"));

            if (start.HasValue)
                filter &= f.Gte(x => x.RequestDateTime, start.Value);

            if (end.HasValue)
                filter &= f.Lte(x => x.RequestDateTime, end.Value);

            if (!string.IsNullOrWhiteSpace(status) && status != "All")
                filter &= f.Eq(x => x.Status, status);

            return await _collection
                .Find(filter)
                .SortByDescending(x => x.RequestDateTime)
                .ToListAsync();
        }
    }
}
