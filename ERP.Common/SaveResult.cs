namespace ERP.Common;

public class SaveResult
{
    public int Count { get; set; }
    public int ErrorNo { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public object Data { get; set; }

    public SaveResult(bool success)
    {
        Success = success;
    }

    public SaveResult(bool success, string message)
    {
        Success = success;
        Message = message;
    }

    public SaveResult(bool success, string message, int errorNo)
    {
        Success = success;
        Message = message;
        ErrorNo = errorNo;
    }
}