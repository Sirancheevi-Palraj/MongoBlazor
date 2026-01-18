namespace MongoBlazor.Model
{
    public class CountResult
    {
        public string Label { get; set; } = string.Empty;
        public int Count { get; set; }
    }
    public class TwoValueResult
    {
        public string Label { get; set; } = string.Empty;
        public double Value { get; set; }
    }

    public class TrendPoint
    {
        public string Timestamp { get; set; } = string.Empty;
        public int Count { get; set; }
    }

    public class StatusTrendResult
    {
        public string Period { get; set; } = "";
        public string Status { get; set; } = "";
        public int Count { get; set; }
    }

}
