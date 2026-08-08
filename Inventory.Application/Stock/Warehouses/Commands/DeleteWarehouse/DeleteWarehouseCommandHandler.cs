using Inventory.Domain;
using Inventory.Domain.Stock.Warehouses;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.Warehouses.Commands.DeleteWarehouse
{
    internal sealed class DeleteWarehouseCommandHandler : ICommandHandler<DeleteWarehouseCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public DeleteWarehouseCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _unitOfWork.WarehouseRepository.FindAsync(w => w.Id == request.Id);
            if (warehouse is null)
                return Result.Failure(WarehouseErrors.NotFound(request.Id));

            _unitOfWork.WarehouseRepository.Delete(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
