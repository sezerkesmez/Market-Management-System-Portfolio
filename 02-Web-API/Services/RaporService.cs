using Kafa20.DAL;
using Kafa20.Data;
using Kafa20.DTOs;
using Kafa20.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kafa20.Services
{
    public class RaporService : IRaporService
    {
        private readonly IRaporRepository _repository;
        private readonly AppDbContext _context;

        public RaporService(IRaporRepository repository, AppDbContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<List<SatisRaporDto>> GetNakitSatislarAsync(DateTime baslangic, DateTime bitis)
        {
            var satislar = await _repository.GetSatislarByOdemeTipiAsync("Nakit", baslangic, bitis);
            return satislar.Select(x => new SatisRaporDto
            {
                HareketId = x.HareketId,
                Tarih = x.Tarih,
                Tutar = x.Tutar,
                OdemeTipi = x.OdemeTipi,
                Aciklama = x.Aciklama,
                IlgiliFaturaNo = x.IlgiliFaturaNo
            }).ToList();
        }

        public async Task<ToplamRaporDto> GetNakitRaporAsync(DateTime baslangic, DateTime bitis)
        {
            var toplam = await _repository.GetToplamTutarByOdemeTipiAsync("Nakit", baslangic, bitis);
            var adet = await _repository.GetSatisAdediByOdemeTipiAsync("Nakit", baslangic, bitis);
            return new ToplamRaporDto
            {
                ToplamTutar = toplam,
                SatisAdedi = adet,
                BaslangicTarihi = baslangic,
                BitisTarihi = bitis
            };
        }

        public async Task<List<SatisRaporDto>> GetKartSatislarAsync(DateTime baslangic, DateTime bitis)
        {
            var satislar = await _repository.GetSatislarByOdemeTipiAsync("Kart", baslangic, bitis);
            return satislar.Select(x => new SatisRaporDto
            {
                HareketId = x.HareketId,
                Tarih = x.Tarih,
                Tutar = x.Tutar,
                OdemeTipi = x.OdemeTipi,
                Aciklama = x.Aciklama,
                IlgiliFaturaNo = x.IlgiliFaturaNo
            }).ToList();
        }

        public async Task<ToplamRaporDto> GetKartRaporAsync(DateTime baslangic, DateTime bitis)
        {
            var toplam = await _repository.GetToplamTutarByOdemeTipiAsync("Kart", baslangic, bitis);
            var adet = await _repository.GetSatisAdediByOdemeTipiAsync("Kart", baslangic, bitis);
            return new ToplamRaporDto
            {
                ToplamTutar = toplam,
                SatisAdedi = adet,
                BaslangicTarihi = baslangic,
                BitisTarihi = bitis
            };
        }

        public async Task<ToplamRaporDto> GetToplamKasaRaporAsync(DateTime baslangic, DateTime bitis)
        {
            var toplam = await _repository.GetToplamKasaAsync(baslangic, bitis);
            var adet = await _repository.GetSatisAdediByOdemeTipiAsync(null, baslangic, bitis);
            return new ToplamRaporDto
            {
                ToplamTutar = toplam,
                SatisAdedi = adet,
                BaslangicTarihi = baslangic,
                BitisTarihi = bitis
            };
        }

        public async Task<List<SaatlikRaporDto>> GetSaatlikHareketlerAsync(DateTime tarih)
        {
            var settings = await _context.MarketSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                throw new Exception("İşletme saatleri tanımlı değil.");
            }

            var baslangic = tarih.Date + settings.OpeningTime;
            var bitis = settings.IsNextDay ? tarih.Date.AddDays(1) + settings.ClosingTime : tarih.Date + settings.ClosingTime;

            var hareketler = await _repository.GetSaatlikHareketlerAsync(baslangic, bitis);

            var result = new List<SaatlikRaporDto>();
            var startHour = settings.OpeningTime.Hours;
            var endHour = settings.ClosingTime.Hours + (settings.IsNextDay ? 24 : 0);

            for (int hour = startHour; hour <= endHour; hour++)
            {
                var normalizedHour = hour % 24;
                var hareket = hareketler.FirstOrDefault(h => h.Saat == normalizedHour) ?? new SaatlikRaporDto
                {
                    Saat = normalizedHour,
                    ToplamTutar = 0,
                    HareketSayisi = 0
                };
                result.Add(hareket);
            }

            return result;
        }

        public async Task<MarketSettings> GetMarketSettingsAsync()
        {
            var settings = await _context.MarketSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                throw new Exception("İşletme saatleri tanımlı değil.");
            }
            return settings;
        }
    }
}