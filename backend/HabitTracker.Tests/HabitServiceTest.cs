using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Services;

namespace HabitTracker.Tests;

public class HabitServiceTest(UserFixture fixture) : IClassFixture<UserFixture>
{
    private IHabitService MakeService() => new HabitService(fixture.MakeContext());
    private HabitId MakeHabit(UserId u) => MakeService().addHabit(new("name", null), u);
    private CompletionTypeId MakeCT(HabitId h) => MakeService().AddCompletionType(h, new("#333333", "meow", "meow"));

    [Fact]
    public void CreatingHabit()
    {
        var u = fixture.MakeUser();
        Assert.Empty(MakeService().getHabits(u));
        var habit = new HabitNameDescription("habit", null);
        var hid = MakeService().addHabit(habit, u);
        Assert.Single(MakeService().getHabits(u));
        Assert.Equal("habit", MakeService().getHabits(u).First()?.Name);
        Assert.Equal(hid.Id, MakeService().getHabits(u).First()?.Id);
    }
    [Fact]
    public void GettingHabitPositive()
    {
        var u = fixture.MakeUser();
        Assert.Empty(MakeService().getHabits(u));
        {
            var habit = new HabitNameDescription("habit", null);
            var hid = MakeService().addHabit(habit, u);
            var received = MakeService().getHabitDetails(hid);
            Assert.NotNull(received);
            Assert.Equal(hid.Id, received.Id);
            Assert.Equal("habit", received.Name);
            Assert.Null(received.Description);
        }
        Assert.Single(MakeService().getHabits(u));
        {
            var habit = new HabitNameDescription("habit2", "desc");
            var hid = MakeService().addHabit(habit, u);
            var received = MakeService().getHabitDetails(hid);
            Assert.NotNull(received);
            Assert.Equal(hid.Id, received.Id);
            Assert.Equal("habit2", received.Name);
            Assert.Equal("desc", received.Description);
        }
        Assert.Equal(2, MakeService().getHabits(u).Count);
    }

    [Fact]
    public void GettingHabitNotOwn()
    {
        var u1 = fixture.MakeUser();
        var u2 = fixture.MakeUser();
        Assert.NotEqual(u1, u2);
        var habit = new HabitNameDescription("habit", null);
        var hid = MakeService().addHabit(habit, u1);
        var counterfeit = new HabitId(hid.Id, u2);
        Assert.Throws<NoSuchHabitException>(() => MakeService().getHabitDetails(counterfeit));
    }
    [Fact]
    public void DeletingHabit()
    {
        var u = fixture.MakeUser();
        var habit = new HabitNameDescription("habit", null);
        var hid = MakeService().addHabit(habit, u);
        var hid2 = MakeService().addHabit(new("habit2", null), u);
        Assert.Equal(2, MakeService().getHabits(u).Count);
        MakeService().RemoveHabit(hid);
        Assert.Single(MakeService().getHabits(u));
        MakeService().RemoveHabit(hid2);
        Assert.Empty(MakeService().getHabits(u));
    }
    [Fact]
    public void UpdatingHabit()
    {
        var u = fixture.MakeUser();
        var hid = MakeHabit(u);
        MakeService().UpdateHabit(hid, new("other", "meow"));
        Assert.Single(MakeService().getHabits(u));
        Assert.Equal("other", MakeService().getHabitDetails(hid).Name);
        Assert.Equal("meow", MakeService().getHabitDetails(hid).Description);
        MakeService().UpdateHabit(hid, new("other", null));
        Assert.Null(MakeService().getHabitDetails(hid).Description);
    }
    [Fact]
    public void CompletionTypeExceptionTest()
    {
        var u = fixture.MakeUser();
        HabitId hid = MakeHabit(u);
        var ct = new CompletionTypeData("#333333", "name", null);
        Assert.Throws<NoSuchHabitException>(() => MakeService().AddCompletionType(hid with { Id = 131535153 }, ct));
        var ctid = MakeService().AddCompletionType(hid, ct);
        Assert.Throws<NoSuchHabitException>(() => MakeService().RemoveCompletionType(ctid with { Habit = ctid.Habit with { Id = 131333 } }));
        Assert.Throws<NoSuchCompletionTypeException>(() => MakeService().RemoveCompletionType(ctid with { Id = 1351531 }));
    }

