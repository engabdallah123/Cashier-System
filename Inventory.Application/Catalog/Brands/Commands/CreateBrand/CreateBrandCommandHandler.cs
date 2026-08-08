using Inventory.Domain;
using Inventory.Domain.Catalog.Brands;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Brands.Commands.CreateBrand
{
    internal sealed class CreateBrandCommandHandler : ICommandHandler<CreateBrandCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreateBrandCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateBrandCommand request, CancellationToken cancellationToken)
        {
            var brandResult = Brand.Create(request.Name);
            if (brandResult.IsFailure)
                return Result<Guid>.Failure(brandResult.Error);

            var brand = brandResult.Value!;

            await _unitOfWork.BrandRepository.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(brand.Id);
        }
    }
}
