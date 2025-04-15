using FluentValidation;
using GeoStream.Api.Application.Dtos.Route;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Route
{
    public class CreateRouteDtoValidator : AbstractValidator<CreateRouteDto>
    {
        public CreateRouteDtoValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage(ValidationMessages.RequiredFieldError)
                .MaximumLength(100).WithMessage(string.Format(ValidationMessages.FieldMaxLengthError, 100));

            RuleFor(x => x.StatusId)
                .Must(value => Enum.IsDefined(typeof(Domain.Enums.Status), value))
                .WithMessage(x => string.Format(
                    ValidationMessages.InvalidEnumError,
                    nameof(Domain.Enums.Status),
                    string.Join(", ", Enum.GetNames(typeof(Domain.Enums.Status))))
                );
        }
    }
}