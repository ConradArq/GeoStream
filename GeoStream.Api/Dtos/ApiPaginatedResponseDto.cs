using System.Net;
using System.Text.Json.Serialization;
using GeoStream.Api.Application.Dtos;
using GeoStream.Api.Application.Resources;

namespace GeoStream.Api.API.Dtos
{
    public class ApiPaginatedResponseDto<TData> : ApiResponseDto<TData>
    {
        [JsonPropertyOrder(100)]
        public Pagination pagination { get; set; }

        public ApiPaginatedResponseDto(int pageNumber, int pageSize, int totalItems, int totalPages, HttpStatusCode statusCode, string? message, TData? data, List<string>? errors)
            : base(statusCode, message, data, errors)
        {
            pagination = new Pagination
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = totalItems,
                TotalPages = totalPages
            };
        }

        public static ApiPaginatedResponseDto<TData> Ok(PaginatedResponseDto<TData?> responseDto)
        {
            return new ApiPaginatedResponseDto<TData>(
                responseDto.PageNumber,
                responseDto.PageSize,
                responseDto.TotalItems,
                responseDto.TotalPages,
                HttpStatusCode.OK,
                responseDto.Message ?? GeneralHttpStatusMessages.OperationCompletedSuccessfully,
                responseDto.Data,
                null
            );
        }

        public class Pagination
        {
            public int PageNumber { get; set; }
            public int PageSize { get; set; }
            public int TotalItems { get; set; }
            public int TotalPages { get; set; }
        }
    }
}
