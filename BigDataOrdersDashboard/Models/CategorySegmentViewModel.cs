namespace BigDataOrdersDashboard.Models
{
    public class CategorySegmentViewModel
    {
        public string Category { get; set; }
        public int Total { get; set; }
        public List<YearData> YearlyData { get; set; }
    }

    public class YearData
    {
        public int Year { get; set; }
        public int Count { get; set; }
    }
}
