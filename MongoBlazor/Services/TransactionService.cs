using MongoBlazor.Model;
using MongoDB.Bson;
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
       
        public async Task<List<CountResult>> GetStatusDistributionAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Group(x => x.Status,
                        g => new CountResult
                        {
                            Label = g.Key,
                            Count = g.Count()
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetStatusDistributionAsync] {ex}");
                return new();
            }
        }
        public async Task<List<CountResult>> GetDailyCountsAsync(DateTime? start, DateTime? end)
        {
            try
            {
                var f = Builders<TransactionData>.Filter.Empty;
                if (start.HasValue) f &= Builders<TransactionData>.Filter.Gte(x => x.RequestDateTime, start.Value);
                if (end.HasValue) f &= Builders<TransactionData>.Filter.Lte(x => x.RequestDateTime, end.Value);

                return await _collection.Aggregate()
                   // .Match(f)
                    .Group(x => x.RequestDateTime.Date,
                        g => new CountResult
                        {
                            Label = g.Key.ToString(),
                            Count = g.Count()
                        })
                    .SortBy(x => x.Label)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetDailyCountsAsync] {ex}");
                return new();
            }
        }
        public async Task<List<CountResult>> GetMonthlyCountsAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Project(x => new
                    {
                        Month = x.RequestDateTime.ToString("yyyy-MM")
                    })
                    .Group(x => x.Month,
                        g => new CountResult
                        {
                            Label = g.Key,
                            Count = g.Count()
                        })
                    .SortBy(x => x.Label)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetMonthlyCountsAsync] {ex}");
                return new();
            }
        }
        public async Task<List<TrendPoint>> GetTrendAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Project(x => new
                    {
                        Key = x.RequestDateTime.ToString("yyyy-MM-dd HH:00")
                    })
                    .Group(x => x.Key,
                        g => new TrendPoint
                        {
                            Timestamp = g.Key,
                            Count = g.Count()
                        })
                    .SortBy(x => x.Timestamp)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetTrendAsync] {ex}");
                return new();
            }
        }
        //public async Task<List<TwoValueResult>> GetAmountByStatusAsync()
        //{
        //    try
        //    {
        //        return await _collection.Aggregate()
        //            .Group(x => x.Status,
        //                g => new TwoValueResult
        //                {
        //                    Label = g.Key,
        //                    Value = g.Average(v => v.)
        //                })
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetAmountByStatusAsync] {ex}");
        //        return new();
        //    }
        //}
        public async Task<List<CountResult>> GetResponseTimeBucketsAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Project(x => new
                    {
                        Seconds = x.TimeTaken.TotalSeconds
                    })
                    .Group(x =>
                        x.Seconds < 1 ? "<1s" :
                        x.Seconds < 2 ? "1-2s" :
                        x.Seconds < 3 ? "2-3s" :
                        ">3s",
                        g => new CountResult
                        {
                            Label = g.Key,
                            Count = g.Count()
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetResponseTimeBucketsAsync] {ex}");
                return new();
            }
        }
        public async Task<List<TwoValueResult>> GetSuccessRateByHourAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Group(x => x.RequestDateTime.Hour,
                        g => new TwoValueResult
                        {
                            Label = $"{g.Key}:00",
                            Value = (double)g.Count(t => t.Status == "SUCCESS") / g.Count() * 100
                        })
                    .SortBy(x => x.Label)
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetSuccessRateByHourAsync] {ex}");
                return new();
            }
        }
        public async Task<List<CountResult>> GetPendingAgingAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Match(x => x.Status == "PENDING")
                    .Project(x => new
                    {
                        Seconds = x.TimeTaken.TotalSeconds
                    })
                    .Group(x =>
                        x.Seconds < 1 ? "<1s" :
                        x.Seconds < 2 ? "1-2s" :
                        x.Seconds < 3 ? "2-3s" :
                        ">3s",
                        g => new CountResult
                        {
                            Label = g.Key,
                            Count = g.Count()
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetPendingAgingAsync] {ex}");
                return new();
            }
        }
        //public async Task<List<CountResult>> GetAmountBucketsAsync()
        //{
        //    try
        //    {
        //        return await _collection.Aggregate()
        //            .Group(x =>
        //                x.Amount < 1000 ? "0-1000" :
        //                x.Amount < 2000 ? "1000-2000" :
        //                x.Amount < 3000 ? "2000-3000" :
        //                "3000+",
        //                g => new CountResult
        //                {
        //                    Label = g.Key,
        //                    Count = g.Count()
        //                })
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetAmountBucketsAsync] {ex}");
        //        return new();
        //    }
        //}
        //public async Task<List<TwoValueResult>> GetTopCustomersAsync(int top = 10)
        //{
        //    try
        //    {
        //        return await _collection.Aggregate()
        //            .Group(x => x.Customer.CustomerId,
        //                g => new TwoValueResult
        //                {
        //                    Label = g.Key,
        //                    Value = g.Sum(t => t.Amount)
        //                })
        //            .SortByDescending(x => x.Value)
        //            .Limit(top)
        //            .ToListAsync();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[GetTopCustomersAsync] {ex}");
        //        return new();
        //    }
        //}
        public async Task<List<CountResult>> GetFailureReasonsAsync()
        {
            try
            {
                return await _collection.Aggregate()
                    .Match(x => x.Status == "FAILED")
                    .Group(x => x.Message,
                        g => new CountResult
                        {
                            Label = g.Key,
                            Count = g.Count()
                        })
                    .ToListAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[GetFailureReasonsAsync] {ex}");
                return new();
            }
        }
        public async Task<List<StatusTrendResult>> GetStatusDailyAsync()
        {
            return await _collection.Aggregate()
                .Group(x => new { Day = x.RequestDateTime.Date, x.Status },
                    g => new StatusTrendResult
                    {
                        Period = g.Key.Day.ToString("yyyy-MM-dd"),
                        Status = g.Key.Status,
                        Count = g.Count()
                    })
                .SortBy(x => x.Period)
                .ToListAsync();
        }
        public async Task<List<StatusTrendResult>> GetStatusWeeklyAsync()
        {
            return await _collection.Aggregate()
                .Project(x => new
                {
                    Week = MongoDB.Bson.BsonDateTime.Create(
                        x.RequestDateTime.AddDays(-(int)x.RequestDateTime.DayOfWeek)
                    ).ToUniversalTime().ToString("yyyy-MM-dd"),
                    x.Status
                })
                .Group(x => new { x.Week, x.Status },
                    g => new StatusTrendResult
                    {
                        Period = g.Key.Week,
                        Status = g.Key.Status,
                        Count = g.Count()
                    })
                .SortBy(x => x.Period)
                .ToListAsync();
        }
        public async Task<List<StatusTrendResult>> GetStatusMonthlyAsync()
        {
            return await _collection.Aggregate()
                .Project(x => new
                {
                    Month = x.RequestDateTime.ToString("yyyy-MM"),
                    x.Status
                })
                .Group(x => new { x.Month, x.Status },
                    g => new StatusTrendResult
                    {
                        Period = g.Key.Month,
                        Status = g.Key.Status,
                        Count = g.Count()
                    })
                .SortBy(x => x.Period)
                .ToListAsync();
        }


    }

}
