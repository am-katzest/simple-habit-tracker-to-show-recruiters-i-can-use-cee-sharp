using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Services;

namespace HabitTracker.Tests;

public class HabitServiceTest(UserFixture fixture) : IClassFixture<UserFixture>
{
    private IHabitService MakeService() => new HabitService(fixture.MakeContext());
    private async Task<HabitId> MakeHabit(UserId u) { return await MakeService().addHabit(new("name", null), u); }
    private async Task<CompletionTypeId> MakeCT(HabitId h) { return await MakeService().AddCompletionType(h, new("#333333", "meow", "meow")); }

    [Fact]
    public async Task CreatingHabit()
    {
        var u = fixture.MakeUser();
        Assert.Empty(await MakeService().getHabits(u));
        var habit = new HabitNameDescription("habit", null);
        var hid = await MakeService().addHabit(habit, u);
        Assert.Single(await MakeService().getHabits(u));
        Assert.Equal("habit", (await MakeService().getHabits(u)).First()?.Name);
        Assert.Equal(hid.Id, (await MakeService().getHabits(u)).First()?.Id);
    }
    [Fact]
    public async Task GettingHabitPositive()
    {
        var u = fixture.MakeUser();
        Assert.Empty(await MakeService().getHabits(u));
        {
            var habit = new HabitNameDescription("habit", null);
            var hid = await MakeService().addHabit(habit, u);
            var received = await MakeService().getHabitDetails(hid);
            Assert.NotNull(received);
            Assert.Equal(hid.Id, received.Id);
            Assert.Equal("habit", received.Name);
            Assert.Null(received.Description);
        }
        Assert.Single(await MakeService().getHabits(u));
        {
            var habit = new HabitNameDescription("habit2", "desc");
            var hid = await MakeService().addHabit(habit, u);
            var received = await MakeService().getHabitDetails(hid);
            Assert.NotNull(received);
            Assert.Equal(hid.Id, received.Id);
            Assert.Equal("habit2", received.Name);
            Assert.Equal("desc", received.Description);
        }
        Assert.Equal(2, (await MakeService().getHabits(u)).Count);
    }

    [Fact]
    public async Task GettingHabitNotOwn()
    {
        var u1 = fixture.MakeUser();
        var u2 = fixture.MakeUser();
        Assert.NotEqual(u1, u2);
        var habit = new HabitNameDescription("habit", null);
        var hid = await MakeService().addHabit(habit, u1);
        var counterfeit = new HabitId(hid.Id, u2);
        await Assert.ThrowsAsync<NoSuchHabitException>(async () => await MakeService().getHabitDetails(counterfeit));
    }
    [Fact]
    public async Task DeletingHabit()
    {
        var u = fixture.MakeUser();
        var habit = new HabitNameDescription("habit", null);
        var hid = await MakeService().addHabit(habit, u);
        var hid2 = await MakeService().addHabit(new("habit2", null), u);
        Assert.Equal(2, (await MakeService().getHabits(u)).Count);
        await MakeService().RemoveHabit(hid);
        Assert.Single(await MakeService().getHabits(u));
        await MakeService().RemoveHabit(hid2);
        Assert.Empty(await MakeService().getHabits(u));
    }
    [Fact]
    public async Task UpdatingHabit()
    {
        var u = fixture.MakeUser();
        var hid = await MakeHabit(u);
        await MakeService().UpdateHabit(hid, new("other", "meow"));
        Assert.Single(await MakeService().getHabits(u));
        Assert.Equal("other", (await MakeService().getHabitDetails(hid)).Name);
        Assert.Equal("meow", (await MakeService().getHabitDetails(hid)).Description);
        await MakeService().UpdateHabit(hid, new("other", null));
        Assert.Null((await MakeService().getHabitDetails(hid)).Description);
    }
    [Fact]
    public async Task CompletionTypeExceptionTest()
    {
        var u = fixture.MakeUser();
        HabitId hid = await MakeHabit(u);
        var ct = new CompletionTypeData("#333333", "name", null);
        await Assert.ThrowsAsync<NoSuchHabitException>(async () => await MakeService().AddCompletionType(hid with { Id = 131535153 }, ct));
        var ctid = await MakeService().AddCompletionType(hid, ct);
        await Assert.ThrowsAsync<NoSuchHabitException>(async () => await MakeService().RemoveCompletionType(ctid with { Habit = ctid.Habit with { Id = 131333 } }));
        await Assert.ThrowsAsync<NoSuchCompletionTypeException>(async () => await MakeService().RemoveCompletionType(ctid with { Id = 1351531 }));
    }

    private static DateTime d = DateTime.Now.TrimToMinute();
    private CompletionData baseCompletionA = new(null, d, true, null, null);
    private CompletionData baseCompletionB = new(null, d.AddHours(-1), false, "meow", "#333333");


