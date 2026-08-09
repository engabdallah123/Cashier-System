using Expenses.Domain;
using Expenses.Domain.Expenses.Entities;
using POS.Shared.Application.Messaging;
using POS.Shared.Domain;

namespace Expenses.Application.Expenses.Commands.CreateExpense
{
    internal sealed class CreateExpenseCommandHandler : ICommandHandler<CreateExpenseCommand, Guid>
    {
        private readonly IExpensesUnitOfWork _unitOfWork;

        public CreateExpenseCommandHandler(IExpensesUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<Guid>> Handle(CreateExpenseCommand request, CancellationToken cancellationToken)
        {
            var expenseResult = Expense.Create(
                request.Title, request.Amount, request.CreatedByUserId,
                request.Description, request.ExpenseDate, request.Notes);

            if (expenseResult.IsFailure)
                return Result<Guid>.Failure(expenseResult.Error);

            var expense = expenseResult.Value!;
            await _unitOfWork.ExpenseRepository.AddAsync(expense);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<Guid>.Success(expense.Id);
        }
    }
}
