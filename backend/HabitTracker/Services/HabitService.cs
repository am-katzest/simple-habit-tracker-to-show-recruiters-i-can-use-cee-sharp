

using HabitTracker.DTOs;
using HabitTracker.DTOs.Habit;
using HabitTracker.DTOs.User;
using HabitTracker.Exceptions;
using HabitTracker.Helpers;
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
public class HabitService(HabitTrackerContext Context, IClock Clock) : IHabitService
{
    IdUser IHabitService.addHabit(HabitNameDescription Habit, IdOnly User)
    {
        Habit h = new() { UserId = User.Id, Name = Habit.Name, Description = Habit.Description };
        Context.Habits.Add(h);
        Context.SaveChanges();
        return new(h.Id, User);
    }

    HabitNameDescriptionId IHabitService.getHabitDetails(IdUser Habit)
    {
        throw new NotImplementedException();
    }

    List<HabitNameId> IHabitService.getHabits(IdOnly User)
    {
        return Context.Habits.Where(h => h.Id == User.Id).Select(h => new HabitNameId(h.Name, h.Id)).ToList();
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
