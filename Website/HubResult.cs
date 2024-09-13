namespace Website;

public class HubResult
{
    public static HubResult Success(string? message = null) => new HubResult(true, message);


    public HubResult()
    {
        IsSuccess = true;
    }

    public static HubResult Failure(string? message = null) => new HubResult(false, message);
    protected HubResult(bool isSuccess,string? message = null)
    {
        IsSuccess = isSuccess;
        Message = message;
    }
    public string? Message { get;  set; }
    public bool IsSuccess { get; set; }
   
}

public class HubResult< TItem> : HubResult 
{

    public new static  HubResult<TItem> Failure(string? message = null)
    {
       var theResult =  new HubResult<TItem>();
       theResult.Message = message;
       theResult.IsSuccess = false;
       return theResult;
    }


    // Implicit conversion from HubResult to HubResult<TItem>
    public static implicit operator HubResult<TItem>(TItem result)
    {
        return Success(result);
    }


    public  static HubResult<TItem> Success(TItem item, string? message = null)
    {
        var theResult =  new HubResult<TItem>();
        theResult.Message = message;
        theResult.IsSuccess = true;
        theResult.Item = item;
        return theResult;
    }

    public HubResult() : base(false)
    {
        
    }
    public TItem? Item { get; set; }
}