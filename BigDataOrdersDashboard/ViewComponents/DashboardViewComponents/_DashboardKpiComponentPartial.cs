using BigDataOrdersDashboard.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // Async metotlar için bunu eklemelisin

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardKpiComponentPartial : ViewComponent
    {
        private readonly BigDataOrdersDbContext _context;

        public _DashboardKpiComponentPartial(BigDataOrdersDbContext context)
        {
            _context = context;
        }

        // 1. DÜZELTME: Invoke yerine InvokeAsync yaptık (Timeout yememek için)
        public async Task<IViewComponentResult> InvokeAsync()
        {
            // Timeout süresini bu işlem için artırıyoruz
            _context.Database.SetCommandTimeout(120);

            #region Kpi_1 (Günlük Siparişler)
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);
            var yesterday = today.AddDays(-1);

            // Tarih aralığı (Range) sorgusu ile performans artırdık
            var todayOrderCount = await _context.Orders
                .CountAsync(x => x.OrderDate >= today && x.OrderDate < tomorrow);

            var yesterdayOrderCount = await _context.Orders
                .CountAsync(x => x.OrderDate >= yesterday && x.OrderDate < today);

            // İkon Belirleme
            if (todayOrderCount > yesterdayOrderCount)
            {
                ViewBag.TrendingIcon = "zmdi zmdi-trending-up float-right";
            }
            else
            {
                ViewBag.TrendingIcon = "zmdi zmdi-trending-down float-right";
            }

            // 2. DÜZELTME: SIFIRA BÖLME HATASI KONTROLÜ
            decimal changeRate = 0;
            if (yesterdayOrderCount > 0)
            {
                changeRate = ((decimal)(todayOrderCount - yesterdayOrderCount) / yesterdayOrderCount) * 100;
            }
            else
            {
                // Eğer dün hiç sipariş yoksa ve bugün varsa artış %100 kabul edilebilir veya 0 geçilir.
                changeRate = todayOrderCount > 0 ? 100 : 0;
            }

            // Renk Belirleme
            if (changeRate < 0)
            {
                ViewBag.ChangeRateColor = "red";
            }
            else
            {
                ViewBag.ChangeRateColor = "green";
            }

            // Ortalama Hesabı (Hata önleyici)
            double dailyAverageOrders = 1; // Varsayılan 1
            try
            {
                // Tablo boşsa Average hata verir, try-catch ile koruyoruz
                var avgResult = await _context.Orders
                    .GroupBy(x => x.OrderDate.Date)
                    .Select(g => g.Count())
                    .AverageAsync();

                if (avgResult > 0) dailyAverageOrders = avgResult;
            }
            catch
            {
                // Hata olursa varsayılan 1 kalır, sistem çökmez.
            }

            double ratio = (todayOrderCount / dailyAverageOrders) * 100.0;

            ViewBag.TodayVsAverageRatio = Math.Round(ratio, 2);
            ViewBag.TodayOrderCount = todayOrderCount;
            ViewBag.DailyOrderChange = Math.Round(changeRate, 2);

            #endregion

            #region Kpi_2 (İptal Oranları)

            var sevenDaysAgo = today.AddDays(-7);

            var totalOrders7Days = await _context.Orders
                .CountAsync(x => x.OrderDate >= sevenDaysAgo && x.OrderDate < tomorrow);

            var cancelledOrders7Days = await _context.Orders
                .CountAsync(x => x.OrderStatus == "İptal Edildi" && x.OrderDate >= sevenDaysAgo && x.OrderDate < tomorrow);

            // 3. DÜZELTME: SIFIRA BÖLME KONTROLÜ
            decimal cancelRate = 0;
            if (totalOrders7Days > 0)
            {
                cancelRate = ((decimal)cancelledOrders7Days / totalOrders7Days) * 100;
            }

            ViewBag.CancelledOrders7Days = cancelledOrders7Days;
            ViewBag.CancelRate = Math.Round(cancelRate, 2);
            ViewBag.CancelColor = "red";
            ViewBag.CancelText = cancelRate > 5 ? "Yüksek İptal Oranı ⚠️" : "Normal Düzeyde";

            #endregion

            #region Kpi_3 (Tamamlanma Oranı)

            var totalOrders = await _context.Orders.CountAsync();
            var completedOrders = await _context.Orders.CountAsync(x => x.OrderStatus == "Tamamlandı");

            // 4. DÜZELTME: SIFIRA BÖLME KONTROLÜ
            decimal completionRate = 0;
            if (totalOrders > 0)
            {
                completionRate = ((decimal)completedOrders / totalOrders) * 100;
            }

            ViewBag.CompletionRate = Math.Round(completionRate, 2);
            ViewBag.CompletedOrders = completedOrders;
            ViewBag.CompletionText = completionRate >= 80 ? "Mükemmel Performans 💪" : "İyileşme Devam Ediyor 📈";

            #endregion

            return View();
        }
    }
}