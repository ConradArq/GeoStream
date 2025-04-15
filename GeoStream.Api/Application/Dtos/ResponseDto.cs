
namespace GeoStream.Api.Application.Dtos
{
    public class ResponseDto<TData>
    {
        /// <summary>
        /// Creates a new instance of <see cref="ResponseDto{TData}"/> with the provided data and an optional message.
        /// </summary>
        /// <param name="data">The data being returned.</param>
        /// <param name="message">An optional informational message that may be returned from the application layer to the Api layer.</param>
        /// <returns>A new <see cref="ResponseDto{TData}"/> instance containing the specified data and message.</returns>
        public ResponseDto(TData? data = default, string? message = null)
        {
            Data = data;
            Message = message;
        }

        public string? Message { get; set; }
        public TData? Data { get; }


        public static ResponseDto<TData> ConvertToBase<TDerived>(ResponseDto<TDerived> response) where TDerived : TData
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response), "Response cannot be null.");
            }

            return new ResponseDto<TData>(response.Data, response.Message);
        }

        public static ResponseDto<TTarget> ConvertToDerived<TTarget>(ResponseDto<TData> response) where TTarget : TData
        {
            if (response == null)
            {
                throw new ArgumentNullException(nameof(response), "Response cannot be null.");
            }
            if (response.Data == null)
            {
                throw new InvalidCastException("The data inside the response cannot be null.");
            }
            if (response.Data is TTarget targetData)
            {
                return new ResponseDto<TTarget>(targetData, response.Message);
            }

            throw new InvalidCastException($"The Data inside the response is not of type {typeof(TTarget).Name}.");
        }
    }
}
