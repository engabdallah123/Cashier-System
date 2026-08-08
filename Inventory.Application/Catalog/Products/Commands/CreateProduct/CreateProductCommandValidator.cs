using FluentValidation;

namespace Inventory.Application.Catalog.Products.Commands.CreateProduct
{
    internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
    {
        public CreateProductCommandValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("اسم المنتج مطلوب.");
            RuleFor(x => x.Sku).NotEmpty().MaximumLength(50).WithMessage("رمز SKU مطلوب ولا يتجاوز 50 حرفاً.");
            RuleFor(x => x.Price).GreaterThanOrEqualTo(0).WithMessage("السعر لا يمكن أن يكون سالباً.");
            RuleFor(x => x.LowStockThreshold).GreaterThanOrEqualTo(0).WithMessage("حد المخزون لا يمكن أن يكون سالباً.");
        }
    }
}
