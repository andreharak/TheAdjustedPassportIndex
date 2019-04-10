using System;

namespace TheReportLibrary
{
    public class DestinationFinalGrades
    {
        public int Rank { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double HDI { get; set; }
        public double GDP { get; set; }
        public double Area { get; set; }
        public double Tourism { get; set; }
        public double Score { get; set; }

        public DestinationFinalGrades(int rank, string code, string name, double hdi, double gdp, double area, double tourism, double score)
        {
            Rank = rank;
            Code = code;
            Name = name;
            HDI = hdi;
            GDP = gdp;
            Area = area;
            Tourism = tourism;
            Score = score;
        }
    }
}
