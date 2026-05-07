namespace Domain.Contracts.API
{
    public class SuccessResponse<T>
    {
        public bool success { get; set; }
        public T data { get; set; }
    }
}
