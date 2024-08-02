using HabitTracker.Exceptions;

namespace HabitTracker.Tests;

public static class AssertHelpers
{
    public static async Task ReturnedError<T>(HttpResponseMessage r) where T : UserVisibleException, new()
    {
        var expected = new T();
        Assert.Equal(expected.Code, r.StatusCode);
        var body = await r.Content.ReadFromJsonAsync<String>();
        Assert.Equal(expected.ErrorMessage, body);
    }
}
