using Inventory.Domain;
using Inventory.Domain.Stock.Warehouses;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.Warehouses.Commands.UpdateWarehouse
{
    internal sealed class UpdateWarehouseCommandHandler : ICommandHandler<UpdateWarehouseCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public UpdateWarehouseCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(UpdateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouse = await _unitOfWork.WarehouseRepository.FindAsync(w => w.Id == request.Id);
            if (warehouse is null)
                return Result.Failure(WarehouseErrors.NotFound(request.Id));

            var updateRes = warehouse.UpdateInfo(request.Name, request.Code, request.Address);
            if (updateRes.IsFailure)
                return updateRes;

            _unitOfWork.WarehouseRepository.Update(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
