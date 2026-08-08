using POS.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Stock.StockMovements
{
    public static class StockMovementErrors
    {
        public static readonly Error ZeroQuantity =
            new("StockMovement.ZeroQuantity", "لا يمكن تسجيل حركة مخزون بكمية صفر.");

        public static readonly Error InconsistentBalance =
            new("StockMovement.InconsistentBalance", "الرصيد بعد الحركة لا يطابق (قبل + الكمية).");

        public static readonly Error ReferenceTypeRequired =
            new("StockMovement.ReferenceTypeRequired", "نوع المرجع مطلوب.");

        public static readonly Error CreatedByRequired =
            new("StockMovement.CreatedByRequired", "بيانات المستخدم المنفذ للحركة مطلوبة.");
    }
}
