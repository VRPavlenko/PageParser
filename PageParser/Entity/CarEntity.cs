using System;
using System.Collections.Generic;
using System.Text;

namespace PageParser.Entities
{
    public class CarEntity
    {
        #region 1st lvl Data

        public string Id { get; set; }
        public string Name { get; set; }
        public List<string> Codes { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? FinishDate { get; set; }

        public string SecondLayerDataUrl { get; set; }

        #endregion 1st lvl Data

        #region 2st lvl Data

        List<CarComplectation>

        #endregion 2st lvl Data
    }
}
