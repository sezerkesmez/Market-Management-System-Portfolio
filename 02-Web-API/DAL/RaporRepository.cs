using Kafa20.Data;
using Kafa20.DTOs;
using Kafa20.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Kafa20.DAL
{
    public class RaporRepository : IRaporRepository
    {
        private readonly AppDbContext _context;

        public RaporRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<KasaHareket>> GetSatislarByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis)
        {
            var query = _context.KasaHareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis && h.IslemTuru == "Satış");

            if (!string.IsNullOrEmpty(odemeTipi))
            {
                query = query.Where(h => h.OdemeTipi == odemeTipi);
            }

            return await query.ToListAsync();
        }

        public async Task<decimal> GetToplamTutarByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis)
        {
            var query = _context.KasaHareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis && h.IslemTuru == "Satış");

            if (!string.IsNullOrEmpty(odemeTipi))
            {
                query = query.Where(h => h.OdemeTipi == odemeTipi);
            }

            return await query.SumAsync(h => h.Tutar);
        }

        public async Task<int> GetSatisAdediByOdemeTipiAsync(string odemeTipi, DateTime baslangic, DateTime bitis)
        {
            var query = _context.KasaHareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis && h.IslemTuru == "Satış");

            if (!string.IsNullOrEmpty(odemeTipi))
            {
                query = query.Where(h => h.OdemeTipi == odemeTipi);
            }

            return await query.CountAsync();
        }

        public async Task<decimal> GetToplamKasaAsync(DateTime baslangic, DateTime bitis)
        {
            return await _context.KasaHareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis && h.IslemTuru == "Satış")
                .SumAsync(h => h.Tutar);
        }

        public async Task<List<SaatlikRaporDto>> GetSaatlikHareketlerAsync(DateTime baslangic, DateTime bitis)
        {
            var hareketler = await _context.KasaHareketler
                .Where(h => h.Tarih >= baslangic && h.Tarih <= bitis && h.IslemTuru == "Satış")
                .GroupBy(h => new { Saat = h.Tarih.Hour })
                .Select(g => new SaatlikRaporDto
                {
                    Saat = g.Key.Saat,
                    ToplamTutar = g.Sum(h => h.Tutar),
                    HareketSayisi = g.Count()
                })
                .OrderBy(h => h.Saat)
                .ToListAsync();

            return hareketler;
        }
    }
}