    [Fact]
    public async Task CompletionAddingWithoutCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var id = await MakeService().AddCompletion(h, baseCompletionA);
        var rets = await MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        var ret = rets.First();
        Assert.Equal(id.Id, ret.Id);
        Assert.Equal(baseCompletionA.Note, ret.Note);
        Assert.Equal(baseCompletionA.Color, ret.Color);
        Assert.Equal(baseCompletionA.CompletionTypeId, ret.CompletionTypeId);
        Assert.Equal(baseCompletionA.CompletionDate, ret.CompletionDate);
        Assert.Equal(baseCompletionA.IsExactTime, ret.IsExactTime);
    }

    [Fact]
    public async Task CompletionAddingNegativeWrongHabitTest()
    {
        var u = fixture.MakeUser();
        await Assert.ThrowsAsync<NoSuchHabitException>(async () => await MakeService().AddCompletion(new(1, u), baseCompletionA));
    }

    [Fact]
    public async Task CompletionAddingNegativeWrongCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        await Assert.ThrowsAsync<NoSuchCompletionTypeException>(async () => await MakeService().AddCompletion(h, baseCompletionA with { CompletionTypeId = 13 }));
        Assert.Empty(await MakeService().GetCompletions(h, null, null));
    }

    [Fact]
    public async Task CompletionAddingWithCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var ct = await MakeCT(h);
        var id = await MakeService().AddCompletion(h, baseCompletionB with { CompletionTypeId = ct.Id });
        var rets = await MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        var ret = rets.First();
        Assert.Equal(id.Id, ret.Id);
        Assert.Equal(baseCompletionB.Note, ret.Note);
        Assert.Equal(baseCompletionB.Color, ret.Color);
        Assert.Equal(ct.Id, ret.CompletionTypeId);
        Assert.Equal(baseCompletionB.CompletionDate, ret.CompletionDate);
    }

    [Fact]
    public async Task CompletionUpdateTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var ct = await MakeCT(h);
        var updated = baseCompletionB with { Color = null, Note = "meow", CompletionDate = d.AddMinutes(22), CompletionTypeId = ct.Id };
        var id = await MakeService().AddCompletion(h, baseCompletionA);
        await MakeService().UpdateCompletion(id, updated);
        var rets = await MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        var ret = rets.First();
        Assert.Equal(id.Id, ret.Id);
        Assert.Equal(updated.Note, ret.Note);
        Assert.Equal(updated.Color, ret.Color);
        Assert.Equal(updated.CompletionTypeId, ret.CompletionTypeId);
        Assert.Equal(updated.CompletionDate, ret.CompletionDate);
        Assert.Equal(updated.IsExactTime, ret.IsExactTime);
    }
    [Fact]
    public async Task CompletionRemovalTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var id1 = await MakeService().AddCompletion(h, baseCompletionA);
        var id2 = await MakeService().AddCompletion(h, baseCompletionB);
        Assert.Equal(2, (await MakeService().GetCompletions(h, null, null)).Count());
        await MakeService().RemoveCompletion(id1);
        var rets = await MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        Assert.Equal(id2.Id, rets.First().Id);
        await MakeService().RemoveCompletion(id2);
        Assert.Empty(await MakeService().GetCompletions(h, null, null));
    }

    [Fact]
    public async Task NonexistantCompletionRemovalTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        await Assert.ThrowsAsync<NoSuchCompletionException>(async () => await MakeService().RemoveCompletion(new(0, h)));
    }

    [Fact]
    public async Task NonexistantCompletionUpdateTest()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        await Assert.ThrowsAsync<NoSuchCompletionException>(async () => await MakeService().UpdateCompletion(new(0, h), baseCompletionA));
    }

    [Fact]
    public async Task CompletionDateBasedSelectionTest()
    {
        DateTime t(double x) => d.AddHours(x).TrimToMinute();
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var id1 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(1) });
        var id2 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(2) });
        var id3 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(3) });
        var id4 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(4) });
        Assert.Equal(4, (await MakeService().GetCompletions(h, null, null)).Count());
        Assert.Empty(await MakeService().GetCompletions(h, null, t(5)));
        Assert.Empty(await MakeService().GetCompletions(h, t(-5), null));
        Assert.Equal(4, (await MakeService().GetCompletions(h, t(5), t(-5))).Count());
        {
            var r = await MakeService().GetCompletions(h, t(1.5), t(-5));
            Assert.Single(r);
            Assert.Equal(id1.Id, r.First().Id);
        }
        {
            var r = await MakeService().GetCompletions(h, t(2.5), t(1.5));
            Assert.Single(r);
            Assert.Equal(id2.Id, r.First().Id);
        }
        {
            var r = await MakeService().GetCompletions(h, t(4), t(3));
            Assert.Single(r);
            Assert.Equal(id3.Id, r.First().Id);
        }
    }
    [Fact]
    public async Task CompletinLimitSelectionTest()
    {
        DateTime t(double x) => d.AddHours(x).TrimToMinute();
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var id1 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(1) });
        var id2 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(2) });
        var id3 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(3) });
        var id4 = await MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(4) });
        Assert.Equal(4, (await MakeService().GetCompletions(h, null, null, null)).Count());
        Assert.Equal(4, (await MakeService().GetCompletions(h, null, null, 6)).Count());
        Assert.Equal(4, (await MakeService().GetCompletions(h, null, null, 4)).Count());
        Assert.Single(await MakeService().GetCompletions(h, null, null, 1));
        {
            var r = await MakeService().GetCompletions(h, t(2.5), null, 1);
            Assert.Single(r);
            Assert.Equal(id2.Id, r.First().Id);
        }
        {
            var r = await MakeService().GetCompletions(h, t(3.5), null, 2);
            Assert.NotNull(r);
            Assert.Equal(2, r.Count());
            Assert.Equal(id3.Id, r.First().Id);
            Assert.Equal(id2.Id, r.Last().Id);
        }
    }

    [Fact]
    public async Task CompletionTypeRemovalWithNoCompletion()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var ct = await MakeCT(h);
        await MakeService().RemoveCompletionType(ct);
        Assert.Empty(await MakeService().GetCompletionTypes(h));
    }

    [Fact]
    public async Task CompletionTypeRemovalAttemptWithExistingCompletion()
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var ct = await MakeCT(h);
        await MakeService().AddCompletion(h, baseCompletionA with { CompletionTypeId = ct.Id });
        await Assert.ThrowsAsync<UnableToDeleteCompletionTypeWithExistingCompletions>(async () => await MakeService().RemoveCompletionType(ct));
    }

    private async Task<CompletionDataId?> CompletionAfterDeletingCompletionType(CompletionData completion, CompletionTypeRemovalStrategy strategy)
    {
        var u = fixture.MakeUser();
        var h = await MakeHabit(u);
        var ct = await MakeCT(h);
        await MakeService().AddCompletion(h, completion with { CompletionTypeId = ct.Id });
        await MakeService().RemoveCompletionType(ct, strategy);
        var res = await MakeService().GetCompletions(h, null, null);
        if (res is [CompletionDataId a])
        {
            return a;
        }
        return null;
    }

    [Fact]
    public async Task CompletionTypeRemovalDeleteCompletions()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA, new(null, ColorReplacementStrategy.NeverReplace, true));
        Assert.Null(res);
    }

    [Fact]
    public async Task CompletionTypeRemovalAddNoteCompletionsEmpty()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Note = null }, new("note", ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("note", res.Note);
    }

    [Fact]
    public async Task CompletionTypeRemovalAddNoteCompletionsExistingNote()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Note = "existing" }, new("note", ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("existing\n\nnote", res.Note);
    }

    [Fact]
    public async Task CompletionTypeRemovalWithNoOptionsDoesNotChangeNoteNorColorWhenSet()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Note = "existing", Color = "#222222" }, new(null, ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("existing", res.Note);
        Assert.Equal("#222222", res.Color);
    }

    [Fact]
    public async Task CompletionTypeRemovalWithNoOptionsDoesNotChangeNoteNorColorWhenUnSet()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Note = null, Color = null }, new(null, ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Null(res.Note);
        Assert.Null(res.Color);
    }

    [Fact]
    public async Task CompletionTypeRemovalSetsColorProperlyWhenCompletionHasNoColorStrategyAlways()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Color = null }, new(null, ColorReplacementStrategy.AlwaysReplace, false));
        Assert.NotNull(res);
        Assert.NotNull(res.Color);
    }

    [Fact]
    public async Task CompletionTypeRemovalSetsColorProperlyWhenCompletionHasNoColorStrategyConditional()
    {
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Color = null }, new(null, ColorReplacementStrategy.ReplaceOnlyIfNotSet, false));
        Assert.NotNull(res);
        Assert.NotNull(res.Color);
    }

    [Fact]
    public async Task CompletionTypeRemovalDoesnNotSetColorWhenCompletionHasColorStrategyConditional()
    {
        var oldColor = "#546789";
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Color = oldColor }, new(null, ColorReplacementStrategy.ReplaceOnlyIfNotSet, false));
        Assert.NotNull(res);
        Assert.Equal(oldColor, res.Color);
    }

    [Fact]
    public async Task CompletionTypeRemovalSetColorWhenCompletionHasColorStrategyAlways()
    {
        var oldColor = "#546789";
        var res = await CompletionAfterDeletingCompletionType(baseCompletionA with { Color = oldColor }, new(null, ColorReplacementStrategy.AlwaysReplace, false));
        Assert.NotNull(res);
        Assert.NotEqual(oldColor, res.Color);
    }
}
