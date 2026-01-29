using Kafa20.DTOs;
using Kafa20.Models;
using Kafa20.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Kafa20.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class KasaRaporController : ControllerBase
    {
        private readonly IRaporService _raporService;

        public KasaRaporController(IRaporService raporService)
        {
            _raporService = raporService;
        }

        [HttpGet("NakitSatislar")]
        public async Task<ActionResult> GetNakitSatislar([FromQuery] DateTime baslangic, [FromQuery] DateTime bitis)
        {
            var result = await _raporService.GetNakitSatislarAsync(baslangic, bitis);
            return Ok(result);
        }

        [HttpGet("NakitRapor")]
        public async Task<ActionResult> GetNakitRapor([FromQuery] DateTime baslangic, [FromQuery] DateTime bitis)
        {
            var result = await _raporService.GetNakitRaporAsync(baslangic, bitis);
            return Ok(result);
        }

        [HttpGet("KartSatislar")]
        public async Task<ActionResult> GetKartSatislar([FromQuery] DateTime baslangic, [FromQuery] DateTime bitis)
        {
            var result = await _raporService.GetKartSatislarAsync(baslangic, bitis);
            return Ok(result);
        }

        [HttpGet("KartRapor")]
        public async Task<ActionResult> GetKartRapor([FromQuery] DateTime baslangic, [FromQuery] DateTime bitis)
        {
            var result = await _raporService.GetKartRaporAsync(baslangic, bitis);
            return Ok(result);
        }

        [HttpGet("ToplamKasaRapor")]
        public async Task<ActionResult> GetToplamKasaRapor([FromQuery] DateTime baslangic, [FromQuery] DateTime bitis)
        {
            var result = await _raporService.GetToplamKasaRaporAsync(baslangic, bitis);
            return Ok(result);
        }

        [HttpGet("SaatlikHareketler")]
        public async Task<ActionResult> GetSaatlikHareketler([FromQuery] DateTime tarih)
        {
            var result = await _raporService.GetSaatlikHareketlerAsync(tarih);
            return Ok(result);
        }

        [HttpGet("MarketSettings")]
        public async Task<ActionResult> GetMarketSettings()
        {
            var settings = await _raporService.GetMarketSettingsAsync();
            return Ok(settings);
        }
    }
}