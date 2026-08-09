using FluentValidation;

namespace Identity.Application.Auth.Commands.Register
{
    internal sealed class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        private static readonly string[] AllowedRoles = { "Admin", "Manager", "Cashier" };

        public RegisterCommandValidator()
        {
            RuleFor(x => x.FullName).NotEmpty().MaximumLength(100).WithMessage("الاسم الكامل مطلوب ولا يتجاوز 100 حرف.");
            RuleFor(x => x.UserName).NotEmpty().MaximumLength(50).WithMessage("اسم المستخدم مطلوب ولا يتجاوز 50 حرف.");
            RuleFor(x => x.Email).NotEmpty().EmailAddress().WithMessage("البريد الإلكتروني مطلوب وبصيغة صحيحة.");
            RuleFor(x => x.Password).NotEmpty().MinimumLength(6).WithMessage("كلمة المرور مطلوبة ولا تقل عن 6 أحرف.");
            RuleFor(x => x.Role).NotEmpty().Must(r => AllowedRoles.Contains(r))
                .WithMessage("الدور يجب أن يكون Admin أو Manager أو Cashier.");
        }
    }
}
