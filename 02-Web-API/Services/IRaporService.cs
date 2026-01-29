using Kafa20.DTOs;
using Kafa20.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Kafa20.Services
{
    public interface IRaporService
    {
        Task<List<SatisRaporDto>> GetNakitSatislarAsync(DateTime baslangic, DateTime bitis);
        Task<ToplamRaporDto> GetNakitRaporAsync(DateTime baslangic, DateTime bitis);
        Task<List<SatisRaporDto>> GetKartSatislarAsync(DateTime baslangic, DateTime bitis);
        Task<ToplamRaporDto> GetKartRaporAsync(DateTime baslangic, DateTime bitis);
        Task<ToplamRaporDto> GetToplamKasaRaporAsync(DateTime baslangic, DateTime bitis);
        Task<List<SaatlikRaporDto>> GetSaatlikHareketlerAsync(DateTime tarih);
        Task<MarketSettings> GetMarketSettingsAsync();
    }
}