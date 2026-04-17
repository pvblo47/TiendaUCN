using TiendaUCN.src.Application.DTOs.BaseResponse;

namespace TiendaUCN.src.Application.DTOs.BaseResponse
{
    public class ErrorDetail(string message, string? details = null)

    {
        public string Message { get; set; } = message;
        public string? Details { get; set; } = details;
    }
}