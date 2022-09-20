using System;
using System.Collections.Generic;
using System.Text;

namespace PageParser.Entity
{
    public class CarComplectation
    {
        public string ComplectationCode { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }
        public string Engine { get; set; }
        public string Body { get; set; }
        public string Grade { get; set; }
        public string AtmMtm { get; set; }
        public string Gear { get; set; }
        public string GearShiftType { get; set; }
        public string Cab { get; set; }
        public string TransmissionModel { get; set; }
        public string LoadingCapacity { get; set; }
        public string RearTire { get; set; }
        public string Destination { get; set; }
        public string FuelInduction { get; set; }
        public string BuildingCondition { get; set; }
        public string ModelCode { get; set; }
    }
}
