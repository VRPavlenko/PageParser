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
        public string ModelCode { get; set; }
    }
}
