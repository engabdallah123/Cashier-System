using Inventory.Domain;
using Inventory.Domain.Batches.ProductBatches;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Batches.ProductBatches.Commands.CreateProductBatch
{
    internal sealed class CreateProductBatchCommandHandler : ICommandHandler<CreateProductBatchCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreateProductBatchCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateProductBatchCommand request, CancellationToken cancellationToken)
        {
            var batchResult = ProductBatch.Create(
                request.ProductId,
                request.WarehouseId,
                request.BatchNumber,
                request.ExpiryDate,
                request.Quantity);

            if (batchResult.IsFailure)
                return Result<Guid>.Failure(batchResult.Error);

            var batch = batchResult.Value!;

            await _unitOfWork.ProductBatchRepository.AddAsync(batch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(batch.Id);
        }
    }
}
