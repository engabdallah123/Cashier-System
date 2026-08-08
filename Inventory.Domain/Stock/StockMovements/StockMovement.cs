using POS.Shared.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Inventory.Domain.Stock.StockMovements
{

    // StockMovement عبارة عن سجل تدقيق (Audit Log) بحت — بيتسجل مرة واحدة ومبيتعدلش
    // بعد كده أبدًا. المصدر الحقيقي للرصيد اللحظي هو Product.QuantityOnHand، وده
    // بيتحسب سريع من غير ما تجمع كل الحركات في كل مرة.
    public sealed class StockMovement : Entity
    {
        public Guid ProductId { get; private set; }
        public Guid WarehouseId { get; private set; }
        public StockMovementType Type { get; private set; }

        // Delta موقّع (موجب = دخول، سالب = خروج) عشان AfterQuantity = BeforeQuantity + Quantity
        public int Quantity { get; private set; }
        public int BeforeQuantity { get; private set; }
        public int AfterQuantity { get; private set; }

        public string ReferenceType { get; private set; } = default!;
        public Guid? ReferenceId { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public string CreatedBy { get; private set; } = default!;

        private StockMovement() { } // EF Core

        private StockMovement(
            Guid id, Guid productId, Guid warehouseId, StockMovementType type,
            int quantity, int beforeQuantity, int afterQuantity,
            string referenceType, Guid? referenceId, string createdBy)
            : base(id)
        {
            ProductId = productId;
            WarehouseId = warehouseId;
            Type = type;
            Quantity = quantity;
            BeforeQuantity = beforeQuantity;
            AfterQuantity = afterQuantity;
            ReferenceType = referenceType;
            ReferenceId = referenceId;
            CreatedBy = createdBy;
            CreatedAt = DateTime.UtcNow;
        }

        public static Result<StockMovement> Create(
            Guid productId,
            Guid warehouseId,
            StockMovementType type,
            int quantity,
            int beforeQuantity,
            int afterQuantity,
            string referenceType,
            Guid? referenceId,
            string createdBy)
        {
            if (quantity == 0)
                return Result<StockMovement>.Failure(StockMovementErrors.ZeroQuantity);

            if (beforeQuantity + quantity != afterQuantity)
                return Result<StockMovement>.Failure(StockMovementErrors.InconsistentBalance);

            if (string.IsNullOrWhiteSpace(referenceType))
                return Result<StockMovement>.Failure(StockMovementErrors.ReferenceTypeRequired);

            if (string.IsNullOrWhiteSpace(createdBy))
                return Result<StockMovement>.Failure(StockMovementErrors.CreatedByRequired);

            var movement = new StockMovement(
                Guid.NewGuid(), productId, warehouseId, type,
                quantity, beforeQuantity, afterQuantity,
                referenceType.Trim(), referenceId, createdBy.Trim());

            return Result<StockMovement>.Success(movement);
        }
    }
}
