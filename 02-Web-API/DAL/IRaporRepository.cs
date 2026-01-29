using Kafa20.DTOs;
using Kafa20.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kafa20.DAL
{
    public interface IRaporRepository
    {
        Task<List<KasaHareket>> GetSatislarByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis);
        Task<decimal> GetToplamTutarByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis);
        Task<int> GetSatisAdediByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis);
        Task<decimal> GetToplamKasaAsync(DateTime baslangic, DateTime bitis);
        Task<List<SaatlikRaporDto>> GetSaatlikHareketlerAsync(DateTime baslangic, DateTime bitis);
    }
}