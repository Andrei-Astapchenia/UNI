using Stoky_programm.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Stoky_programm.Tests
{
    public static class MaterialObjTests
    {
        public static void TestProducts()
        {
            Console.WriteLine("=== Тестирование класса Products ===");

            // Создание объектов предприятия
            var companyCar = new EnterpriseAsset(
                name: "Служебный автомобиль",
                unit: MaterialObj.UnitType.Piece,
                quantity: 3, // 3 одинаковых автомобиля
                price: 200000,
                asset: EnterpriseAsset.AssetType.Vehicle,
                condition: EnterpriseAsset.Conditions.Good,
                inventoryNumber: "1001",
                commissioningDate: new DateTime(2023, 5, 15)
            );

            var officeFurniture = new EnterpriseAsset(
                name: "Офисный стол",
                unit: MaterialObj.UnitType.Piece,
                quantity: 10, // 10 столов
                price: 15000,
                asset: EnterpriseAsset.AssetType.Furniture,
                condition: EnterpriseAsset.Conditions.New,
                inventoryNumber: "2001",
                commissioningDate: DateTime.Now
            );

            // Работа с методами
            Console.WriteLine(companyCar.ToString());

            // Повреждение
            companyCar.Damaged();
            Console.WriteLine($"После повреждения: {companyCar.GetCondition()}");

            // Ремонт
            companyCar.Repair();
            Console.WriteLine($"После ремонта: {companyCar.GetCondition()}");

            // Проверка обслуживания
            if (companyCar.NeedsService())
                Console.WriteLine("Требуется обслуживание!");
        }
    }
}
