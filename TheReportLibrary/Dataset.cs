using System;

namespace TheReportLibrary
{
    public class Dataset1
    {
        public string Passport { get; set; }
        public string Destination { get; set; }
        public int Value { get; set; }
    }

    public class Dataset2
    {
        public string Country_ISO3 { get; set; }
        public string Country_Name { get; set; }
        public string Indicator_Id { get; set; }
        public string Indicator { get; set; }
        public string Subindicator_Type { get; set; }
        public string Y2015 { get; set; }
        public string Y2017 { get; set; }
    }
}
