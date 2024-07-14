using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using static System.Net.HttpStatusCode;

namespace HabitTracker.Tests;
public class HabitControllerTest(HostFixture fixture) : IClassFixture<HostFixture>
{
    [Fact]
    public async Task HabitCreationAndGetting()
    {
        using var c = fixture.Client;
        await c.RegisterNewUniqueUser();
        var ans1 = await c.PostAsJsonAsync("api/habits/", new HabitNameDescription("habit", "desc"));
        var bd1 = await ans1.Content.ReadAsStringAsync();
        Assert.Equal(OK, ans1.StatusCode);
        var ans2 = await c.PostAsJsonAsync("api/habits/", new HabitNameDescription("habit2", null));
        var bd2 = await ans2.Content.ReadAsStringAsync();
        Assert.Equal(OK, ans2.StatusCode);
        Assert.NotEqual(bd1, bd2);
        var id = Int32.Parse(bd1);
        var ret1 = await c.GetFromJsonAsync<HabitNameDescriptionId>($"api/habits/{id}");
        Assert.NotNull(ret1);
        Assert.Equal(id, ret1.Id);
        Assert.Equal("desc", ret1.Description);
        Assert.Equal("habit", ret1.Name);
        var both = await c.GetFromJsonAsync<List<HabitNameId>>("api/habits/");
        Assert.NotNull(both);
        Assert.Equal(2, both.Count);
        var first = both.Where(x => x.Id != id).Single();
        var second = both.Where(x => x.Id == id).Single();
        Assert.Equal("habit2", first.Name);
        Assert.Equal("habit", second.Name);
    }

    [Fact]
    public async Task HabitDeletion()
    {
        using var c = fixture.Client;
        await c.RegisterNewUniqueUser();
        var ans1 = await c.PostAsJsonAsync("api/habits/", new HabitNameDescription("habit", "desc"));
        var bd1 = await ans1.Content.ReadAsStringAsync();
        Assert.Equal(OK, ans1.StatusCode);
        Assert.Equal(1, (await c.GetFromJsonAsync<List<HabitNameId>>("api/habits/"))?.Count);
        await AssertHelpers.ReturnedError<NoSuchHabitException>(await c.DeleteAsync("api/habits/1561854"));
        Assert.Equal(1, (await c.GetFromJsonAsync<List<HabitNameId>>("api/habits/"))?.Count);
        var ans2 = await c.DeleteAsync($"api/habits/{bd1}");
        Assert.True(ans2.IsSuccessStatusCode);
        Assert.Equal(0, (await c.GetFromJsonAsync<List<HabitNameId>>("api/habits/"))?.Count);
    }

    [Fact]
    public async Task HabitUpdate()
    {
        using var c = fixture.Client;
        await c.RegisterNewUniqueUser();
        var ans1 = await c.PostAsJsonAsync("api/habits/", new HabitNameDescription("habit", "desc"));
        var bd1 = await ans1.Content.ReadAsStringAsync();
        Assert.Equal(OK, ans1.StatusCode);
        await AssertHelpers.ReturnedError<NoSuchHabitException>(await c.PutAsJsonAsync("api/habits/1561854", new HabitNameDescription("new", null)));
        var ans2 = await c.PutAsJsonAsync($"api/habits/{bd1}", new HabitNameDescription("new", null));
        Assert.True(ans2.IsSuccessStatusCode);
        var ret1 = await c.GetFromJsonAsync<HabitNameDescriptionId>($"api/habits/{bd1}");
        Assert.NotNull(ret1);
        Assert.Null(ret1.Description);
        Assert.Equal("new", ret1.Name);
    }
}
