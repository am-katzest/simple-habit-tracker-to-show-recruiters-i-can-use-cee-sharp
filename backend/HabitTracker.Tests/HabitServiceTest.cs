using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Services;

namespace HabitTracker.Tests;

public class HabitServiceTest(UserFixture fixture) : IClassFixture<UserFixture>
{
    private IHabitService MakeService() => new HabitService(fixture.MakeContext());
    private HabitId MakeHabit(UserId u) => MakeService().addHabit(new("name", null), u);

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
}
