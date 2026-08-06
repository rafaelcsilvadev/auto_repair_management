using AutoRepairManagement.API.Core.Helpers;
using Microsoft.AspNetCore.Http;

namespace AutoRepairManagement.Test.Core;

public class ResultExtensionTests
{
    [Theory]
    [InlineData(StatusCodes.Status200OK)]
    [InlineData(StatusCodes.Status201Created)]
    [InlineData(StatusCodes.Status404NotFound)]
    [InlineData(StatusCodes.Status409Conflict)]
    [InlineData(StatusCodes.Status400BadRequest)]
    [InlineData(StatusCodes.Status500InternalServerError)]
    public void ToHttpResult_MapsResultStatusToMatchingHttpStatusCode(int status)
    {
        // Arrange
        var result = BuildResultWithStatus(status);

        // Act
        var httpResult = result.ToHttpResult();

        // Assert
        var statusCodeResult = Assert.IsAssignableFrom<IStatusCodeHttpResult>(httpResult);
        Assert.Equal(status, statusCodeResult.StatusCode);
    }

    [Fact]
    public void ToHttpResult_WhenStatusIsCreated_UsesGivenLocation()
    {
        // Arrange
        var result = Result.Created();

        // Act
        // Results.Created(uri, value) closes Created<TValue> over an internal anonymous
        // TValue that can't be named here (and is opaque to `dynamic`, which enforces C#
        // accessibility rules), so Location is read through plain reflection instead.
        var httpResult = result.ToHttpResult(location: "/clients/1");
        var location = httpResult.GetType().GetProperty("Location")?.GetValue(httpResult);

        // Assert
        Assert.Equal("/clients/1", location);
    }

    private static Result BuildResultWithStatus(int status) => status switch
    {
        StatusCodes.Status200OK => Result.Ok(data: "payload", page: null, pageTotal: null),
        StatusCodes.Status201Created => Result.Created(),
        StatusCodes.Status404NotFound => Result.NotFound(["Not found"]),
        StatusCodes.Status409Conflict => Result.Conflict(["Conflict"]),
        StatusCodes.Status400BadRequest => Result.BadRequest(["Invalid"]),
        _ => Result.InternalServerError(["Internal Server Error"]),
    };
}
