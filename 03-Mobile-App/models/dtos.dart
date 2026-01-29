class ToplamRaporDto {
  final double toplamTutar;
  final int satisAdedi;
  final DateTime baslangicTarihi;
  final DateTime bitisTarihi;

  ToplamRaporDto({
    required this.toplamTutar,
    required this.satisAdedi,
    required this.baslangicTarihi,
    required this.bitisTarihi,
  });

  factory ToplamRaporDto.fromJson(Map<String, dynamic> json) {
    return ToplamRaporDto(
      toplamTutar: (json['toplamTutar'] as num).toDouble(),
      satisAdedi: json['satisAdedi'],
      baslangicTarihi: DateTime.parse(json['baslangicTarihi']),
      bitisTarihi: DateTime.parse(json['bitisTarihi']),
    );
  }
}

class SaatlikRaporDto {
  final int saat;
  final double toplamTutar;
  final int hareketSayisi;

  SaatlikRaporDto({
    required this.saat,
    required this.toplamTutar,
    required this.hareketSayisi,
  });

  factory SaatlikRaporDto.fromJson(Map<String, dynamic> json) {
    return SaatlikRaporDto(
      saat: json['saat'],
      toplamTutar: (json['toplamTutar'] as num).toDouble(),
      hareketSayisi: json['hareketSayisi'],
    );
  }
}

class MarketSettings {
  final int id;
  final String openingTime;
  final String closingTime;
  final bool isNextDay;

  MarketSettings({
    required this.id,
    required this.openingTime,
    required this.closingTime,
    required this.isNextDay,
  });

  factory MarketSettings.fromJson(Map<String, dynamic> json) {
    return MarketSettings(
      id: json['id'],
      openingTime: json['openingTime'],
      closingTime: json['closingTime'],
      isNextDay: json['isNextDay'],
    );
  }
}