    private static DateTime d = DateTime.Now.TrimToMinute();
    private CompletionData baseCompletionA = new(null, d, true, null, null);
    private CompletionData baseCompletionB = new(null, d.AddHours(-1), false, "meow", "#333333");


    [Fact]
    public void CompletionAddingWithoutCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var id = MakeService().AddCompletion(h, baseCompletionA);
        var rets = MakeService().GetCompletions(h, null, null);
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
    public void CompletionAddingNegativeWrongHabitTest()
    {
        var u = fixture.MakeUser();
        Assert.Throws<NoSuchHabitException>(() => MakeService().AddCompletion(new(1, u), baseCompletionA));
    }

    [Fact]
    public void CompletionAddingNegativeWrongCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        Assert.Throws<NoSuchCompletionTypeException>(() => MakeService().AddCompletion(h, baseCompletionA with { CompletionTypeId = 13 }));
        Assert.Empty(MakeService().GetCompletions(h, null, null));
    }

    [Fact]
    public void CompletionAddingWithCompletionTypeTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var ct = MakeCT(h);
        var id = MakeService().AddCompletion(h, baseCompletionB with { CompletionTypeId = ct.Id });
        var rets = MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        var ret = rets.First();
        Assert.Equal(id.Id, ret.Id);
        Assert.Equal(baseCompletionB.Note, ret.Note);
        Assert.Equal(baseCompletionB.Color, ret.Color);
        Assert.Equal(ct.Id, ret.CompletionTypeId);
        Assert.Equal(baseCompletionB.CompletionDate, ret.CompletionDate);
    }

    [Fact]
    public void CompletionUpdateTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var ct = MakeCT(h);
        var updated = baseCompletionB with { Color = null, Note = "meow", CompletionDate = d.AddMinutes(22), CompletionTypeId = ct.Id };
        var id = MakeService().AddCompletion(h, baseCompletionA);
        MakeService().UpdateCompletion(id, updated);
        var rets = MakeService().GetCompletions(h, null, null);
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
    public void CompletionRemovalTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var id1 = MakeService().AddCompletion(h, baseCompletionA);
        var id2 = MakeService().AddCompletion(h, baseCompletionB);
        Assert.Equal(2, MakeService().GetCompletions(h, null, null).Count());
        MakeService().RemoveCompletion(id1);
        var rets = MakeService().GetCompletions(h, null, null);
        Assert.Single(rets);
        Assert.Equal(id2.Id, rets.First().Id);
        MakeService().RemoveCompletion(id2);
        Assert.Empty(MakeService().GetCompletions(h, null, null));
    }

    [Fact]
    public void NonexistantCompletionRemovalTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        Assert.Throws<NoSuchCompletionException>(() => MakeService().RemoveCompletion(new(0, h)));
    }

    [Fact]
    public void NonexistantCompletionUpdateTest()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        Assert.Throws<NoSuchCompletionException>(() => MakeService().UpdateCompletion(new(0, h), baseCompletionA));
    }

    [Fact]
    public void CompletionDateBasedSelectionTest()
    {
        DateTime t(double x) => d.AddHours(x).TrimToMinute();
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var id1 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(1) });
        var id2 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(2) });
        var id3 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(3) });
        var id4 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(4) });
        Assert.Equal(4, MakeService().GetCompletions(h, null, null).Count());
        Assert.Empty(MakeService().GetCompletions(h, null, t(5)));
        Assert.Empty(MakeService().GetCompletions(h, t(-5), null));
        Assert.Equal(4, MakeService().GetCompletions(h, t(5), t(-5)).Count());
        {
            var r = MakeService().GetCompletions(h, t(1.5), t(-5));
            Assert.Single(r);
            Assert.Equal(id1.Id, r.First().Id);
        }
        {
            var r = MakeService().GetCompletions(h, t(2.5), t(1.5));
            Assert.Single(r);
            Assert.Equal(id2.Id, r.First().Id);
        }
        {
            var r = MakeService().GetCompletions(h, t(4), t(3));
            Assert.Single(r);
            Assert.Equal(id3.Id, r.First().Id);
        }
    }
    [Fact]
    public void CompletinLimitSelectionTest()
    {
        DateTime t(double x) => d.AddHours(x).TrimToMinute();
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var id1 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(1) });
        var id2 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(2) });
        var id3 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(3) });
        var id4 = MakeService().AddCompletion(h, baseCompletionA with { CompletionDate = t(4) });
        Assert.Equal(4, MakeService().GetCompletions(h, null, null, null).Count());
        Assert.Equal(4, MakeService().GetCompletions(h, null, null, 6).Count());
        Assert.Equal(4, MakeService().GetCompletions(h, null, null, 4).Count());
        Assert.Single(MakeService().GetCompletions(h, null, null, 1));
        {
            var r = MakeService().GetCompletions(h, t(2.5), null, 1);
            Assert.Single(r);
            Assert.Equal(id2.Id, r.First().Id);
        }
        {
            var r = MakeService().GetCompletions(h, t(3.5), null, 2);
            Assert.NotNull(r);
            Assert.Equal(2, r.Count());
            Assert.Equal(id3.Id, r.First().Id);
            Assert.Equal(id2.Id, r.Last().Id);
        }
    }

    [Fact]
    public void CompletionTypeRemovalWithNoCompletion()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var ct = MakeCT(h);
        MakeService().RemoveCompletionType(ct);
        Assert.Empty(MakeService().GetCompletionTypes(h));
    }

    [Fact]
    public void CompletionTypeRemovalAttemptWithExistingCompletion()
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var ct = MakeCT(h);
        MakeService().AddCompletion(h, baseCompletionA with { CompletionTypeId = ct.Id });
        Assert.Throws<UnableToDeleteCompletionTypeWithExistingCompletions>(() => MakeService().RemoveCompletionType(ct));
    }

    private CompletionDataId? CompletionAfterDeletingCompletionType(CompletionData completion, CompletionTypeRemovalStrategy strategy)
    {
        var u = fixture.MakeUser();
        var h = MakeHabit(u);
        var ct = MakeCT(h);
        MakeService().AddCompletion(h, completion with { CompletionTypeId = ct.Id });
        MakeService().RemoveCompletionType(ct, strategy);
        var res = MakeService().GetCompletions(h, null, null);
        if (res is [CompletionDataId a])
        {
            return a;
        }
        return null;
    }

    [Fact]
    public void CompletionTypeRemovalDeleteCompletions()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA, new(null, ColorReplacementStrategy.NeverReplace, true));
        Assert.Null(res);
    }

    [Fact]
    public void CompletionTypeRemovalAddNoteCompletionsEmpty()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Note = null }, new("note", ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("note", res.Note);
    }

    [Fact]
    public void CompletionTypeRemovalAddNoteCompletionsExistingNote()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Note = "existing" }, new("note", ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("existing\n\nnote", res.Note);
    }

    [Fact]
    public void CompletionTypeRemovalWithNoOptionsDoesNotChangeNoteNorColorWhenSet()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Note = "existing", Color = "#222222" }, new(null, ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Equal("existing", res.Note);
        Assert.Equal("#222222", res.Color);
    }

    [Fact]
    public void CompletionTypeRemovalWithNoOptionsDoesNotChangeNoteNorColorWhenUnSet()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Note = null, Color = null }, new(null, ColorReplacementStrategy.NeverReplace, false));
        Assert.NotNull(res);
        Assert.Null(res.Note);
        Assert.Null(res.Color);
    }

    [Fact]
    public void CompletionTypeRemovalSetsColorProperlyWhenCompletionHasNoColorStrategyAlways()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Color = null }, new(null, ColorReplacementStrategy.AlwaysReplace, false));
        Assert.NotNull(res);
        Assert.NotNull(res.Color);
    }

    [Fact]
    public void CompletionTypeRemovalSetsColorProperlyWhenCompletionHasNoColorStrategyConditional()
    {
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Color = null }, new(null, ColorReplacementStrategy.ReplaceOnlyIfNotSet, false));
        Assert.NotNull(res);
        Assert.NotNull(res.Color);
    }

    [Fact]
    public void CompletionTypeRemovalDoesnNotSetColorWhenCompletionHasColorStrategyConditional()
    {
        var oldColor = "#546789";
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Color = oldColor }, new(null, ColorReplacementStrategy.ReplaceOnlyIfNotSet, false));
        Assert.NotNull(res);
        Assert.Equal(oldColor, res.Color);
    }

    [Fact]
    public void CompletionTypeRemovalSetColorWhenCompletionHasColorStrategyAlways()
    {
        var oldColor = "#546789";
        var res = CompletionAfterDeletingCompletionType(baseCompletionA with { Color = oldColor }, new(null, ColorReplacementStrategy.AlwaysReplace, false));
        Assert.NotNull(res);
        Assert.NotEqual(oldColor, res.Color);
    }
}
