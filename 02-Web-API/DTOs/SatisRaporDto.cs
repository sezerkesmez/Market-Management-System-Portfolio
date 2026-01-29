// DTOs/SatisRaporDto.cs
using System;

namespace Kafa20.DTOs
{
    public class SatisRaporDto
    {
        public int HareketId { get; set; }
        public DateTime Tarih { get; set; }
        public decimal Tutar { get; set; }
        public string OdemeTipi { get; set; }
        public string Aciklama { get; set; }
        public string IlgiliFaturaNo { get; set; }
    }

    public class ToplamRaporDto
    {
        public decimal ToplamTutar { get; set; }
        public int SatisAdedi { get; set; }
        public DateTime BaslangicTarihi { get; set; }
        public DateTime BitisTarihi { get; set; }
    }

    public class SaatlikHareketDto
    {
        public int Saat { get; set; } // 0-23
        public decimal ToplamTutar { get; set; }
        public int SatisAdedi { get; set; }
    }
}