using System;

namespace TheReportLibrary
{
    public class PassportFinalScore
    {
        public int Rank { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public double Score { get; set; }
        public int VisaFree { get; set; }
        public int VisaOnArrival { get; set; }
        public int eTArequired { get; set; }

        public PassportFinalScore(int rank, string code, string name, double score, int visaFree, int visaOnArrival, int ETArequired)
        {
            Rank = rank;
            Code = code;
            Name = name;
            Score = score;
            VisaFree = visaFree;
            VisaOnArrival = visaOnArrival;
            eTArequired = ETArequired;
        }
    }
}
