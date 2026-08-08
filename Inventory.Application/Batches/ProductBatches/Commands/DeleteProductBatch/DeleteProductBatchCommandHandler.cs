using Inventory.Domain;
using Inventory.Domain.Batches.ProductBatches;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Batches.ProductBatches.Commands.DeleteProductBatch
{
    internal sealed class DeleteProductBatchCommandHandler : ICommandHandler<DeleteProductBatchCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public DeleteProductBatchCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteProductBatchCommand request, CancellationToken cancellationToken)
        {
            var batch = await _unitOfWork.ProductBatchRepository.FindAsync(b => b.Id == request.Id);
            if (batch is null)
                return Result.Failure(ProductBatchErrors.NotFound(request.Id));

            _unitOfWork.ProductBatchRepository.Delete(batch);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
