using Kafa20.Model;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace Kafa20.DAL
{
    public static class SatisDAL
    {
        private static string connectionString = "Server=.\\SQLEXPRESS;Database=kafa20;Trusted_Connection=True;";

        public static void SatisYap(Satis satis, bool stoktanDus = true)
        {
            using (SqlConnection conn = new SqlConnection(connectionString))
            {
                conn.Open();
                SqlTransaction trans = conn.BeginTransaction();

                try
                {
                    // 1. Satış kaydı
                    SqlCommand cmd = new SqlCommand(@"
                        INSERT INTO Satislar (Tarih, ToplamTutar, OdemeYontemi, ReferansKod)
                        OUTPUT INSERTED.SatisId
                        VALUES (@Tarih, @ToplamTutar, @OdemeYontemi, @ReferansKod)", conn, trans);

                    cmd.Parameters.AddWithValue("@Tarih", satis.Tarih);
                    cmd.Parameters.AddWithValue("@ToplamTutar", satis.ToplamTutar);
                    cmd.Parameters.AddWithValue("@OdemeYontemi", satis.OdemeYontemi);
                    cmd.Parameters.AddWithValue("@ReferansKod", satis.ReferansKod);

                    int satisId = (int)cmd.ExecuteScalar();

                    // 2. Detaylar
                    foreach (var detay in satis.Detaylar)
                    {
                        SqlCommand detayCmd = new SqlCommand(@"
                            INSERT INTO SatisDetay (SatisId, StokId, Adet, Fiyat, Tutar)
                            VALUES (@SatisId, @StokId, @Adet, @Fiyat, @Tutar)", conn, trans);

                        detayCmd.Parameters.AddWithValue("@SatisId", satisId);
                        detayCmd.Parameters.AddWithValue("@StokId", detay.StokId);
                        detayCmd.Parameters.AddWithValue("@Adet", detay.Adet);
                        detayCmd.Parameters.AddWithValue("@Fiyat", detay.Fiyat);
                        detayCmd.Parameters.AddWithValue("@Tutar", detay.Tutar);
                        detayCmd.ExecuteNonQuery();

                        // 3. Stoktan düş
                        if (stoktanDus)
                        {
                            SqlCommand stokCmd = new SqlCommand(@"
                                UPDATE Stok SET Adet = Adet - @Adet WHERE StokId = @StokId", conn, trans);

                            stokCmd.Parameters.AddWithValue("@Adet", detay.Adet);
                            stokCmd.Parameters.AddWithValue("@StokId", detay.StokId);
                            stokCmd.ExecuteNonQuery();
                        }
                    }

                    // 4. Kasa hareketi
                    if (satis.OdemeYontemi != null) // vadeli olsaydı null olurdu
                    {
                        SqlCommand kasaCmd = new SqlCommand(@"
                            INSERT INTO KasaHareket (Tarih, Tutar, OdemeTipi, Aciklama, IslemTuru, IlgiliFaturaNo)
                            VALUES (@Tarih, @Tutar, @OdemeTipi, @Aciklama, @IslemTuru, @IlgiliFaturaNo)", conn, trans);

                        kasaCmd.Parameters.AddWithValue("@Tarih", satis.Tarih);
                        kasaCmd.Parameters.AddWithValue("@Tutar", satis.ToplamTutar);
                        kasaCmd.Parameters.AddWithValue("@OdemeTipi", satis.OdemeYontemi);
                        kasaCmd.Parameters.AddWithValue("@Aciklama", "Satış işlemi");
                        kasaCmd.Parameters.AddWithValue("@IslemTuru", "Satış");
                        kasaCmd.Parameters.AddWithValue("@IlgiliFaturaNo", satis.ReferansKod);
                        kasaCmd.ExecuteNonQuery();
                    }

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw;
                }
            }
        }
    }
}
