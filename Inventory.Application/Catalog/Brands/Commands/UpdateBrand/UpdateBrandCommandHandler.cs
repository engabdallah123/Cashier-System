using Inventory.Domain;
using Inventory.Domain.Catalog.Brands;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Brands.Commands.UpdateBrand
{
    internal sealed class UpdateBrandCommandHandler : ICommandHandler<UpdateBrandCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public UpdateBrandCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateBrandCommand request, CancellationToken cancellationToken)
        {
            var brand = await _unitOfWork.BrandRepository.FindAsync(b => b.Id == request.Id);
            if (brand is null)
                return Result.Failure(BrandErrors.NotFound(request.Id));

            var renameRes = brand.Rename(request.Name);
            if (renameRes.IsFailure)
                return renameRes;

            _unitOfWork.BrandRepository.Update(brand);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
