using Inventory.Domain;
using Inventory.Domain.Stock.Warehouses;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Stock.Warehouses.Commands.CreateWarehouse
{
    internal sealed class CreateWarehouseCommandHandler : ICommandHandler<CreateWarehouseCommand, Guid>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public CreateWarehouseCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateWarehouseCommand request, CancellationToken cancellationToken)
        {
            var warehouseResult = Warehouse.Create(request.Name, request.Code, request.Address);
            if (warehouseResult.IsFailure)
                return Result<Guid>.Failure(warehouseResult.Error);

            var warehouse = warehouseResult.Value!;

            await _unitOfWork.WarehouseRepository.AddAsync(warehouse);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(warehouse.Id);
        }
    }
}
