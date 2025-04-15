using FluentValidation;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Route
{
    public class PaginationRequestDtoValidator : AbstractValidator<PaginationRequestDto>
    {
        public PaginationRequestDtoValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.FieldMustBeGreaterThanZeroError);

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage(ValidationMessages.FieldMustBeGreaterThanZeroError);
        }
    }
}