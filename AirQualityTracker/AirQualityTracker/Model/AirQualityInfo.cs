namespace AirQualityTracker
{
    public class AirQualityInfo
    {
        public DateTime Date { get; set; }
        public double PollutionIndex { get; set; }
        public string? AirQualityStatus { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public double AIPredictionAccuracy { get; set; }
    }
}