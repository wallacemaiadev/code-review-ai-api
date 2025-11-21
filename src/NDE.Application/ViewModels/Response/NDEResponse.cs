namespace NDE.Application.ViewModels.Response
{
  public class NDEResponseModel<T>
  {
    public bool Success { get; set; }
    public T? Data { get; set; }
    public IEnumerable<string>? Errors { get; set; }

    public static NDEResponseModel<T> Ok(T? data) =>
        new NDEResponseModel<T> { Success = true, Data = data };

    public static NDEResponseModel<T> Fail(IEnumerable<string> errors) =>
        new NDEResponseModel<T> { Success = false, Errors = errors };
  }

  public class NDEPagedResponseModel<T> : NDEResponseModel<IEnumerable<T>>
  {
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    public static NDEPagedResponseModel<T> Ok(IEnumerable<T> items, int page, int pageSize, int totalCount) =>
      new NDEPagedResponseModel<T>
      {
        Success = true,
        Data = items,
        Page = page,
        PageSize = pageSize,
        TotalCount = totalCount
      };

    public new static NDEPagedResponseModel<T> Fail(IEnumerable<string> errors) =>
        new NDEPagedResponseModel<T> { Success = false, Errors = errors };
  }
}