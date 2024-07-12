using HabitTracker.DTOs.Habit;
using HabitTracker.DTOs.User;
using HabitTracker.Exceptions;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;
namespace HabitTracker.Services;

public interface IHabitService
{
    IdUser addHabit(HabitNameDescription Habit, DTOs.User.IdOnly User);
    void UpdateHabit(IdUser Habit, HabitNameDescription Replacement);
    void RemoveHabit(IdUser Habit);
    HabitNameDescriptionId getHabitDetails(IdUser Habit);
    List<HabitNameId> getHabits(DTOs.User.IdOnly User);
};
public class HabitService(HabitTrackerContext Context, IClock _Clock) : IHabitService
{
    IdUser IHabitService.addHabit(HabitNameDescription Habit, IdOnly User)
    {
        Habit h = new() { UserId = User.Id, Name = Habit.Name, Description = Habit.Description };
        Context.Habits.Add(h);
        Context.SaveChanges();
        return new(h.Id, User);
    }

    private Habit FindHabit(IdUser Habit)
    {
        try
        {
            return Context.Habits.Single(h => h.Id == Habit.Id && h.UserId == Habit.User.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchHabitException();
        }
    }
    HabitNameDescriptionId IHabitService.getHabitDetails(IdUser Habit)
    {
        var h = FindHabit(Habit);
        return new(h.Name, h.Id, h.Description);
    }

    List<HabitNameId> IHabitService.getHabits(IdOnly User)
    {
        var u = Context.Users.Include(u => u.Habits).Single(u => u.Id == User.Id);
        return u.Habits.AsEnumerable().Select(h => new HabitNameId(h.Name, h.Id)).ToList();
    }

    void IHabitService.RemoveHabit(IdUser Habit)
    {
        throw new NotImplementedException();
    }

    void IHabitService.UpdateHabit(IdUser Habit, HabitNameDescription Replacement)
    {
        throw new NotImplementedException();
    }
}
