namespace TiendaUCN.src.Application.DTOs.BaseResponse;

public class GenericResponse<T>(string message, T? data = default)
{
    public string Message { get; set; } = message;
    public T? Data { get; set; } = data;
}



