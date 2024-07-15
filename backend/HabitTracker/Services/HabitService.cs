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
    CompletionTypeId AddCompletionType(HabitId Habit, CompletionTypeData type);
    void RemoveCompletionType(CompletionTypeId id);
    void UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement);
    List<CompletionTypeDataId> GetCompletionTypes(HabitId id);
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

    private CompletionType FindCompletionType(CompletionTypeId id)
    {
        var h = FindHabit(id.Habit);
        try
        {
            return Context.Entry(h).Collection(h => h.CompletionTypes).Query().Single(c => c.Id == id.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchCompletionTypeException();
        }
    }

    CompletionTypeId IHabitService.AddCompletionType(HabitId Habit, CompletionTypeData ctype)
    {
        var h = FindHabit(Habit);
        var t = new CompletionType() { Habit = h, Name = ctype.Name, Description = ctype.Description, Color = ctype.Color };
        Context.Add(t);
        Context.SaveChanges();
        return new(t.Id, Habit);
    }
    void IHabitService.RemoveCompletionType(CompletionTypeId id)
    {
        var c = FindCompletionType(id);
        Context.Remove(c);
        Context.SaveChanges();
    }
    void IHabitService.UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement)
    {
        var c = FindCompletionType(id);
        c.Name = replacement.Name;
        c.Color = replacement.Color;
        c.Description = replacement.Description;
        Context.SaveChanges();
    }
    List<CompletionTypeDataId> IHabitService.GetCompletionTypes(HabitId id)
    {
        var h = FindHabit(id);
        Context.Entry(h).Collection(h => h.CompletionTypes).Load();
        return h.CompletionTypes.Select(ct => new CompletionTypeDataId(ct.Color, ct.Name, ct.Description, ct.Id)).ToList();
    }
}
