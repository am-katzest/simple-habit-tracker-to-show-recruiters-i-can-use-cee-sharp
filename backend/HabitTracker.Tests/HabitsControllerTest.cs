using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using static System.Net.HttpStatusCode;

namespace HabitTracker.Tests;
public class HabitsControllerTest(HostFixture fixture) : IClassFixture<HostFixture>
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

    [Fact]
    public async Task CompletionTypeAdd()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = new CompletionTypeData("#333333", "name", null);
        Assert.Equal(0, (await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes"))?.Count);
        var id = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        var res = await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes");
        var b = await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes");
        Assert.NotNull(b);
        var r = b.Single();
        Assert.NotNull(r);
        Assert.Equal(ct.Color, r.Color);
        Assert.Equal(ct.Description, r.Description);
        Assert.Equal(ct.Name, r.Name);
    }
    [Fact]
    public async Task CompletionTypeAddValidation()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = new CompletionTypeData("", "name", null);
        String[] colors = ["", "wrong-color", "#abcde", "#", "#abcdeg", "#0123456"];
        foreach (var color in colors)
        {
            Assert.Equal(BadRequest, (await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct with { Color = color })).StatusCode);
        }
    }

    [Fact]
    public async Task CompletionTypeRemovePositive()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = new CompletionTypeData("#333333", "name", null);
        var id = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        var id2 = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        Assert.Equal(2, (await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes"))?.Count);
        await c.DeleteAsync($"api/habits/{h}/CompletionTypes/{id}");
        Assert.Equal(1, (await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes"))?.Count);
        await c.DeleteAsync($"api/habits/{h}/CompletionTypes/{id2}");
        Assert.Equal(0, (await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes"))?.Count);
    }

    [Fact]
    public async Task CompletionTypeUpdate()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = new CompletionTypeData("#333333", "name", null);
        var ct2 = new CompletionTypeData("#444444", "other", "something");
        var id = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        await c.PutAsJsonAsync($"api/habits/{h}/CompletionTypes/{id}", ct2);
        var updated = (await c.GetFromJsonAsync<List<CompletionTypeDataId>>($"api/habits/{h}/CompletionTypes"))?.Single();
        Assert.NotNull(updated);
        Assert.Equal(ct2.Color, updated.Color);
        Assert.Equal(ct2.Description, updated.Description);
        Assert.Equal(ct2.Name, updated.Name);
    }
    [Fact]
    public async Task CompletionTypeErrors()
    {
        using var c = fixture.Client;
        var ct = new CompletionTypeData("#333333", "name", null);
        var h = await c.NewHabit();
        await AssertHelpers.ReturnedError<NoSuchHabitException>(await c.PostAsJsonAsync($"api/habits/1313/CompletionTypes", ct));
        var id = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        await AssertHelpers.ReturnedError<NoSuchHabitException>(await c.DeleteAsync($"api/habits/53135/CompletionTypes/{id}"));
        await AssertHelpers.ReturnedError<NoSuchCompletionTypeException>(await c.DeleteAsync($"api/habits/{h}/CompletionTypes/531313"));
    }

    private CompletionData baseCompletion = new(null, DateTime.Now, true, null, null);

    [Fact]
    public async Task CompletionPostGetRoundtripTest()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = await c.NewCompletionType(h);
        var completion = baseCompletion with { CompletionTypeId = ct };
        await AssertHelpers.ReturnedError<NoSuchHabitException>(await c.PostAsJsonAsync($"api/habits/1313/Completions/", completion));
        await AssertHelpers.ReturnedError<NoSuchCompletionTypeException>(await c.PostAsJsonAsync($"api/habits/{h}/Completions/", completion with { CompletionTypeId = 13513 }));
        var id = await c.PostAsJsonAsync($"api/habits/{h}/Completions/", completion).Id();
        var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/");
        Assert.NotNull(ret);
        Assert.Single(ret);
        Assert.Equal(id, ret.First().Id);
    }
    [Fact]
    public async Task CompletionGetFilteringTest()
    {
        using var c = fixture.Client;
        var h = await c.NewHabit();
        var d = DateTime.Now;
        Task<int> newCompletion(int hours) => c.PostAsJsonAsync($"api/habits/{h}/Completions/", new CompletionData(null, d.AddHours(hours), true, null, null)).Id();

        var id1 = await newCompletion(1);
        var id2 = await newCompletion(2);
        var id3 = await newCompletion(3);

        string t(double x) => d.AddHours(x).ToString("o").Replace("+", "%2B");

        Assert.True((await c.GetAsync($"api/habits/{h}/Completions/")).IsSuccessStatusCode);
        Assert.True((await c.GetAsync($"api/habits/{h}/Completions/?after={t(0)}")).IsSuccessStatusCode);
        Assert.True((await c.GetAsync($"api/habits/{h}/Completions/?before={t(0)}")).IsSuccessStatusCode);
        Assert.True((await c.GetAsync($"api/habits/{h}/Completions/?before={t(0)}&after={t(0)}")).IsSuccessStatusCode);

        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/");
            Assert.NotNull(ret);
            Assert.Equal(3, ret.Count);
        }

        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?after={t(0)}");
            Assert.NotNull(ret);
            Assert.Equal(3, ret.Count);
        }

        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?after={t(0)}&before={t(4)}");
            Assert.NotNull(ret);
            Assert.Equal(3, ret.Count);
        }

        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?after={t(2.5)}");
            Assert.NotNull(ret);
            Assert.Single(ret);
            Assert.Equal(id3, ret.First().Id);
        }

        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?before={t(1.5)}");
            Assert.NotNull(ret);
            Assert.Single(ret);
            Assert.Equal(id1, ret.First().Id);
        }
        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?before={t(2.5)}&after={t(1.5)}");
            Assert.NotNull(ret);
            Assert.Single(ret);
            Assert.Equal(id2, ret.First().Id);
        }
        {
            var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/?before={t(3.5)}&limit=2");
            Assert.NotNull(ret);
            Assert.Equal(2, ret.Count);
        }
    }


    private async Task<(CompletionDataId?, HttpResponseMessage)> CompletionAfterDeletingCompletionType(CompletionData completion, string query)
    {

        using var c = fixture.Client;
        var h = await c.NewHabit();
        var ct = new CompletionTypeData("#333333", "name", null);
        var ctid = await c.PostAsJsonAsync($"api/habits/{h}/CompletionTypes", ct).Id();
        await c.PostAsJsonAsync($"api/habits/{h}/Completions/", completion with { CompletionTypeId = ctid });
        var deletionResult = await c.DeleteAsync($"api/habits/{h}/CompletionTypes/{ctid}/" + query);
        var ret = await c.GetFromJsonAsync<List<CompletionDataId>>($"api/habits/{h}/Completions/");
        var remainingCompletion = (ret) switch { [CompletionDataId a] => a, _ => null, };
        return (remainingCompletion, deletionResult);
    }

    [Fact]
    public async Task CompletionTypeRemoveNegative()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion, "");
        await AssertHelpers.ReturnedError<UnableToDeleteCompletionTypeWithExistingCompletions>(response);
        Assert.NotNull(completion);
    }

    [Fact]
    public async Task CompletionTypeRemoveDeleteCompletions()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion, "?DeleteCompletions=true");
        Assert.True(response.IsSuccessStatusCode);
        Assert.Null(completion);
    }

    [Fact]
    public async Task CompletionTypeRemoveSetColorNever()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Color = null }, "?ColorStrategy=NeverReplace&DeleteCompletions=false");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.Null(completion.Color);
    }

    [Fact]
    public async Task CompletionTypeRemoveSetColorAlways()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Color = "#444444" }, "?ColorStrategy=AlwaysReplace&DeleteCompletions=false");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.NotEqual("#444444", completion.Color);
    }

    [Fact]
    public async Task CompletionTypeRemoveSetColorConditionallyColorPresent()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Color = "#444444" }, "?ColorStrategy=ReplaceOnlyIfNotSet&DeleteCompletions=false");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.Equal("#444444", completion.Color);
    }
    [Fact]
    public async Task CompletionTypeRemoveSetColorConditionallyColorAbsent()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Color = null }, "?ColorStrategy=ReplaceOnlyIfNotSet&DeleteCompletions=False");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.NotNull(completion.Color);
    }
    [Fact]
    public async Task CompletionTypeRemoveLeaveNoteAlone()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Note = "note" }, "?DeleteCompletions=false");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.Equal("note", completion.Note);
    }
    [Fact]
    public async Task CompletionTypeRemoveAddNote()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Note = null }, "?Note=meow&DeleteCompletions=false");
        Assert.True(response.IsSuccessStatusCode);
        Assert.NotNull(completion);
        Assert.Equal("meow", completion.Note);
    }
    [Fact]
    public async Task CompletionTypeRemoveStrategyValidation()
    {
        var (completion, response) = await CompletionAfterDeletingCompletionType(baseCompletion with { Note = null }, "?Note=meow&DeleteCompletions=true");
        Assert.False(response.IsSuccessStatusCode);
    }
}
