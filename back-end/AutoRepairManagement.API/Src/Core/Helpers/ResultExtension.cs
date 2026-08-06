namespace AutoRepairManagement.API.Core.Helpers;

public static class ResultExtension
{
    public static IResult ToHttpResult(this Result result, string? location = null)
    {
        return result.Status switch
        {
            StatusCodes.Status204NoContent
                or StatusCodes.Status200OK => Results.Ok(new {
                    Status = result.Status,
                    Page = result.Page,
                    PageTotal = result.PageTotal,
                    Data = result.Data,
                }),
            StatusCodes.Status201Created =>
                Results.Created($"{location}", new
                {
                    Status = result.Status,
                    Message = result.Message,
                }),
            StatusCodes.Status404NotFound =>
                Results.NotFound(new {Status = result.Status, Message = result.Error}),
            StatusCodes.Status409Conflict =>
                Results.Conflict(new {Status = result.Status, Message = result.Error}),
            StatusCodes.Status400BadRequest =>
                Results.BadRequest(new {Status = result.Status, Message = result.Error}),
            _ => Results.InternalServerError(new {Status = result.Status, Message = result.Error}),
        };
    }
}
