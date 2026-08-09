namespace POS.Desktop.Services.State
{
    public class CartItemModel
    {
        public Guid ProductId { get; set; }
        public string Barcode { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public decimal Quantity { get; set; } = 1;
        public decimal UnitPrice { get; set; }
        public decimal Discount { get; set; }
        public decimal TaxRate { get; set; }

        public decimal LineSubtotal => Quantity * UnitPrice;
        public decimal LineTax => (LineSubtotal - Discount) * (TaxRate / 100m);
        public decimal LineTotal => LineSubtotal - Discount + LineTax;
    }

    public class CartStateContainer
    {
        public List<CartItemModel> Items { get; } = new();
        public Guid? CustomerId { get; set; }
        public string CustomerName { get; set; } = "Walk-in Customer";
        public decimal OverallDiscount { get; set; }
        public string PaymentMethod { get; set; } = "Cash";

        public event Action? OnCartChanged;

        public void AddOrIncrementProduct(Guid productId, string barcode, string name, decimal price, decimal taxRate = 0)
        {
            var existing = Items.FirstOrDefault(i => i.ProductId == productId);
            if (existing is not null)
            {
                existing.Quantity += 1;
            }
            else
            {
                Items.Add(new CartItemModel
                {
                    ProductId = productId,
                    Barcode = barcode,
                    ProductName = name,
                    UnitPrice = price,
                    Quantity = 1,
                    TaxRate = taxRate
                });
            }
            NotifyStateChanged();
        }

        public void UpdateQuantity(Guid productId, decimal quantity)
        {
            var item = Items.FirstOrDefault(i => i.ProductId == productId);
            if (item is not null)
            {
                if (quantity <= 0)
                {
                    Items.Remove(item);
                }
                else
                {
                    item.Quantity = quantity;
                }
                NotifyStateChanged();
            }
        }

        public void RemoveItem(Guid productId)
        {
            Items.RemoveAll(i => i.ProductId == productId);
            NotifyStateChanged();
        }

        public void Clear()
        {
            Items.Clear();
            CustomerId = null;
            CustomerName = "Walk-in Customer";
            OverallDiscount = 0;
            PaymentMethod = "Cash";
            NotifyStateChanged();
        }

        public decimal SubTotal => Items.Sum(i => i.LineSubtotal);
        public decimal ItemDiscounts => Items.Sum(i => i.Discount);
        public decimal TotalDiscount => ItemDiscounts + OverallDiscount;
        public decimal TotalTax => Items.Sum(i => i.LineTax);
        public decimal GrandTotal => Math.Max(0, SubTotal - TotalDiscount + TotalTax);

        private void NotifyStateChanged() => OnCartChanged?.Invoke();
    }
}
