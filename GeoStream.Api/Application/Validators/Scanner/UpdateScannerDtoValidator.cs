using FluentValidation;
using GeoStream.Api.Application.Dtos.Scanner;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Route
{
    public class UpdateScannerDtoValidator : AbstractValidator<UpdateScannerDto>
    {
        public UpdateScannerDtoValidator()
        {
            RuleFor(x => x.Code)
                .MaximumLength(100).WithMessage(string.Format(ValidationMessages.FieldMaxLengthError, 100));

            RuleFor(x => x.StatusId)
                .Must(value => value == null || Enum.IsDefined(typeof(Domain.Enums.Status), value))
                .WithMessage(x => string.Format(
                    ValidationMessages.InvalidEnumError,
                    nameof(Domain.Enums.Status),
                    string.Join(", ", Enum.GetNames(typeof(Domain.Enums.Status))))
                );
        }
    }
}