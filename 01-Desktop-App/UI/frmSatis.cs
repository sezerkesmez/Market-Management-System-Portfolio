using Guna.UI2.WinForms;
using Guna.UI2.WinForms.Enums;
using Kafa20.DAL;
using Kafa20.Data;
using Kafa20.Model;
using Kafa20.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using static Guna.UI2.WinForms.Suite.Descriptions;


namespace Kafa20
{
    public partial class frmSatis : Form
    {
        private class UrunVeri
        {
            public int StokId { get; set; }
            public string UrunAdi { get; set; }
            public int Adet { get; set; }
            public decimal Fiyat { get; set; }

            public decimal Tutar => Adet * Fiyat;
        }

        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "ReleaseCapture")]
        private extern static void ReleaseCapture();

        [System.Runtime.InteropServices.DllImport("user32.DLL", EntryPoint = "SendMessage")]
        private extern static void SendMessage(System.IntPtr hWnd, int wMsg, int wParam, int lParam);
       
        private List<SepetUrun> sepetListesi = new List<SepetUrun>();
        private List<BekleyenSepet> _bekleyenSepetler = new List<BekleyenSepet>();


        private Guna2TextBox txtBarkod;
        private Guna2Panel panelSepet;
        private Guna2DataGridView dgvSepet;
        private UrunDAL _urunDAL;
        private Guna2Button btnKartOde;
        private Guna2Button btnNakitOde;
        private Guna2TextBox txtOdenecekTutar;
        private Label lblOdenen;
        private FlowLayoutPanel flpSanalSepet;
        private Guna2Panel pnlSanalSepet;
        private Label lblToplamTutar;

        private Guna.UI2.WinForms.Guna2Panel pnlBekleyenSepetler; // Ana yan panel
        private Guna.UI2.WinForms.Guna2Button btnBekleyenleriAcKapa; // Yan paneli açıp kapatma düğmesi
        private Guna.UI2.WinForms.Guna2Panel pnlBekleyenSepetlerContainer; // Bekleyen sepetlerin kartlarını tutacak panel
        private bool isBekleyenPanelOpen = false; // Panelin açık/kapalı durumunu takip eder

        private string aktifReferansKod = null;
        private decimal odenenTutar = 0;
        private decimal toplamSepetTutari = 0;
        private List<SatisDetay> parcaliSatisDetaylari = new List<SatisDetay>();
        private bool sepetYuklendi = false;



        public frmSatis()
        {
            InitializeComponent();
            _urunDAL = new UrunDAL();
            this.Load += frmSatis_Load;

            // Formu yuvarlat
            Guna2Elipse elipse = new Guna2Elipse();
            elipse.BorderRadius = 20;
            elipse.TargetControl = this;

            // Üst Panel
            Guna2Panel pnlUst = new Guna2Panel
            {
                Dock = DockStyle.Top,
                Height = 40,
                FillColor = Color.FromArgb(36, 36, 62)
            };
            this.Controls.Add(pnlUst);
            pnlUst.MouseDown += Form_MouseDown;
            this.MouseDown += Form_MouseDown;

            // 3. Bekleyen Paneli Açma/Kapama Düğmesi
            btnBekleyenleriAcKapa = new Guna.UI2.WinForms.Guna2Button();
            btnBekleyenleriAcKapa.Name = "btnBekleyenleriAcKapa";
           
            btnBekleyenleriAcKapa.Text = "Bekleyen ▶"; // Başlangıçta kapalı olduğu için sağ ok
            btnBekleyenleriAcKapa.Location = new Point(this.Width - 30, 110); // Sağ kenara yakın bir yerde
            btnBekleyenleriAcKapa.Size = new Size(30, 130); // Dar ama uzun bir düğme
            btnBekleyenleriAcKapa.FillColor = System.Drawing.ColorTranslator.FromHtml("#5B7343");
            btnBekleyenleriAcKapa.ForeColor = Color.White;
            btnBekleyenleriAcKapa.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnBekleyenleriAcKapa.BorderRadius = 8;
            btnBekleyenleriAcKapa.Click += btnBekleyenleriAcKapa_Click;
            btnBekleyenleriAcKapa.BringToFront(); // Diğer kontrollerin üzerinde görünsün
            this.Controls.Add(btnBekleyenleriAcKapa);

            // Form boyutlandığında düğmenin konumunu ayarla
            // Form boyutlandığında düğmenin konumunu ayarla
            this.Resize += (s, ev) =>
            {
                // Panelin genişliği 0 ise (kapalıyken), düğme formun sağına yapışık olmalı
                // Eğer panel açıksa, düğme panelin solunda kalmalı
                if (!isBekleyenPanelOpen) // Panel kapalıyken
                {
                    btnBekleyenleriAcKapa.Location = new Point(this.Width - btnBekleyenleriAcKapa.Width, 110);
                }
                else // Panel açıkken
                {
                    btnBekleyenleriAcKapa.Location = new Point(pnlBekleyenSepetler.Left - btnBekleyenleriAcKapa.Width, 110);
                }
            };

            // Sepet Paneli (Sağ)
            panelSepet = new Guna2Panel();
            panelSepet.BorderRadius = 12;
            panelSepet.FillColor = Color.White;
            panelSepet.Size = new Size(400, 610);
            panelSepet.Location = new Point(800, 50);
            //panelSepet.ShadowDecoration.Enabled = true;
            Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            this.Controls.Add(panelSepet);

            //Sanal sepet paneli
            pnlSanalSepet = new Guna2Panel();
            pnlSanalSepet.Name = "pnlSanalSepet";
            pnlSanalSepet.Size = new Size(420, 350);
            pnlSanalSepet.Location = new Point(370, 110); // örnek konum                              
            pnlSanalSepet.BorderRadius = 12;
            pnlSanalSepet.BorderThickness = 1;
            pnlSanalSepet.BorderColor = Color.Gainsboro;
            //pnlSanalSepet.BackColor = Color.FromArgb(160, 240, 240, 240); // 160: şeffaflık (0=şeffaf, 255=opak)
            pnlSanalSepet.Visible = false;
            pnlSanalSepet.BorderRadius = 12;
            this.Controls.Add(pnlSanalSepet);

            // İçerik paneli (ürünleri alt alta yazmak için)
            flpSanalSepet = new FlowLayoutPanel();
            flpSanalSepet.Name = "flpSanalSepet";
            flpSanalSepet.Dock = DockStyle.Fill;
            flpSanalSepet.Padding = new Padding(8);        
            flpSanalSepet.AutoScroll = true;
            flpSanalSepet.BackColor = Color.Transparent;           
            pnlSanalSepet.Controls.Add(flpSanalSepet);

            // Sanalsepet Toplam tutarı gösterecek Label
            lblToplamTutar = new Label();
            lblToplamTutar.Name = "lblToplamTutar";
            lblToplamTutar.Font = new Font("Segoe UI", 12, FontStyle.Bold);
            lblToplamTutar.ForeColor = Color.DarkGreen;
            lblToplamTutar.TextAlign = ContentAlignment.MiddleRight;
            lblToplamTutar.Dock = DockStyle.Bottom;
            lblToplamTutar.BackColor=Color.Transparent;
            lblToplamTutar.Height = 30;
            lblToplamTutar.Padding = new Padding(0, 0, 10, 0);
            lblToplamTutar.Text = "Toplam: ₺0,00";

            pnlSanalSepet.Controls.Add(lblToplamTutar);
            lblToplamTutar.BringToFront(); // üstte kalsın


            // 3. Kapat butonu (panele ekle)
            Guna2Button btnKartKapat = new Guna2Button();
            btnKartKapat.Text = "✖";
            btnKartKapat.Size = new Size(30, 30);
            btnKartKapat.FillColor = Color.FromArgb(220, 53, 69); // kırmızı
            btnKartKapat.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            btnKartKapat.ForeColor = Color.White;
            btnKartKapat.BorderRadius = 10;
            btnKartKapat.Location = new Point(pnlSanalSepet.Width - 40, 10);
            btnKartKapat.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            btnKartKapat.Click += (s, e) =>
            {
                pnlSanalSepet.Visible = false;
                flpSanalSepet.Controls.Clear(); // listeyi de temizle
            };
            pnlSanalSepet.Controls.Add(btnKartKapat);
            btnKartKapat.BringToFront();


            // Kapat Butonu
            Guna2ControlBox btnKapat = new Guna2ControlBox
            {
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FillColor = pnlUst.FillColor,
                IconColor = Color.White,
                Location = new Point(this.Width - 45, 5),
                Size = new Size(35, 30)
            };
            btnKapat.Click += (s, e) => this.Close();
            pnlUst.Controls.Add(btnKapat);

            // Simge Butonu
            Guna2ControlBox btnSimge = new Guna2ControlBox
            {
                ControlBoxType = ControlBoxType.MinimizeBox,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
                FillColor = pnlUst.FillColor,
                IconColor = Color.White,
                Location = new Point(this.Width - 85, 5),
                Size = new Size(35, 30)
            };
            pnlUst.Controls.Add(btnSimge);

            // Ürün Paneli
            Guna2Panel panelUrunler = new Guna2Panel
            {
                Name = "panelUrunler",
                FillColor = Color.White,
                Size = new Size(350, this.Height - 50),
                Location = new Point(0, 40),
                //BorderRadius = 10,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            //panelUrunler.ShadowDecoration.Enabled = true;
            this.Controls.Add(panelUrunler);

            Guna2TextBox txtArama = new Guna2TextBox
            {
                Name = "txtArama",
                PlaceholderText = "🔍 Ürün ara...",
                Font = new Font("Segoe UI", 12),
                BorderRadius = 10,
                BorderThickness=0,
                Height = 40,
                Dock = DockStyle.Top,
                Margin = new Padding(10),
                Padding=new Padding(10),
                PlaceholderForeColor = Color.Gray,
                IconLeftOffset = new Point(5, 0),
                IconLeftSize = new Size(20, 20)
            };

            txtArama.TextChanged += TxtArama_TextChanged;

            // 📌 Panelin üst kısmına ekle
            panelUrunler.Controls.Add(txtArama);
            panelUrunler.Controls.SetChildIndex(txtArama, 0); // En üste koy



            Label lblUrunler = new Label
            {
                Text = "Ürün Paneli",
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 12, FontStyle.Bold),
                ForeColor = Color.DimGray,
                Location = new Point(10, 10),
                AutoSize = true
            };
            panelUrunler.Controls.Add(lblUrunler);

            FlowLayoutPanel flpUrunler = new FlowLayoutPanel
            {
                Name = "flpUrunler",
                Location = new Point(10, 40),
                Size = new Size(panelUrunler.Width - 20, panelUrunler.Height - 50),
                BackColor = Color.White,
                AutoScroll = true,              
                WrapContents = true,
                FlowDirection = FlowDirection.LeftToRight,
                Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left
            };
            panelUrunler.Controls.Add(flpUrunler);

            // Barkod Giriş
                txtBarkod = new Guna2TextBox
            {
                Name = "txtBarkod",
                PlaceholderText = "Barkod okut...",
                Font = new Font("Segoe UI", 12),
                Width = 250,
                Location = new Point(panelUrunler.Right + 20, 60),
                BorderRadius = 10
            };
            txtBarkod.KeyDown += TxtBarkod_KeyDown;
            this.Controls.Add(txtBarkod);

            //dgvSepet  ayarları
            SetupSepetDataGridView();

            // 1. Bekleyen Sepetler Yan Panelini Oluşturma (Ana Panel)
            pnlBekleyenSepetler = new Guna.UI2.WinForms.Guna2Panel();
            pnlBekleyenSepetler.Name = "pnlBekleyenSepetler";
            pnlBekleyenSepetler.Dock = DockStyle.None;
            pnlBekleyenSepetler.Width = 0; // Başlangıçta kapalı
            pnlBekleyenSepetler.FillColor = Color.FromArgb(160, 240, 240, 240); // 160: şeffaflık (0=şeffaf, 255=opak)
            pnlBekleyenSepetler.BorderRadius = 10;
            pnlBekleyenSepetler.BorderThickness = 1;
            pnlBekleyenSepetler.Size = new Size(300, 400); // Genişlik: 300, Yükseklik: 400
            pnlBekleyenSepetler.Location = new Point(this.Width - 300, 100); // Sağa yaslı, üstten 100px boşluk
            pnlBekleyenSepetler.BorderColor = Color.Gainsboro;
            this.Controls.Add(pnlBekleyenSepetler);
           

                                                             // 2. Bekleyen Sepetleri Tutacak İç Panel
            pnlBekleyenSepetlerContainer = new Guna.UI2.WinForms.Guna2Panel();
            pnlBekleyenSepetlerContainer.Name = "pnlBekleyenSepetlerContainer";
            pnlBekleyenSepetlerContainer.Dock = DockStyle.Fill; // Ana paneli dolduracak
            pnlBekleyenSepetlerContainer.AutoScroll = true;
            pnlBekleyenSepetlerContainer.Padding = new Padding(10);
            pnlBekleyenSepetlerContainer.BackColor = Color.Transparent; // Ana panelin rengini alsın
                                                                        // Geçici olarak bir arka plan rengi atayalım ki, sorun olursa boş panelin göründüğünden emin olalım
                                                                        // pnlBekleyenSepetlerContainer.BackColor = Color.LightBlue; // DEBUG AMAÇLI, sonra silebilirsiniz
           
            pnlBekleyenSepetler.Controls.Add(pnlBekleyenSepetlerContainer);

            


          


            GuncelToplamTutariHesaplaVeGoster();
        

        // Adet Giriş
        Guna2NumericUpDown nudAdet = new Guna2NumericUpDown
            {
                Name = "nudAdet",
                Font = new Font("Segoe UI", 12),
                Width = 80,
                Location = new Point(txtBarkod.Right + 10, txtBarkod.Top),
                Value = 1,
                Minimum = 1,
                BorderRadius = 10
            };
            this.Controls.Add(nudAdet);

            


            // Başlık (ikon ve stil ile)
            Label lblTitle = new Label();
            lblTitle.Text = "🛒 Sepet";
            lblTitle.BackColor = Color.Transparent;
            lblTitle.Font = new Font("Segoe UI", 13, FontStyle.Bold);
            lblTitle.ForeColor = Color.DimGray;
            lblTitle.Location = new Point(20, 20);
            lblTitle.AutoSize = true;
            panelSepet.Controls.Add(lblTitle);
          
           

            // Toplam Etiketi
            Label lblToplam = new Label();
            lblToplam.Name = "lblToplam";
            lblToplam.Text = "Toplam: ₺0.00";
            lblToplam.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblToplam.ForeColor = Color.Black;
            lblToplam.Location = new Point(20, 380);
            lblToplam.AutoSize = true;
            panelSepet.Controls.Add(lblToplam);

            //ödenen tutar
            lblOdenen = new Label();
            lblOdenen.Text = "Ödenen: ₺0.00";
            lblOdenen.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            lblOdenen.ForeColor = Color.FromArgb(220, 53, 69); // Kırmızımsı
            lblOdenen.Location = new Point(220, 380);
            lblOdenen.AutoSize = true;
            panelSepet.Controls.Add(lblOdenen);

            // Tutar Giriş TextBox'u
            txtOdenecekTutar = new Guna2TextBox();
            txtOdenecekTutar.Name = "txtOdenecekTutar";
            txtOdenecekTutar.PlaceholderText = "Ödenecek Tutar ₺";
            txtOdenecekTutar.Font = new Font("Segoe UI", 12);
            txtOdenecekTutar.Width = 360;
            txtOdenecekTutar.Location = new Point(20, 420); // Ödeme butonlarının üstü
            txtOdenecekTutar.BorderRadius = 8;
            txtOdenecekTutar.TextAlign = HorizontalAlignment.Center;
            panelSepet.Controls.Add(txtOdenecekTutar);

            // Butonlar
            Guna2Button btnKart = new Guna2Button();
            btnKart.Text = "Kart Ödeme";
            btnKart.FillColor = Color.FromArgb(47, 84, 150);
            btnKart.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnKart.Size = new Size(360, 40);
            btnKart.Location = new Point(20, 470);
            btnKart.BorderRadius = 8;
            btnKart.Click += btnKart_Click;
            panelSepet.Controls.Add(btnKart);

            Guna2Button btnNakit = new Guna2Button();
            btnNakit.Text = "Nakit Ödeme";
            btnNakit.FillColor = Color.FromArgb(121, 143, 200);
            btnNakit.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnNakit.Size = new Size(360, 40);
            btnNakit.Location = new Point(20, 520);
            btnNakit.BorderRadius = 8;
            btnNakit.Click += btnNakit_Click;
            panelSepet.Controls.Add(btnNakit);

            Guna2Button btnBeklet = new Guna2Button();
            btnBeklet.Text = "Satışı Beklet";
            btnBeklet.FillColor = Color.FromArgb(0, 174, 153);
            btnBeklet.Font = new Font("Segoe UI", 11, FontStyle.Bold);
            btnBeklet.Size = new Size(360, 40);
            btnBeklet.Location = new Point(20, 570);
            btnBeklet.BorderRadius = 8;
            btnBeklet.Click += btnBeklet_Click;
            panelSepet.Controls.Add(btnBeklet);


        }

        private void TxtArama_TextChanged(object sender, EventArgs e)
        {
            var txt = sender as Guna2TextBox;
            string arama = txt.Text.Trim();

            UrunleriYukle(0, 20, arama);
        }

        /// sanal sepetin toplamını hesaplama
        private void ToplamTutarGuncelle()
        {
            var flp = this.Controls.Find("flpSanalSepet", true).FirstOrDefault() as FlowLayoutPanel;
            var lblToplamTutar = this.Controls.Find("lblToplamTutar", true).FirstOrDefault() as Label;

            if (flp == null || lblToplamTutar == null) return;

            decimal toplam = 0;

            foreach (Guna2Panel panel in flp.Controls.OfType<Guna2Panel>())
            {
                var lbl = panel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;
                if (lbl?.Tag is UrunVeri veri)
                {
                    toplam += veri.Tutar;
                }
            }

            lblToplamTutar.Text = $"Toplam: {toplam:C2}";
        }


        /// <summary>
        /// Bekleyen sepetler yan panelini açar veya kapatır.
        /// </summary>
        private void btnBekleyenleriAcKapa_Click(object sender, EventArgs e)
        {
            int targetWidth;
            string buttonText;
            int buttonX;

            if (isBekleyenPanelOpen)
            {
                // Paneli kapat
                targetWidth = 0;
                buttonText = "Bekleyen Sepetler ▶";
                buttonX = this.Width - btnBekleyenleriAcKapa.Width;
            }
            else
            {
                // Paneli aç
                targetWidth = 300; // Panelin açılacağı genişlik
                buttonText = "Bekleyen Sepetler ◀";
                buttonX = this.Width - targetWidth - btnBekleyenleriAcKapa.Width;
            }

            System.Windows.Forms.Timer animationTimer = new System.Windows.Forms.Timer();
            animationTimer.Interval = 10;
            int step = 30;

            pnlBekleyenSepetler.BringToFront();
            btnBekleyenleriAcKapa.BringToFront();

            animationTimer.Tick += (s, ev) =>
            {
                if (isBekleyenPanelOpen) // Kapanıyor
                {
                    if (pnlBekleyenSepetler.Width - step <= targetWidth)
                    {
                        pnlBekleyenSepetler.Width = targetWidth;
                        btnBekleyenleriAcKapa.Location = new Point(buttonX, btnBekleyenleriAcKapa.Location.Y);
                        animationTimer.Stop();
                        animationTimer.Dispose();
                        pnlBekleyenSepetler.Refresh(); // Add this line
                        pnlBekleyenSepetlerContainer.Refresh(); // Add this line
                    }
                    else
                    {
                        pnlBekleyenSepetler.Width -= step;
                        btnBekleyenleriAcKapa.Location = new Point(pnlBekleyenSepetler.Left - btnBekleyenleriAcKapa.Width, btnBekleyenleriAcKapa.Location.Y);
                    }
                }
                else // Açılıyor
                {
                    if (pnlBekleyenSepetler.Width + step >= targetWidth)
                    {
                        pnlBekleyenSepetler.Width = targetWidth;
                        btnBekleyenleriAcKapa.Location = new Point(buttonX, btnBekleyenleriAcKapa.Location.Y);
                        animationTimer.Stop();
                        animationTimer.Dispose();

                        // *** YENİ EKLENEN SATIR ***
                        pnlBekleyenSepetler.PerformLayout();
                        pnlBekleyenSepetler.Refresh(); // Add this line
                        pnlBekleyenSepetlerContainer.Refresh(); // Add this line

                        // Panel tamamen açıldıktan sonra içerikleri güncelle
                        GuncelleBekleyenSepetlerPaneli();
                    }
                    else
                    {
                        pnlBekleyenSepetler.Width += step;
                        btnBekleyenleriAcKapa.Location = new Point(pnlBekleyenSepetler.Left - btnBekleyenleriAcKapa.Width, btnBekleyenleriAcKapa.Location.Y);
                    }
                }
            };

            animationTimer.Start();

            btnBekleyenleriAcKapa.Text = buttonText;
            isBekleyenPanelOpen = !isBekleyenPanelOpen;
        }
        private void btnBeklet_Click(object sender, EventArgs e)
        {
            if (dgvSepet.Rows.Count == 0)
            {
                MessageBox.Show("Beklemeye alınacak ürün bulunamadı. Sepet boş!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string beklemeNotu = Microsoft.VisualBasic.Interaction.InputBox("Beklemeye alınan sepet için bir not girin (isteğe bağlı):", "Sepet Notu", "");

            BekleyenSepet yeniBekleyenSepet = new BekleyenSepet();
            yeniBekleyenSepet.BeklemeTarihi = DateTime.Now;
            yeniBekleyenSepet.BeklemeNotu = beklemeNotu;

            decimal anlikToplamTutar = 0;

            foreach (DataGridViewRow row in dgvSepet.Rows)
            {
                SepetUrunItem item = new SepetUrunItem
                {
                    StokId = row.Cells["StokId"].Value != null ? Convert.ToInt32(row.Cells["StokId"].Value) : 0,
                    UrunAdi = row.Cells["UrunAdi"].Value?.ToString(),
                    Adet = row.Cells["Adet"].Value != null ? Convert.ToInt32(row.Cells["Adet"].Value) : 0,
                    Fiyat = row.Cells["Fiyat"].Value != null ?
                        decimal.TryParse(row.Cells["Fiyat"].Value.ToString().Replace("₺", "").Trim(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture, out decimal f) ? f : 0 : 0,
                    Tutar = row.Cells["Tutar"].Value != null ?
                        decimal.TryParse(row.Cells["Tutar"].Value.ToString().Replace("₺", "").Trim(), System.Globalization.NumberStyles.Currency, System.Globalization.CultureInfo.CurrentCulture, out decimal t) ? t : 0 : 0
                };
                yeniBekleyenSepet.Urunler.Add(item);
                anlikToplamTutar += item.Tutar;
            }

            yeniBekleyenSepet.ToplamTutar = anlikToplamTutar;
            _bekleyenSepetler.Add(yeniBekleyenSepet);

            dgvSepet.Rows.Clear();
            GuncelToplamTutariHesaplaVeGoster();

            //MessageBox.Show("Sepet beklemeye alındı!", "İşlem Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);

            // *** BU SATIRI KALDIRILDI! ***
             GuncelleBekleyenSepetlerPaneli(); 
        }
        /// <summary>
        /// Bekleyen sepetler panelini (pnlBekleyenSepetlerContainer) günceller.
        /// Her sepet için bir Guna2Panel kartı oluşturur.
        /// </summary>
        /// <summary>
        /// Bekleyen sepetler panelini (pnlBekleyenSepetlerContainer) günceller.
        /// Her sepet için bir Guna2Panel kartı oluşturur.
        /// </summary>
        private void GuncelleBekleyenSepetlerPaneli()
        {
            pnlBekleyenSepetlerContainer.Controls.Clear();

            int yPos = 10;

            foreach (var sepet in _bekleyenSepetler)
            {
                Guna2Panel sepetKarti = new Guna2Panel
                {
                    Width = Math.Max(pnlBekleyenSepetlerContainer.ClientSize.Width - 20, 260),
                    Height = 80,
                    Location = new Point(10, yPos),
                    FillColor = Color.White,
                    BorderRadius = 8,
                    BorderThickness = 1,
                    BorderColor = Color.LightGray,
                    Tag = sepet.SepetId
                };

                sepetKarti.DoubleClick += BekleyenSepetKarti_DoubleClick;

                Label lblTarih = new Label
                {
                    Text = sepet.BeklemeTarihi.ToString("HH:mm - dd.MM"),
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Location = new Point(10, 10),
                    AutoSize = true
                };
                lblTarih.DoubleClick += BekleyenSepetKarti_DoubleClick;
                sepetKarti.Controls.Add(lblTarih);

                Label lblTutar = new Label
                {
                    Text = sepet.ToplamTutar.ToString("C"),
                    Font = new Font("Segoe UI", 11, FontStyle.Bold),
                    Location = new Point(10, 35),
                    AutoSize = true
                };
                lblTutar.DoubleClick += BekleyenSepetKarti_DoubleClick;
                sepetKarti.Controls.Add(lblTutar);

                Label lblNot = new Label
                {
                    Text = !string.IsNullOrEmpty(sepet.BeklemeNotu) ? $"Not: {sepet.BeklemeNotu}" : "Not yok",
                    Font = new Font("Segoe UI", 8, FontStyle.Italic),
                    Location = new Point(10, 58),
                    AutoSize = true,
                    ForeColor = Color.DimGray
                };
                lblNot.DoubleClick += BekleyenSepetKarti_DoubleClick;
                sepetKarti.Controls.Add(lblNot);

                Guna2Button btnSil = new Guna2Button
                {
                    Text = "🗑️",
                    FillColor = Color.Red,
                    ForeColor = Color.White,
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    Size = new Size(25, 25),
                    BorderRadius = 5,
                    Location = new Point(sepetKarti.Width - 35, 10),
                    Tag = sepet.SepetId
                };
                btnSil.Click += BekleyenSepetKartiSil_Click;
                sepetKarti.Controls.Add(btnSil);

                pnlBekleyenSepetlerContainer.Controls.Add(sepetKarti);
                yPos += sepetKarti.Height + 10;
            }
        }
        private void BekleyenSepetKarti_DoubleClick(object sender, EventArgs e)
        {
            // Tıklanan kontrolün Tag'indeki SepetId'yi al
            Guid secilenSepetId;
            if (sender is Control clickedControl && clickedControl.Tag is Guid)
            {
                secilenSepetId = (Guid)clickedControl.Tag;
            }
            else if (sender is Label clickedLabel && clickedLabel.Parent is Guna.UI2.WinForms.Guna2Panel parentPanel && parentPanel.Tag is Guid)
            {
                secilenSepetId = (Guid)parentPanel.Tag;
            }
            else
            {
                return; // Geçersiz tıklama
            }


            // Eğer mevcut sepette ürün varsa, kullanıcıya sor
            if (dgvSepet.Rows.Count > 0)
            {
                DialogResult result = MessageBox.Show(
                    "Mevcut sepeti beklemeye alıp, seçilen bekleyen sepeti mi yüklemek istiyorsunuz?",
                    "Sepet Yükleme Onayı",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Yes)
                {
                    btnBeklet_Click(null, null); // Mevcut sepeti beklemeye al
                }
                else if (result == DialogResult.Cancel)
                {
                    return; // İşlemi iptal et
                }
            }

            BekleyenSepet yuklenecekSepet = _bekleyenSepetler.FirstOrDefault(s => s.SepetId == secilenSepetId);

            if (yuklenecekSepet != null)
            {
                // Mevcut sepeti temizle
                dgvSepet.Rows.Clear();

                // Bekleyen sepetteki ürünleri ana sepete aktar
                foreach (var item in yuklenecekSepet.Urunler)
                {
                    // Sütun sırası: Sil, StokId, UrunAdi, Adet, Fiyat, Tutar, Artir, Azalt
                    dgvSepet.Rows.Add(
                        "🗑️",
                        item.StokId,
                        item.UrunAdi,
                        item.Adet,
                        item.Fiyat.ToString("C"),
                        item.Tutar.ToString("C"),
                        "➕",
                        "➖"
                    );
                }

                // Yüklenen sepeti bekleyenler listesinden kaldır
                _bekleyenSepetler.Remove(yuklenecekSepet);

                GuncelToplamTutariHesaplaVeGoster(); // Toplamı güncelle
                GuncelleBekleyenSepetlerPaneli(); // Bekleyenler panelini güncelle

                MessageBox.Show("Sepet başarıyla yüklendi.", "İşlem Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);

                // Sepet yüklendikten sonra paneli otomatik kapat (isteğe bağlı)
                if (isBekleyenPanelOpen)
                {
                    btnBekleyenleriAcKapa_Click(null, null); // Paneli kapat
                }
            }
        }

        /// <summary>
        /// Bekleyen bir sepet kartındaki "X" butonuna tıklandığında sepeti siler.
        /// </summary>
        private void BekleyenSepetKartiSil_Click(object sender, EventArgs e)
        {
            Guna.UI2.WinForms.Guna2Button clickedButton = sender as Guna.UI2.WinForms.Guna2Button;
            if (clickedButton != null && clickedButton.Tag is Guid)
            {
                Guid silinecekSepetId = (Guid)clickedButton.Tag;

                if (MessageBox.Show("Bu bekleyen sepeti silmek istediğinizden emin misiniz?", "Sepet Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    BekleyenSepet silinecekSepet = _bekleyenSepetler.FirstOrDefault(s => s.SepetId == silinecekSepetId);
                    if (silinecekSepet != null)
                    {
                        _bekleyenSepetler.Remove(silinecekSepet);
                        GuncelleBekleyenSepetlerPaneli(); // Paneli güncelle
                        MessageBox.Show("Bekleyen sepet silindi.", "İşlem Tamam", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
        }
        private async void TitrePanel(Control kontrol, int genislik = 5, int tekrar = 3)
        {
            if (kontrol == null) return;

            var orijinalKonum = kontrol.Location;

            for (int i = 0; i < tekrar; i++)
            {
                kontrol.Location = new Point(orijinalKonum.X + genislik, orijinalKonum.Y);
                await Task.Delay(50);
                kontrol.Location = new Point(orijinalKonum.X - genislik, orijinalKonum.Y);
                await Task.Delay(50);
            }

            // Orijinal konuma dön
            kontrol.Location = orijinalKonum;
        }

        private void dgvSepet_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.ColumnIndex < 0) return;

            string columnName = dgvSepet.Columns[e.ColumnIndex].Name;

            // 🔁 Artır / Azalt / Sil işlemleri
            if (columnName == "Artir" || columnName == "Azalt" || columnName == "Sil")
            {
                DataGridViewRow clickedRow = dgvSepet.Rows[e.RowIndex];

                if (clickedRow.Cells["StokId"].Value == null)
                {
                    MessageBox.Show("Ürün ID bulunamadı. Lütfen sepet satırındaki StokId sütununu kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                int stokId = Convert.ToInt32(clickedRow.Cells["StokId"].Value);

                switch (columnName)
                {
                    case "Artir":
                        UrunIslemiYap(stokId, 1);
                        break;
                    case "Azalt":
                        UrunIslemiYap(stokId, -1);
                        break;
                    case "Sil":
                        UrunIslemiYap(stokId, 0, true);
                        break;
                }
            }

            // 🟨 Ürün adına tıklanırsa sanal sepete ürün ekle
            else if (columnName == "UrunAdi")
            {
                var row = dgvSepet.Rows[e.RowIndex];
                string urunAdi = row.Cells["UrunAdi"].Value.ToString();
                decimal fiyat = decimal.Parse(row.Cells["Fiyat"].Value.ToString().Replace("₺", ""));
                int eklenecekAdet = 1;

                int stoktakiAdet = int.Parse(row.Cells["Adet"].Value.ToString());


                var pnl = this.Controls.Find("pnlSanalSepet", true).FirstOrDefault() as Guna2Panel;
                var flp = pnl?.Controls.Find("flpSanalSepet", true).FirstOrDefault() as FlowLayoutPanel;

                if (pnl == null || flp == null) return;

                pnl.Visible = true;
                if (!pnl.Controls.ContainsKey("pnlAltButonlar"))
                {
                    SanalSepetButonPaneliniOlustur();
                }

                // Daha önce eklenmiş ürün paneli var mı?
                Guna2Panel varOlanPanel = null;

                foreach (Guna2Panel item in flp.Controls.OfType<Guna2Panel>())
                {
                    if (item.Tag != null && item.Tag.ToString() == urunAdi)
                    {
                        varOlanPanel = item;
                        break;
                    }
                }

                if (varOlanPanel != null)
                {
                    var lbl = varOlanPanel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;
                    if (lbl == null) return;

                    UrunVeri veri = lbl.Tag as UrunVeri;

                    if (veri == null)
                    {
                        // Eğer tag boşsa tekrar oluştur (önceki bozuksa)
                        veri = new UrunVeri
                        {
                            StokId = Convert.ToInt32(row.Cells["StokId"].Value),
                            UrunAdi = urunAdi,
                            Adet = 0,
                            Fiyat = fiyat
                        };
                    }

                    if (veri.Adet >= stoktakiAdet)
                    {
                        TitrePanel(varOlanPanel);
                        return;
                    }

                    veri.Adet += 1;
                    lbl.Tag = veri;
                    lbl.Text = $"{veri.UrunAdi} - {veri.Adet} Adet - {veri.Tutar:C2}";
                }
                else
                {
                    // 🆕 Yeni ürün paneli oluştur
                    Guna2Panel urunPanel = new Guna2Panel();
                    urunPanel.Name = "urun_" + urunAdi;
                    urunPanel.Size = new Size(350, 30);
                    urunPanel.FillColor = Color.Transparent;
                    urunPanel.BorderRadius = 12;
                    urunPanel.Margin = new Padding(3);
                    urunPanel.Tag = urunAdi;

                    // Ürün bilgisi Label
                    Label lblInfo = new Label();
                    lblInfo.AutoSize = false;
                    lblInfo.Dock = DockStyle.Left;
                    lblInfo.Width = 180;
                    lblInfo.Font = new Font("Segoe UI", 9, FontStyle.Bold);
                    UrunVeri veri = new UrunVeri
                    {
                        StokId = Convert.ToInt32(row.Cells["StokId"].Value),
                        UrunAdi = urunAdi,
                        Adet = 1,
                        Fiyat = fiyat
                    };
                    lblInfo.Tag = veri;
                    lblInfo.Text = $"{veri.UrunAdi} - {veri.Adet} Adet - {veri.Tutar:C2}";
                    lblInfo.Name = "lblInfo";

                    // ➖ Azalt butonu
                    Guna2Button btnAzalt = new Guna2Button();
                    btnAzalt.Size = new Size(30, 30);
                    btnAzalt.Font = new Font("Segoe UI Emoji", 8, FontStyle.Regular);
                    btnAzalt.Text = "➖";
                    btnAzalt.FillColor = Color.Transparent;
                    btnAzalt.BackColor = Color.Transparent;
                    btnAzalt.ForeColor = Color.OrangeRed;
                    btnAzalt.Cursor = Cursors.Hand;
                    btnAzalt.Tag = urunPanel;
                    btnAzalt.Click += BtnAzalt_Click;

                    // ➕ Artır butonu
                    Guna2Button btnArtir = new Guna2Button();
                    btnArtir.Size = new Size(30, 30);
                    btnArtir.Font = new Font("Segoe UI Emoji", 8, FontStyle.Regular);
                    btnArtir.Text = "➕";
                    btnArtir.FillColor = Color.Transparent;
                    btnArtir.BackColor = Color.Transparent;
                    btnArtir.ForeColor = Color.Green;
                    btnArtir.Cursor = Cursors.Hand;
                    btnArtir.Tag = urunPanel;
                    btnArtir.Click += BtnArtir_Click;

                    // Butonlar Paneli
                    FlowLayoutPanel flpButtons = new FlowLayoutPanel();
                    flpButtons.Dock = DockStyle.Right;
                    flpButtons.FlowDirection = FlowDirection.LeftToRight;
                    flpButtons.WrapContents = false;
                    flpButtons.Width = 70;
                    flpButtons.BackColor = Color.Transparent;
                    flpButtons.Controls.Add(btnAzalt);
                    flpButtons.Controls.Add(btnArtir);

                    // Paneli birleştir
                    urunPanel.Controls.Add(lblInfo);
                    urunPanel.Controls.Add(flpButtons);
                    flp.Controls.Add(urunPanel);
                }

                ToplamTutarGuncelle();
            }
        }
        private void BtnArtir_Click(object sender, EventArgs e)
        {
            var btn = sender as Guna2Button;
            var urunPanel = btn?.Tag as Guna2Panel;
            if (urunPanel == null) return;

            var lbl = urunPanel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;
            if (lbl == null || !(lbl.Tag is UrunVeri veri)) return;

            // dgvSepet'ten stok adedini al
            int stokAdeti = 999;
            foreach (DataGridViewRow row in dgvSepet.Rows)
            {
                if (row.Cells["UrunAdi"].Value.ToString() == veri.UrunAdi)
                {
                    stokAdeti = int.Parse(row.Cells["Adet"].Value.ToString());
                    break;
                }
            }

            if (veri.Adet >= stokAdeti)
            {
                TitrePanel(urunPanel);
                return;
            }

            veri.Adet++;
            lbl.Text = $"{veri.UrunAdi} - {veri.Adet} Adet - {veri.Tutar:C2}";
            ToplamTutarGuncelle();
        }
        private void BtnAzalt_Click(object sender, EventArgs e)
        {
            var btn = sender as Guna2Button;
            var urunPanel = btn?.Tag as Guna2Panel;
            if (urunPanel == null) return;

            var lbl = urunPanel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;
            if (lbl == null || !(lbl.Tag is UrunVeri veri)) return;

            // Adet 1 ise ➖ yapıldığında ürünü kaldır
            if (veri.Adet <= 1)
            {
                var flp = urunPanel.Parent as FlowLayoutPanel;
                flp?.Controls.Remove(urunPanel);
                ToplamTutarGuncelle();
                return;
            }

            // Adeti azalt
            veri.Adet--;
            lbl.Text = $"{veri.UrunAdi} - {veri.Adet} Adet - {veri.Tutar:C2}";
            ToplamTutarGuncelle();
        }

        // BURAYA EKLE: UrunIslemiYap metodu (dgvSepet_CellContentClick'in kullandığı metod)
        private void UrunIslemiYap(int stokId, int adetDegisimi, bool sil = false)
        {
            // Bu metodun içeriğini önceki cevabımdan alıp buraya yapıştırmalısın.
            // dgv değişkenini sınıf düzeyindeki dgvSepet'e yönlendiriyoruz
            // var dgv = this.Controls.Find("dgvSepet", true).FirstOrDefault() as Guna2DataGridView; // Bu satıra gerek kalmaz
            // if (dgv == null) return; // Bu satıra gerek kalmaz

            DataGridViewRow hedefSira = null;

            foreach (DataGridViewRow row in dgvSepet.Rows) // dgv yerine dgvSepet kullanıyoruz
            {
                if (Convert.ToInt32(row.Cells["StokId"].Value) == stokId)
                {
                    hedefSira = row;
                    break;
                }
            }

            if (hedefSira == null) return;

            if (sil)
            {
                if (MessageBox.Show("Bu ürünü sepetten silmek istediğinizden emin misiniz?", "Ürün Silme Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    dgvSepet.Rows.Remove(hedefSira);
                    GuncelToplamTutariHesaplaVeGoster();
                }
            }
            else
            {
                int mevcutAdet = Convert.ToInt32(hedefSira.Cells["Adet"].Value);
                int yeniAdet = mevcutAdet + adetDegisimi;

                if (yeniAdet <= 0)
                {
                    if (MessageBox.Show("Bu ürünün adetini sıfırlamak (sepetteki son adedi silmek) istediğinizden emin misiniz?", "Adet Azaltma Onayı", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        dgvSepet.Rows.Remove(hedefSira);
                    }
                }
                else
                {
                    hedefSira.Cells["Adet"].Value = yeniAdet;

                    decimal birimFiyat;
                    string fiyatText = hedefSira.Cells["Fiyat"].Value.ToString().Replace("₺", "").Trim();
                    if (decimal.TryParse(fiyatText, out birimFiyat))
                    {
                        hedefSira.Cells["Tutar"].Value = (birimFiyat * yeniAdet).ToString("C");
                    }
                    else
                    {
                        MessageBox.Show("Ürün fiyatı okunamadı. Lütfen DataGridView sütun formatını kontrol edin.", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                GuncelToplamTutariHesaplaVeGoster();
            }
        }
        private void SetupSepetDataGridView()
        {
            // DataGridView (Sepet)
            dgvSepet = new Guna2DataGridView(); // Guna2DataGridView'ı sınıf düzeyinde tanımladık, burada sadece new'liyoruz
            dgvSepet.Name = "dgvSepet";
            dgvSepet.Size = new Size(360, 300);
            dgvSepet.Location = new Point(20, 60); // panelSepet içindeki konumu
            dgvSepet.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
            dgvSepet.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 10, FontStyle.Bold);
            dgvSepet.DefaultCellStyle.Font = new Font("Segoe UI", 10);
            dgvSepet.BorderStyle = BorderStyle.None;
            dgvSepet.RowTemplate.Height = 32;
            dgvSepet.AllowUserToAddRows = false;
            dgvSepet.AllowUserToDeleteRows = false;
            dgvSepet.AllowUserToResizeColumns = false;
            dgvSepet.BackgroundColor = Color.White;
            dgvSepet.GridColor = Color.Gainsboro;
            panelSepet.Controls.Add(dgvSepet); // panelSepet'e ekle

            // Sütunları temizle ve yeniden ekle
            dgvSepet.Columns.Clear();

            // --- SİMGE (EMOJİ/UNİCODE KARAKTER) VE DİĞER SÜTUNLAR ---

            // 1. Silme Simgesi Sütunu
            DataGridViewTextBoxColumn colSil = new DataGridViewTextBoxColumn();
            colSil.Name = "Sil";
            colSil.HeaderText = "Sil";
            colSil.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // BURAYI DEĞİŞTİRİYORUZ!
                                                                           // colSil.Width = 30; // Bu satırı artık AutoSizeMode kullandığımız için kaldırıyoruz veya yoruma alıyoruz
            colSil.DefaultCellStyle.Font = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
            colSil.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colSil.ReadOnly = true;
            colSil.ToolTipText = "Ürünü sepetten sil";
            dgvSepet.Columns.Add(colSil);

            // 2. Gizli StokId Sütunu (ürünü benzersiz tanımak için)
            dgvSepet.Columns.Add("StokId", "Stok ID");
            dgvSepet.Columns["StokId"].Visible = false;
            dgvSepet.Columns["StokId"].Width = 0; // Genişliği 0 yap

            // 3. Ürün Adı Sütunu
            dgvSepet.Columns.Add("UrunAdi", "Ürün");
            dgvSepet.ReadOnly =true;
            dgvSepet.Columns["UrunAdi"].AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill; // Kalan boşluğu doldurur

            // 4. Adet Sütunu
            dgvSepet.Columns.Add("Adet", "Adet");
            dgvSepet.Columns["Adet"].Width = 90;
            dgvSepet.Columns["Adet"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // 5. Fiyat Sütunu
            dgvSepet.Columns.Add("Fiyat", "Fiyat");
            dgvSepet.Columns["Fiyat"].Width = 90;
            dgvSepet.Columns["Fiyat"].DefaultCellStyle.Format = "C";
            dgvSepet.Columns["Fiyat"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 6. Tutar Sütunu
            dgvSepet.Columns.Add("Tutar", "Tutar");
            dgvSepet.Columns["Tutar"].Width = 100;
            dgvSepet.Columns["Tutar"].DefaultCellStyle.Format = "C";
            dgvSepet.Columns["Tutar"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleRight;

            // 7. Artırma Simgesi Sütunu
            DataGridViewTextBoxColumn colArtir = new DataGridViewTextBoxColumn();
            colArtir.Name = "Artir";
            colArtir.HeaderText = "+";
            colArtir.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // BURAYI DEĞİŞTİRİYORUZ!
                                                                             // colArtir.Width = 30; // Bu satırı artık AutoSizeMode kullandığımız için kaldırıyoruz veya yoruma alıyoruz
            colArtir.DefaultCellStyle.Font = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
            colArtir.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colArtir.ReadOnly = true;
            colArtir.ToolTipText = "Adeti artır";
            dgvSepet.Columns.Add(colArtir);

            // 8. Azaltma Simgesi Sütunu
            DataGridViewTextBoxColumn colAzalt = new DataGridViewTextBoxColumn();
            colAzalt.Name = "Azalt";
            colAzalt.HeaderText = "-";
            colAzalt.AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; // BURAYI DEĞİŞTİRİYORUZ!
                                                                             // colAzalt.Width = 30; // Bu satırı artık AutoSizeMode kullandığımız için kaldırıyoruz veya yoruma alıyoruz
            colAzalt.DefaultCellStyle.Font = new Font("Segoe UI Emoji", 8, FontStyle.Bold);
            colAzalt.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            colAzalt.ReadOnly = true;
            colAzalt.ToolTipText = "Adeti azalt";
            dgvSepet.Columns.Add(colAzalt);

            // Kullanıcıların sütunları yeniden sıralamasını veya genişletmesini engelle
            foreach (DataGridViewColumn col in dgvSepet.Columns)
            {
                col.SortMode = DataGridViewColumnSortMode.NotSortable;
                if (col.Name != "UrunAdi") // UrunAdi sütununun esnek kalmasını sağlar
                {
                    col.Resizable = DataGridViewTriState.False;
                }
                if (col.Visible == false) // Gizli sütunların genişliğini 0 yapar
                {
                    col.Width = 0;
                }
            }

            // DataGridView'ın CellContentClick olayına abone ol
            dgvSepet.CellContentClick += dgvSepet_CellContentClick;
        }
        private void SanalSepetButonPaneliniOlustur()
        {
            var pnlSanalSepet = this.Controls.Find("pnlSanalSepet", true).FirstOrDefault() as Guna2Panel;
            if (pnlSanalSepet == null) return;

            // Alt panel (butonlar için)
            Panel pnlAltButonlar = new Panel();
            pnlAltButonlar.Dock = DockStyle.Bottom;
            pnlAltButonlar.Height = 50;
            pnlAltButonlar.BackColor = Color.Transparent;
            pnlAltButonlar.Name = "pnlAltButonlar";

            // ➕ Nakit Öde butonu
            btnNakitOde = new Guna2Button();
            btnNakitOde.Text = "🟢 Nakit Öde";
            btnNakitOde.Width = 140;
            btnNakitOde.Height = 35;
            btnNakitOde.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnNakitOde.FillColor = Color.ForestGreen;
            btnNakitOde.ForeColor = Color.White;
            btnNakitOde.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnNakitOde.BorderRadius = 10;
            btnNakitOde.Click += btnNakitOde_Click;
            btnNakitOde.Location = new Point(pnlSanalSepet.Width - 600, 7); // sağdan 160px

            // ➕ Kartla Öde butonu
            btnKartOde = new Guna2Button();
            btnKartOde.Text = "💳 Kartla Öde";
            btnKartOde.Width = 140;
            btnKartOde.Height = 35;
            btnKartOde.Anchor = AnchorStyles.Right | AnchorStyles.Top;
            btnKartOde.FillColor = Color.SteelBlue;
            btnKartOde.ForeColor = Color.White;
            btnKartOde.Font = new Font("Segoe UI", 9, FontStyle.Bold);
            btnKartOde.BorderRadius = 10;
            btnKartOde.Click += btnKartOde_Click;
            btnKartOde.Location = new Point(pnlSanalSepet.Width - 400, 7); // sağdan 10px
      
            // Butonları alt panele ekle
            pnlAltButonlar.Controls.Add(btnNakitOde);
            pnlAltButonlar.Controls.Add(btnKartOde);

            // Alt paneli ana sanal sepete ekle
            pnlSanalSepet.Controls.Add(pnlAltButonlar);
            pnlAltButonlar.BringToFront();
        }

        private void Form_MouseDown(object sender, MouseEventArgs e)
        {
            ReleaseCapture();
            SendMessage(this.Handle, 0x112, 0xf012, 0);
        }

        private void frmSatis_Load(object sender, EventArgs e)
        {
            UrunleriYukle();
        }

        private void UrunleriYukle(int offset = 0, int limit = 20, string arama = "", bool enCokSatan = false)
        {
            FlowLayoutPanel flp = this.Controls.Find("flpUrunler", true).FirstOrDefault() as FlowLayoutPanel;
            if (flp == null) return;

            if (offset == 0) flp.Controls.Clear(); // İlk yüklemede temizle

            UrunDAL urunDAL = new UrunDAL();
            List<Urun> urunler = urunDAL.UrunleriGetir1(offset, limit, arama, enCokSatan);

            foreach (var urun in urunler)
            {
                var kart = new Guna2CustomGradientPanel
                {
                    Size = new Size(90, 110),
                    BorderRadius = 12,
                    FillColor = Color.White,
                    Margin = new Padding(6),
                    Tag = urun,
                    Cursor = Cursors.Hand,
                    ShadowDecoration = { Enabled = true }
                };

                PictureBox pic = new PictureBox
                {
                    Size = new Size(64, 64),
                    SizeMode = PictureBoxSizeMode.Zoom,
                    Location = new Point((kart.Width - 60) / 2, 10),
                    BackColor = Color.Transparent
                };

                if (urun.Resim != null)
                {
                    using (MemoryStream ms = new MemoryStream(urun.Resim))
                    {
                        pic.Image = Image.FromStream(ms);
                    }
                }

                Label lbl = new Label
                {
                    Text = $"{urun.StokAdi}\n{urun.SatisFiyat:C}",
                    Font = new Font("Segoe UI", 9, FontStyle.Bold),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Size = new Size(kart.Width - 10, 40),
                    Location = new Point(5, 70),
                    BackColor = Color.Transparent
                };

                kart.Click += KartTiklandi;
                pic.Click += KartTiklandi;
                lbl.Click += KartTiklandi;

                kart.Controls.Add(pic);
                kart.Controls.Add(lbl);
                flp.Controls.Add(kart);
            }
        }

        /// <summary>
        /// Verilen ürünü sepete ekler veya sepette varsa adedini ve tutarını günceller.
        /// </summary>
        /// <param name="urun">Sepete eklenecek/güncellenecek Urun nesnesi.</param>
        /// <param name="eklenecekAdet">Eklenmek istenen adet.</param>
        private void SepeteEkleVeyaAdetArtir(Urun urun, int eklenecekAdet)
        {
            var dgv = this.Controls.Find("dgvSepet", true).FirstOrDefault() as Guna2DataGridView;
            if (dgv == null)
            {
                MessageBox.Show("Sepet DataGridView'ı (dgvSepet) bulunamadı!", "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // SepetUrun modeli yerine doğrudan DataGridViewRow üzerinde çalışıyoruz.
            DataGridViewRow mevcutSira = null;

            // Sepette aynı StokId'ye sahip ürün var mı kontrol et
            foreach (DataGridViewRow row in dgv.Rows)
            {
                // DataGridView'daki StokId sütununu kontrol et.
                // Convert.ToInt32 kullanıyoruz çünkü Value object tipindedir.
                if (row.Cells["StokId"] != null && row.Cells["StokId"].Value != null &&
                    Convert.ToInt32(row.Cells["StokId"].Value) == urun.StokId)
                {
                    mevcutSira = row; // Ürün bulundu
                    break;
                }
            }

            if (mevcutSira != null)
            {
                // Ürün sepette zaten var, adedini güncelle
                int mevcutAdet = Convert.ToInt32(mevcutSira.Cells["Adet"].Value);
                mevcutAdet += eklenecekAdet;
                mevcutSira.Cells["Adet"].Value = mevcutAdet;

                // Tutarı güncelle
                decimal fiyat = urun.SatisFiyat; // Güncel birim fiyatını Urun nesnesinden al
                decimal yeniTutar = fiyat * mevcutAdet;
                mevcutSira.Cells["Tutar"].Value = yeniTutar.ToString("C"); // "C" para birimi formatı
            }
            else
            {  // Ürün sepette yok, yeni satır olarak ekle
                decimal fiyat = urun.SatisFiyat;
                decimal tutar = fiyat * eklenecekAdet;

                // YENİ SÜTUN SIRASI: Sil Simgesi, StokId, UrunAdi, Adet, Fiyat, Tutar, Artir Simgesi, Azalt Simgesi
                dgv.Rows.Add(
                    "🗑️",             // Silme Simgesi (ilk sütun)
                    urun.StokId,      // StokId
                    urun.StokAdi,     // UrunAdi
                    eklenecekAdet,    // Adet
                    fiyat.ToString("C"), // Fiyat
                    tutar.ToString("C"), // Tutar
                    "➕",             // Artırma Simgesi
                    "➖"              // Azaltma Simgesi
                );
            }

            // Toplam tutarı güncelle
            GuncelToplamTutariHesaplaVeGoster();

            // nudAdet kontrolünü sıfırla (eğer kullanılıyorsa)
            var nudAdet = this.Controls.Find("nudAdet", true).FirstOrDefault() as Guna2NumericUpDown;
            if (nudAdet != null)
                nudAdet.Value = 1;
        }

        /// <summary>
        /// Sepetteki toplam tutarı hesaplar ve ilgili Label'ı günceller.
        /// </summary>
        private void GuncelToplamTutariHesaplaVeGoster()
        {
            var dgv = this.Controls.Find("dgvSepet", true).FirstOrDefault() as Guna2DataGridView;
            if (dgv == null) return;

            decimal toplam = 0;
            foreach (DataGridViewRow row in dgv.Rows)
            {
                if (row.Cells["Tutar"] != null && row.Cells["Tutar"].Value != null)
                {
                    // Tutar değerini string'den decimal'e çevirirken para birimi sembolünü temizle
                    string tutarText = row.Cells["Tutar"].Value.ToString().Replace("₺", "").Trim();
                    if (decimal.TryParse(tutarText, out decimal parsed))
                        toplam += parsed;
                }
            }

            var lblToplam = this.Controls.Find("lblToplam", true).FirstOrDefault() as Label;
            if (lblToplam != null)
                lblToplam.Text = $"Toplam: ₺{toplam:N2}"; // N2: Sayı formatı (binlik ayraçlı, 2 ondalıklı)
        }

        private void KartTiklandi(object sender, EventArgs e)
        {
            Control control = sender as Control;
            Guna2CustomGradientPanel panel = null;

            if (control is Guna2CustomGradientPanel)
                panel = (Guna2CustomGradientPanel)control;
            else
                panel = control.Parent as Guna2CustomGradientPanel;

            if (panel != null && panel.Tag is Urun urun) // panel.Tag'in Urun nesnesi olduğunu varsaydım
            {
                var nudAdet = this.Controls.Find("nudAdet", true).FirstOrDefault() as Guna2NumericUpDown;
                int adet = nudAdet != null ? (int)nudAdet.Value : 1;

                // Merkezi sepete ekleme metodunu çağır
                SepeteEkleVeyaAdetArtir(urun, adet);
            }
        }
        
        private void TxtBarkod_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string barkod = txtBarkod.Text.Trim();

                if (!string.IsNullOrEmpty(barkod))
                {
                    // UrunDAL kullanarak barkoda göre ürünü veritabanından çek
                    Urun bulunanUrun = _urunDAL.GetUrunByBarkod(barkod);

                    if (bulunanUrun != null)
                    {
                        // nudAdet kontrolünü bul ve değerini al
                        var nudAdet = this.Controls.Find("nudAdet", true).FirstOrDefault() as Guna2NumericUpDown;
                        // Eğer nudAdet bulunduysa değerini al, bulunamadıysa varsayılan olarak 1 adet al
                        int eklenecekAdet = nudAdet != null ? (int)nudAdet.Value : 1;

                        // Ürün bulunduysa, SepeteEkleVeyaAdetArtir metodunu çağır
                        // Şimdi nudAdet'ten gelen değeri kullanıyoruz!
                        SepeteEkleVeyaAdetArtir(bulunanUrun, eklenecekAdet);

                        txtBarkod.Clear(); // İşlem bitince barkod alanını temizle
                        txtBarkod.Focus(); // Yeni barkod girişi için odakla
                    }
                    else
                    {
                        MessageBox.Show($"'{barkod}' barkod numarasına sahip ürün bulunamadı.", "Ürün Bulunamadı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                        txtBarkod.Clear(); // Bulunamasa bile barkod alanını temizle
                        txtBarkod.Focus(); // Odakla
                    }
                }
                else
                {
                    // Boş barkod girildiğinde de temizle ve odakla
                    txtBarkod.Clear();
                    txtBarkod.Focus();
                }

                e.Handled = true;         // Tuş basım olayının başka kontrollere gitmesini engelle
                e.SuppressKeyPress = true; // Enter tuşunun Windows'taki varsayılan davranışını bastır
            }
        }

        private void IlkSepetiHazirla()
        {
            toplamSepetTutari = 0;
            parcaliSatisDetaylari.Clear();

            foreach (DataGridViewRow row in dgvSepet.Rows)
            {
                if (row.IsNewRow) continue;

                int stokId = Convert.ToInt32(row.Cells["StokId"].Value);
                int adet = Convert.ToInt32(row.Cells["Adet"].Value);

                string fiyatText = row.Cells["Fiyat"].Value.ToString().Replace("₺", "").Trim();
                decimal fiyat = decimal.Parse(fiyatText);

                decimal tutar = fiyat * adet;
                toplamSepetTutari += tutar;

                parcaliSatisDetaylari.Add(new SatisDetay
                {
                    StokId = stokId,
                    Adet = adet,
                    Fiyat = fiyat,
                    Tutar = tutar
                });
            }

            sepetYuklendi = true;
        }
        private void OdemeYap(string odemeYontemi)
        {
            if (dgvSepet.Rows.Count == 0)
            {
                MessageBox.Show("Sepet boş!", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🧠 İlk kez ödeme yapılıyorsa, sepet detayları hazırlanır
            if (!sepetYuklendi)
                IlkSepetiHazirla(); // parcaliSatisDetaylari ve toplamSepetTutari hesaplanır

            // 🔸 Kalan ödeme tutarı
            decimal kalan = toplamSepetTutari - odenenTutar;
            decimal girilenTutar = kalan;

            // 💬 TextBox doluysa özel tutar işlenir
            if (txtOdenecekTutar != null && !string.IsNullOrWhiteSpace(txtOdenecekTutar.Text))
            {
                string girilen = txtOdenecekTutar.Text.Replace("₺", "").Trim();

                if (!decimal.TryParse(girilen, out girilenTutar) || girilenTutar <= 0)
                {
                    MessageBox.Show("Geçerli bir ödeme tutarı girin.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (girilenTutar > kalan)
                {
                    MessageBox.Show($"Girdiğiniz tutar kalan borçtan fazla olamaz.\nKalan: ₺{kalan:N2}", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }

            // 🎯 İlk ödeme mi?
            bool ilkOdeme = (odenenTutar == 0);

            // 📎 Referans kod üret (ilk ödemede)
            if (ilkOdeme && string.IsNullOrEmpty(aktifReferansKod))
                aktifReferansKod = Guid.NewGuid().ToString();

            // 🧾 Satış nesnesi hazırlanır
            Satis satis = new Satis
            {
                Tarih = DateTime.Now,
                OdemeYontemi = odemeYontemi,
                ToplamTutar = girilenTutar,
                Detaylar = parcaliSatisDetaylari,
                ReferansKod = aktifReferansKod
            };

            try
            {
                // 💾 Satışı veritabanına kaydet
                SatisDAL.SatisYap(satis, stoktanDus: ilkOdeme);

                // 💰 Ödenen tutarı güncelle
                odenenTutar += girilenTutar;
                lblOdenen.Text = $"Ödenen: ₺{odenenTutar:N2}";
                txtOdenecekTutar.Clear();

                // ✅ Satış tamamlandıysa form temizlenir
                if (odenenTutar >= toplamSepetTutari)
                {
                    MessageBox.Show("Satış tamamlandı.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    dgvSepet.Rows.Clear();
                    lblOdenen.Text = "Ödenen: ₺0.00";
                    GuncelToplamTutariHesaplaVeGoster();
                    txtOdenecekTutar.Clear();
                    parcaliSatisDetaylari.Clear();
                    odenenTutar = 0;
                    toplamSepetTutari = 0;
                    sepetYuklendi = false;
                    aktifReferansKod = null;
                    
                }
                else
                {
                    decimal yeniKalan = toplamSepetTutari - odenenTutar;
                    MessageBox.Show($"Kalan ödeme: ₺{yeniKalan:N2}", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Satış sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        private void SanalSepetOdemeYap(string odemeYontemi)
        {
            if (flpSanalSepet.Controls.Count == 0)
            {
                MessageBox.Show("Sanal sepette ürün bulunmuyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<SatisDetay> detaylar = new List<SatisDetay>();
            decimal toplamTutar = 0;

            // 🔁 Sanal sepetteki ürünleri topla
            foreach (Guna2Panel urunPanel in flpSanalSepet.Controls.OfType<Guna2Panel>())
            {
                Label lblInfo = urunPanel.Controls.Find("lblInfo", true).FirstOrDefault() as Label;
                if (lblInfo == null || lblInfo.Tag == null) continue;

                UrunVeri veri = lblInfo.Tag as UrunVeri;

                // Satış detayını oluştur
                SatisDetay detay = new SatisDetay
                {
                    StokId = veri.StokId,
                    Adet = veri.Adet,
                    Fiyat = veri.Fiyat,
                    Tutar = veri.Tutar
                };

                detaylar.Add(detay);
                toplamTutar += veri.Tutar;
            }

            if (detaylar.Count == 0)
            {
                MessageBox.Show("Sanal sepet boş gibi görünüyor.", "Uyarı", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 🔹 Satış nesnesini oluştur
            Satis satis = new Satis
            {
                Tarih = DateTime.Now,
                OdemeYontemi = odemeYontemi,
                ToplamTutar = toplamTutar,
                Detaylar = detaylar,
                ReferansKod = Guid.NewGuid().ToString().Substring(0, 8).ToUpper() // isteğe bağlı
            };

            try
            {
                // 🔸 Satışı veritabanına kaydet
                SatisDAL.SatisYap(satis, stoktanDus: true);

                // 🔻 dgvSepet'ten ilgili ürünleri azalt
                foreach (var detay in detaylar)
                {
                    foreach (DataGridViewRow row in dgvSepet.Rows)
                    {
                        if (row.Cells["StokId"].Value == null) continue;

                        int rowStokId = Convert.ToInt32(row.Cells["StokId"].Value);
                        if (rowStokId == detay.StokId)
                        {
                            int mevcutAdet = Convert.ToInt32(row.Cells["Adet"].Value);
                            int yeniAdet = mevcutAdet - detay.Adet;

                            if (yeniAdet <= 0)
                            {
                                dgvSepet.Rows.Remove(row);
                            }
                            else
                            {
                                row.Cells["Adet"].Value = yeniAdet;
                                row.Cells["Tutar"].Value = yeniAdet * detay.Fiyat;
                            }

                            break;
                        }
                    }
                }

                // ✅ Temizlik: Sanal sepeti temizle
                flpSanalSepet.Controls.Clear();
                pnlSanalSepet.Visible = false;
                GuncelToplamTutariHesaplaVeGoster();
                lblToplamTutar.Text = "₺0.00";

                MessageBox.Show("Seçili ürünler başarıyla ödendi.", "Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Sanal ödeme sırasında hata oluştu:\n" + ex.Message, "Hata", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }


        // Sanal sepet butonu
        private void btnNakitOde_Click(object sender, EventArgs e)
        {
            SanalSepetOdemeYap("Nakit");
        }
        // Sanal Sepet butonu
        private void btnKartOde_Click(object sender, EventArgs e)
        {
            SanalSepetOdemeYap("Kart");
        } 

        //dgvsepet butonu
        private void btnNakit_Click(object sender, EventArgs e)
        {
            OdemeYap("Nakit");
        }
        //dgvsepet butonu
        private void btnKart_Click(object sender, EventArgs e)
        {
            OdemeYap("Kart");
        }  


    }
}
