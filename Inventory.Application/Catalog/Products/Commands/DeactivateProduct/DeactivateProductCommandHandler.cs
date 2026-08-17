using Inventory.Domain;
using Inventory.Domain.Catalog.Products.Errors;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Products.Commands.DeactivateProduct
{
    internal sealed class DeactivateProductCommandHandler : ICommandHandler<DeactivateProductCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public DeactivateProductCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeactivateProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _unitOfWork.ProductRepository.GetByIdAsync(request.Id);
            if (product is null)
                return Result.Failure(ProductErrors.NotFound(request.Id));

            var result = product.Deactivate();
            if (result.IsFailure)
                return result;

            _unitOfWork.ProductRepository.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
