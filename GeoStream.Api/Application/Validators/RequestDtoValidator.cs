using FluentValidation;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.Application.Validators.Route
{
    public class RequestDtoValidator : AbstractValidator<RequestDto>
    {
        public RequestDtoValidator()
        {
            RuleFor(x => x.OrderDirection)
                .Must(direction => string.IsNullOrWhiteSpace(direction) || direction is "asc" or "desc")
                .WithMessage(ValidationMessages.InvalidOrderDirection);

            RuleFor(x => x.OrderDirection)
                .Empty()
                .When(x => string.IsNullOrWhiteSpace(x.OrderBy))
                .WithMessage(ValidationMessages.OrderDirectionWithoutOrderByError);
        }
    }
}