using BigDataOrdersDashboard.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Text.Json;

namespace BigDataOrdersDashboard.ViewComponents.CustomerDetailViewComponents
{
    public class _CustomerDetailAIAnalysisByLastOrdersComponentPartial : ViewComponent
    {
        private readonly BigDataOrdersDbContext _context;
        private readonly HttpClient _client;

        private const string GeminiApiKey = "api key";
        private const string GeminiModel = "gemini-2.5-flash";
        private const string GeminiBaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/";

        public _CustomerDetailAIAnalysisByLastOrdersComponentPartial(BigDataOrdersDbContext context, HttpClient client)
        {
            _context = context;
            _client = client;
        }
        public async Task<IViewComponentResult> InvokeAsync(int id)
        {
            id = 8;

            var customer = _context.Customers
                .Include(c => c.Orders)
                .ThenInclude(o => o.Product)
                .ThenInclude(p => p.Category)
                .Where(c => c.CustomerId == id)
                .Select(c => new
                {
                    c.CustomerName,
                    c.CustomerSurname,
                    Orders = c.Orders
                    .OrderByDescending(o => o.OrderDate)
                    .Take(20)
                    .Select(o => new
                    {
                        o.OrderDate,
                        Product = o.Product.ProductName,
                        Category = o.Product.Category.CategoryName,
                        o.Quantity,
                        o.Product.UnitPrice,
                        TotalPrice = o.Quantity * o.Product.UnitPrice
                    })
                }).FirstOrDefault();

            if (customer == null) return Content("Müşteri bulunamadı.");

            var jsonData = JsonSerializer.Serialize(customer);


            string systemInstruction = "Sen bir veri analisti ve müşteri davranış uzmanısın.";
            string prompt = $@"
                               ⚠️ Çok önemli:
                               Kesinlikle ``` (backtick) veya kod bloğu verme.
                               Sadece saf HTML üret. Markdown verme. Kod bloğu verme.
                               
                               {systemInstruction}
                               Aşağıdaki veriyi analiz et ve sonucu HTML formatında ver.
                               
                               Bu başlıkları kullan (sırasını ve isimleri değiştirme):
                               
                               <h4>👤 Müşteri Profili</h4>
                               <p><b>Ad:</b> ...</p>
                               <p><b>Soyad:</b> ...</p>
                               <p><b>Toplam Sipariş:</b> ...</p>
                               <p><b>Toplam Harcama:</b> ...</p>
                               
                               <h4>🛍️ Ürün Tercihleri</h4>
                               <ul>
                                 <li>🏠 Ev & Dekorasyon – X sipariş</li>
                                 <li>💄 Kozmetik – X sipariş</li>
                               </ul>
                               <p><b>Öne çıkan ürünler:</b></p>
                               <ul>
                                 <li>Ürün adı (adet — fiyat)</li>
                               </ul>
                               
                               <h4>⏰ Zaman Bazlı Alışveriş Davranışı</h4>
                               <p>En yoğun ay: ...</p>
                               <p>En yoğun gün: ...</p>
                               <p>Favori saat aralığı: ...</p>
                               
                               <h4>💰 Ortalama Harcama ve Sıklık</h4>
                               <p>Aylık ortalama sipariş: ...</p>
                               <p>Ortalama sepet tutarı: ...</p>
                               <p>En yüksek sipariş: ...</p>
                               <p>En düşük sipariş: ...</p>
                               
                               <h4>🎯 Sadakat ve Tekrar Harcama Eğilimi</h4>
                               <p>Tekrar alışveriş eğilimi: ...</p>
                               <p>Marka sadakati: ...</p>
                               <p>Kategori sadakati: ...</p>
                               
                               <h4>🚀 Pazarlama Önerileri</h4>
                               <ul>
                                 <li>🎁 Kampanya önerisi: ...</li>
                                 <li>✉️ Hedefli e-posta: ...</li>
                                 <li>🆕 Yeni ürün tanıtımı önerisi: ...</li>
                               </ul>
                               
                               Veri:
                               {jsonData}
                               ";


            var url = $"{GeminiBaseUrl}{GeminiModel}:generateContent?key={GeminiApiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7f,
                    maxOutputTokens = 10000
                }
            };

            var jsonRequest = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(jsonRequest, Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);

            string completion = "";

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();

                using var doc = JsonDocument.Parse(responseString);

                try
                {
                    completion = doc.RootElement
                                                .GetProperty("candidates")[0]
                                                .GetProperty("content")
                                                .GetProperty("parts")[0]
                                                .GetProperty("text")
                                                .GetString();
                }
                catch
                {
                    completion = "Analiz oluşturulurken veri formatı hatası oluştu.";
                }
            }
            else
            {
                var errorMsg = await response.Content.ReadAsStringAsync();
                completion = $"API Hatası: {errorMsg}";
            }

            string[] sections = completion?.Split("<h4>") ?? Array.Empty<string>();

            ViewBag.AnalysisSection1 = "<h4>" + sections.ElementAtOrDefault(1);
            ViewBag.AnalysisSection2 = "<h4>" + sections.ElementAtOrDefault(2);
            ViewBag.AnalysisSection3 = "<h4>" + sections.ElementAtOrDefault(3);
            ViewBag.AnalysisSection4 = "<h4>" + sections.ElementAtOrDefault(4);
            ViewBag.AnalysisSection5 = "<h4>" + sections.ElementAtOrDefault(5);
            ViewBag.AnalysisSection6 = "<h4>" + sections.ElementAtOrDefault(6);

            return View();
        }
    }
}