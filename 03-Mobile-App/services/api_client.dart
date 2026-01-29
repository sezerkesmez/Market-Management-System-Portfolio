import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/dtos.dart';

class ApiClient {
  final String baseUrl =
      'http://192.168.1.102:5000/api/KasaRapor'; // Yerel IP ve HTTP portu

  Future<ToplamRaporDto> getNakitRapor(
    DateTime baslangic,
    DateTime bitis,
  ) async {
    try {
      final response = await http.get(
        Uri.parse(
          '$baseUrl/NakitRapor?baslangic=${baslangic.toIso8601String()}&bitis=${bitis.toIso8601String()}',
        ),
      );
      if (response.statusCode == 200) {
        return ToplamRaporDto.fromJson(json.decode(response.body));
      } else {
        throw Exception(
          'Nakit raporu alınamadı: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      throw Exception('Nakit raporu alınırken hata: $e');
    }
  }

  Future<ToplamRaporDto> getKartRapor(
    DateTime baslangic,
    DateTime bitis,
  ) async {
    try {
      final response = await http.get(
        Uri.parse(
          '$baseUrl/KartRapor?baslangic=${baslangic.toIso8601String()}&bitis=${bitis.toIso8601String()}',
        ),
      );
      if (response.statusCode == 200) {
        return ToplamRaporDto.fromJson(json.decode(response.body));
      } else {
        throw Exception(
          'Kart raporu alınamadı: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      throw Exception('Kart raporu alınırken hata: $e');
    }
  }

  Future<ToplamRaporDto> getToplamKasaRapor(
    DateTime baslangic,
    DateTime bitis,
  ) async {
    try {
      final response = await http.get(
        Uri.parse(
          '$baseUrl/ToplamKasaRapor?baslangic=${baslangic.toIso8601String()}&bitis=${bitis.toIso8601String()}',
        ),
      );
      if (response.statusCode == 200) {
        return ToplamRaporDto.fromJson(json.decode(response.body));
      } else {
        throw Exception(
          'Toplam kasa raporu alınamadı: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      throw Exception('Toplam kasa raporu alınırken hata: $e');
    }
  }

  Future<List<SaatlikRaporDto>> getSaatlikHareketler(DateTime tarih) async {
    try {
      final response = await http.get(
        Uri.parse(
          '$baseUrl/SaatlikHareketler?tarih=${tarih.toIso8601String()}',
        ),
      );
      if (response.statusCode == 200) {
        final List<dynamic> jsonList = json.decode(response.body);
        return jsonList.map((json) => SaatlikRaporDto.fromJson(json)).toList();
      } else {
        throw Exception(
          'Saatlik hareketler alınamadı: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      throw Exception('Saatlik hareketler alınırken hata: $e');
    }
  }

  Future<MarketSettings> getMarketSettings() async {
    try {
      final response = await http.get(Uri.parse('$baseUrl/MarketSettings'));
      if (response.statusCode == 200) {
        return MarketSettings.fromJson(json.decode(response.body));
      } else {
        throw Exception(
          'Market ayarları alınamadı: ${response.statusCode} - ${response.body}',
        );
      }
    } catch (e) {
      throw Exception('Market ayarları alınırken hata: $e');
    }
  }
}
