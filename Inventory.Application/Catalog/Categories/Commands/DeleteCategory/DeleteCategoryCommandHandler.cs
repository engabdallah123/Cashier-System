using Inventory.Domain;
using Inventory.Domain.Catalog.Categories;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Inventory.Application.Catalog.Categories.Commands.DeleteCategory
{
    internal sealed class DeleteCategoryCommandHandler : ICommandHandler<DeleteCategoryCommand>
    {
        private readonly IInventoryUnitOfWork _unitOfWork;

        public DeleteCategoryCommandHandler(IInventoryUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(DeleteCategoryCommand request, CancellationToken cancellationToken)
        {
            var category = await _unitOfWork.CategoryRepository.FindAsync(c => c.Id == request.Id);
            if (category is null)
                return Result.Failure(CategoryErrors.NotFound(request.Id));

            _unitOfWork.CategoryRepository.Delete(category);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success();
        }
    }
}
