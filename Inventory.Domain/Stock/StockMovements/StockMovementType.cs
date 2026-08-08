using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Stock.StockMovements
{
    public enum StockMovementType
    {
        Purchase = 1,     // شراء من مورد - زيادة
        Sale = 2,          // بيع - نقصان
        Return = 3,         // مرتجع عميل - زيادة
        Adjustment = 4,      // تسوية يدوية (جرد) - زيادة أو نقصان
        TransferIn = 5,       // تحويل وارد من مخزن تاني
        TransferOut = 6,        // تحويل صادر لمخزن تاني
        Damaged = 7,              // تالف/هالك - نقصان
        InitialStock = 8           // رصيد افتتاحي
    }
}
