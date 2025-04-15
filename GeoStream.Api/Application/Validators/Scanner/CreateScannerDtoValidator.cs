using FluentValidation;
using GeoStream.Api.Application.Dtos.Scanner;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Scanner
{
    public class CreateScannerDtoValidator : AbstractValidator<CreateScannerDto>
    {
        public CreateScannerDtoValidator()
        {
            RuleFor(x => x.Code)
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