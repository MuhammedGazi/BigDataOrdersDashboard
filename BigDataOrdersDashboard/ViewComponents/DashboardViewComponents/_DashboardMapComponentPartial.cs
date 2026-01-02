using BigDataOrdersDashboard.Context;
using BigDataOrdersDashboard.Models; // DTO'larınızın olduğu namespace
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data.Common; // DbDataReader için gerekli

namespace BigDataOrdersDashboard.ViewComponents.DashboardViewComponents
{
    public class _DashboardMapComponentPartial : ViewComponent
    {
        private readonly BigDataOrdersDbContext _context;

        public _DashboardMapComponentPartial(BigDataOrdersDbContext context)
        {
            _context = context;
        }

        public async Task<IViewComponentResult> InvokeAsync() // ASYNC YAPILDI
        {
            var result = new List<CountryReportDto>();

            // Bağlantıyı EF Core üzerinden alıyoruz
            var connection = _context.Database.GetDbConnection();

            // Bağlantıyı manuel açıyoruz (Async olarak)
            await _context.Database.OpenConnectionAsync();

            using (var command = connection.CreateCommand())
            {
                // --- KRİTİK AYAR: Timeout süresini 180 saniye (3 dk) yapıyoruz ---
                command.CommandTimeout = 180;

                command.CommandText = @"
                    SELECT 
                        t1.CustomerCountry AS Country,
                        t1.Total2023,
                        t2.Total2024,
                        CAST(((t2.Total2024 - t1.Total2023) * 100.0 / t1.Total2023) AS DECIMAL(5,2)) AS ChangeRate
                    FROM
                    (
                        SELECT 
                            c.CustomerCountry, 
                            COUNT(*) AS Total2023
                        FROM Orders o WITH(NOLOCK) -- Performans için NOLOCK eklendi
                        INNER JOIN Customers c WITH(NOLOCK) ON o.CustomerId = c.CustomerId
                        WHERE o.OrderDate >= '2023-01-01' AND o.OrderDate < '2024-01-01'
                        GROUP BY c.CustomerCountry
                    ) AS t1
                    INNER JOIN
                    (
                        SELECT 
                            c.CustomerCountry, 
                            COUNT(*) AS Total2024
                        FROM Orders o WITH(NOLOCK)
                        INNER JOIN Customers c WITH(NOLOCK) ON o.CustomerId = c.CustomerId
                        WHERE o.OrderDate >= '2024-01-01' AND o.OrderDate < '2025-01-01'
                        GROUP BY c.CustomerCountry
                    ) AS t2
                    ON t1.CustomerCountry = t2.CustomerCountry";

                // Okuma işlemini de ASYNC yapıyoruz
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var countryName = reader.GetString(0);

                        result.Add(new CountryReportDto
                        {
                            Country = countryName,
                            Total2023 = reader.GetInt32(1),
                            Total2024 = reader.GetInt32(2),
                            ChangeRate = reader.GetDecimal(3),
                            // Koordinat servisiniz statik olduğu için aynen kalabilir
                            Latitude = CountryCoordinates.GetLat(countryName),
                            Longitude = CountryCoordinates.GetLon(countryName)
                        });
                    }
                }
            }

            // Bağlantı 'using' bloğu bitince veya context dispose olunca kapanır ama 
            // EF Core manuel açılan bağlantıları bazen açık bırakabilir, garantiye alalım:
            await _context.Database.CloseConnectionAsync();

            return View(result);
        }
    }
}