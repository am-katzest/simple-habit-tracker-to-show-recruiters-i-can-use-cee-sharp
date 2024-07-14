using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;
namespace HabitTracker.Services;

public interface IHabitService
{
    HabitId addHabit(HabitNameDescription Habit, UserId User);
    void UpdateHabit(HabitId Habit, HabitNameDescription Replacement);
    void RemoveHabit(HabitId Habit);
    HabitNameDescriptionId getHabitDetails(HabitId Habit);
    List<HabitNameId> getHabits(UserId User);
};


public class HabitService(HabitTrackerContext Context) : IHabitService
{
    HabitId IHabitService.addHabit(HabitNameDescription Habit, UserId User)
    {
        Habit h = new() { UserId = User.Id, Name = Habit.Name, Description = Habit.Description };
        Context.Habits.Add(h);
        Context.SaveChanges();
        return new(h.Id, User);
    }

    private Habit FindHabit(HabitId Habit)
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
    HabitNameDescriptionId IHabitService.getHabitDetails(HabitId Habit)
    {
        var h = FindHabit(Habit);
        return new(h.Name, h.Id, h.Description);
    }

    List<HabitNameId> IHabitService.getHabits(UserId User)
    {
        var u = Context.Users.Include(u => u.Habits).Single(u => u.Id == User.Id);
        return u.Habits.AsEnumerable().Select(h => new HabitNameId(h.Name, h.Id)).ToList();
    }

    void IHabitService.RemoveHabit(HabitId Habit)
    {
        var h = FindHabit(Habit);
        Context.Habits.Remove(h);
        Context.SaveChanges();
    }

    void IHabitService.UpdateHabit(HabitId Habit, HabitNameDescription Replacement)
    {
        var h = FindHabit(Habit);
        h.Name = Replacement.Name;
        h.Description = Replacement.Description;
        Context.SaveChanges();
    }
}
