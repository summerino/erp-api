using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ERP.Web.API.Domain.Models.Mobile.Sales
{
    public class SalesTracking
    {
        public decimal Lat { get; set; }

        public decimal Lng { get; set; }

        public DateTime TrackedDate { get; set; }
    }
}
