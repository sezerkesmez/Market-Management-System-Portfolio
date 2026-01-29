using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kafa20.Model
{
    public class Satis
    {
        public int SatisId { get; set; }
        public DateTime Tarih { get; set; }
        public decimal ToplamTutar { get; set; }
        public string OdemeYontemi { get; set; }
        public string ReferansKod { get; set; }
        public List<SatisDetay> Detaylar { get; set; }
    }


}
