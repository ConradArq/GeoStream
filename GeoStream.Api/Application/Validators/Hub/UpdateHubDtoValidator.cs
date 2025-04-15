using FluentValidation;
using GeoStream.Api.Application.Dtos.Hub;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Hub
{
    public class UpdateHubDtoValidator : AbstractValidator<UpdateHubDto>
    {
        public UpdateHubDtoValidator()
        {
            RuleFor(x => x.Name)
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