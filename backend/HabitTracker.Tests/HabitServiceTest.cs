using HabitTracker.DTOs.Habit;
using HabitTracker.Services;

namespace HabitTracker.Tests;

public class HabitServiceTest(UserFixture Fixture) : IClassFixture<UserFixture>
{
    private IHabitService MakeService() => MakeService(new RealClock());
    private IHabitService MakeService(IClock clock) => new HabitService(Fixture.MakeContext(), clock);

    [Fact]
    public void CreatingHabit()
    {
        var u = Fixture.MakeUser();
        Assert.Empty(MakeService().getHabits(u));
        var habit = new HabitNameDescription("habit", null);
        var hid = MakeService().addHabit(habit, u);
        Assert.Single(MakeService().getHabits(u));
        Assert.Equal("habit", MakeService().getHabits(u).First()?.Name);
        Assert.Equal(hid.Id, MakeService().getHabits(u).First()?.Id);
    }
}
