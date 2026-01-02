using BigDataOrdersDashboard.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory; // Cache kütüphanesini ekleyin

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardMainChartComponentPartial : ViewComponent
    {
        private readonly BigDataOrdersDbContext _context;
        private readonly IMemoryCache _memoryCache; // Cache servisi

        public _DashboardMainChartComponentPartial(BigDataOrdersDbContext context, IMemoryCache memoryCache)
        {
            _context = context;
            _memoryCache = memoryCache;
        }

        public async Task<IViewComponentResult> InvokeAsync() // ASENKRON YAPILDI
        {
            // Cache Anahtarı
            string cacheKey = "DashboardMainChartData";

            // Veri Cache'te yoksa Veritabanından çek, varsa Cache'ten al
            if (!_memoryCache.TryGetValue(cacheKey, out DashboardDataDto cachedData))
            {
                // Timeout Süresini Artır (Veritabanı büyük olduğu için)
                _context.Database.SetCommandTimeout(180);

                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                var sixMonthsAgo = today.AddMonths(-6);

                // --- 1. SORGUNUN OPTİMİZASYONU (BUGÜNÜN SATIŞLARI) ---
                // 3 kere sorgu atmak yerine, bugünün verisini TEK seferde çekip hafızada grupluyoruz.

                var todaysOrders = await _context.Orders
                    .AsNoTracking() // Okuma hızı için takibi kapat
                    .Where(o => o.OrderDate >= today && o.OrderDate < tomorrow)
                    .Join(_context.Products,
                          order => order.ProductId,
                          product => product.ProductId,
                          (order, product) => new
                          {
                              order.OrderStatus,
                              TotalAmount = order.Quantity * product.UnitPrice
                          })
                    .ToListAsync();

                // Hafızadaki veriyi filtreleyip topluyoruz (Veritabanına tekrar gitmez)
                cachedData = new DashboardDataDto();

                cachedData.TodaySalesCompleted = Math.Round(todaysOrders
                    .Where(x => x.OrderStatus == "Tamamlandı").Sum(x => x.TotalAmount), 2);

                cachedData.TodaySalesShipped = Math.Round(todaysOrders
                    .Where(x => x.OrderStatus == "Kargoda").Sum(x => x.TotalAmount), 2);

                cachedData.TodaySalesPreparing = Math.Round(todaysOrders
                    .Where(x => x.OrderStatus == "Hazırlanıyor").Sum(x => x.TotalAmount), 2);


                // --- 2. SORGUNUN OPTİMİZASYONU (GRAFİK VERİSİ) ---
                var monthlySalesRaw = await _context.Orders
                    .AsNoTracking()
                    .Where(o => o.OrderDate >= sixMonthsAgo)
                    .GroupBy(o => new { o.OrderDate.Year, o.OrderDate.Month, o.OrderStatus })
                    .Select(g => new
                    {
                        Year = g.Key.Year,
                        Month = g.Key.Month,
                        Durum = g.Key.OrderStatus,
                        SatisAdedi = g.Count()
                    })
                    .OrderBy(x => x.Year)
                    .ThenBy(x => x.Month)
                    .ToListAsync();

                var monthlySalesFormatted = monthlySalesRaw.Select(x => new
                {
                    Ay = $"{x.Year}-{x.Month:D2}",
                    x.Durum,
                    x.SatisAdedi
                }).ToList();

                cachedData.MonthlySalesJson = System.Text.Json.JsonSerializer.Serialize(monthlySalesFormatted);

                // Veriyi 10 dakika Cache'e atıyoruz
                var cacheOptions = new MemoryCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(10));
                _memoryCache.Set(cacheKey, cachedData, cacheOptions);
            }

            // ViewBag atamaları
            ViewBag.TodaySales = cachedData.TodaySalesCompleted;
            ViewBag.TodaySalesShipped = cachedData.TodaySalesShipped;
            ViewBag.TodaySalesPreparing = cachedData.TodaySalesPreparing;
            ViewBag.MonthlySalesJson = cachedData.MonthlySalesJson;

            return View();
        }

        // Cache için basit bir DTO sınıfı (Sınıf içine veya dışına koyabilirsiniz)
        private class DashboardDataDto
        {
            public decimal TodaySalesCompleted { get; set; }
            public decimal TodaySalesShipped { get; set; }
            public decimal TodaySalesPreparing { get; set; }
            public string MonthlySalesJson { get; set; }
        }
    }
}