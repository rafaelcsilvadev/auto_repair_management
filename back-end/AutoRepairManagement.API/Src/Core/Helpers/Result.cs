namespace AutoRepairManagement.API.Core.Helpers;

public class Result
{
       public object? Data { get; }
       public string? Message { get; }
       public List<string>? Error { get; }
       public int? Page { get; }
       public int? PageTotal { get; }
       public int Status { get; }

       private Result(object? data, int status, int? page, int? pageTotal)
       {
           Status = status;
           Page = page;
           PageTotal = pageTotal;
           Data = data;
       }

       private Result(object? data, int status)
       {
           Status = status;
           Data = data;
       }

       private Result(string message, int status)
       {
           Status = status;
           Message = message;
       }

       private Result(List<string>? error, int status)
       {
           Status = status;
           Error = error;
       }

       public static Result Ok(object? data, int? page, int? pageTotal) 
           => page is null ?
               new(data, StatusCodes.Status200OK) : 
               new(data, StatusCodes.Status200OK, page, pageTotal);
       public static Result Created() 
           => new("Created Success", StatusCodes.Status201Created);
       public static Result NotFound(List<string>? error) => new(error, StatusCodes.Status404NotFound);
       public static Result Conflict(List<string>? error) => new(error, StatusCodes.Status409Conflict);
       public static Result BadRequest(List<string>? error) => new(error, StatusCodes.Status400BadRequest);
       public static Result InternalServerError(List<string>? error) => 
           new(error, StatusCodes.Status500InternalServerError);
};
