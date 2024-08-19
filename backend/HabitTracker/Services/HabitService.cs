using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;
namespace HabitTracker.Services;

public interface IHabitService
{
    HabitId addHabit(HabitNameDescription habit, UserId user);
    void UpdateHabit(HabitId id, HabitNameDescription replacement);
    void RemoveHabit(HabitId id);
    HabitNameDescriptionId getHabitDetails(HabitId id);
    List<HabitNameId> getHabits(UserId user);

    CompletionTypeId AddCompletionType(HabitId habit, CompletionTypeData type);
    void RemoveCompletionType(CompletionTypeId id);
    void UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement);
    List<CompletionTypeDataId> GetCompletionTypes(HabitId id);

    CompletionId AddCompletion(HabitId habit, CompletionData completion);
    void RemoveCompletion(CompletionId id);
    void UpdateCompletion(CompletionId id, CompletionData replacement);
    List<CompletionDataId> GetCompletions(HabitId id, DateTime? before, DateTime? after, int? limit = null);
};


public class HabitService(HabitTrackerContext context) : IHabitService
{
    HabitId IHabitService.addHabit(HabitNameDescription habit, UserId user)
    {
        Habit h = new() { UserId = user.Id, Name = habit.Name, Description = habit.Description };
        context.Habits.Add(h);
        context.SaveChanges();
        return new(h.Id, user);
    }

    private Habit FindHabit(HabitId habit)
    {
        try
        {
            return context.Habits.Single(h => h.Id == habit.Id && h.UserId == habit.User.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchHabitException();
        }
    }
    HabitNameDescriptionId IHabitService.getHabitDetails(HabitId habit)
    {
        var h = FindHabit(habit);
        return new(h.Name, h.Id, h.Description);
    }

    List<HabitNameId> IHabitService.getHabits(UserId user)
    {
        var u = context.Users.Include(u => u.Habits).Single(u => u.Id == user.Id);
        return u.Habits.AsEnumerable().Select(h => new HabitNameId(h.Name, h.Id)).ToList();
    }

    void IHabitService.RemoveHabit(HabitId id)
    {
        var h = FindHabit(id);
        context.Habits.Remove(h);
        context.SaveChanges();
    }

    void IHabitService.UpdateHabit(HabitId id, HabitNameDescription replacement)
    {
        var h = FindHabit(id);
        h.Name = replacement.Name;
        h.Description = replacement.Description;
        context.SaveChanges();
    }

    private CompletionType FindCompletionType(CompletionTypeId id)
    {
        var h = FindHabit(id.Habit);
        try
        {
            return context.Entry(h).Collection(h => h.CompletionTypes).Query().Single(c => c.Id == id.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchCompletionTypeException();
        }
    }

    private Completion FindCompletion(CompletionId id)
    {
        var h = FindHabit(id.Habit);
        try
        {
            return context.Entry(h).Collection(h => h.Completions).Query().Single(c => c.Id == id.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchCompletionException();
        }
    }

    private CompletionType? FindCompletionCompletionType(HabitId habit, CompletionData completion)
    {
        return completion.CompletionTypeId switch
        {
            int ctid => FindCompletionType(new(ctid, habit)),
            null => null,
        };
    }

    CompletionTypeId IHabitService.AddCompletionType(HabitId habit, CompletionTypeData ctype)
    {
        var h = FindHabit(habit);
        var t = new CompletionType() { Habit = h, Name = ctype.Name, Description = ctype.Description, Color = ctype.Color };
        context.Add(t);
        context.SaveChanges();
        return new(t.Id, habit);
    }
    void IHabitService.RemoveCompletionType(CompletionTypeId id)
    {
        try
        {
            var c = FindCompletionType(id);
            context.Remove(c);
            context.SaveChanges();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new UnableToDeleteCompletionTypeWithExistingCompletions();
        }
        context.SaveChanges();
    }
    void IHabitService.UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement)
    {
        var c = FindCompletionType(id);
        c.Name = replacement.Name;
        c.Color = replacement.Color;
        c.Description = replacement.Description;
        context.SaveChanges();
    }
    List<CompletionTypeDataId> IHabitService.GetCompletionTypes(HabitId id)
    {
        var h = FindHabit(id);
        context.Entry(h).Collection(h => h.CompletionTypes).Load();
        return h.CompletionTypes.Select(ct => new CompletionTypeDataId(ct.Color, ct.Name, ct.Description, ct.Id)).ToList();
    }

    CompletionId IHabitService.AddCompletion(HabitId habit, CompletionData completion)
    {
        var h = FindHabit(habit);
        var ct = FindCompletionCompletionType(habit, completion);
        var c = new Completion() { Habit = h, Note = completion.Note, Type = ct, Color = completion.Color, CompletionDate = completion.CompletionDate, IsExactTime = completion.IsExactTime };
        context.Add(c);
        context.SaveChanges();
        return new(c.Id, habit);
    }

    void IHabitService.RemoveCompletion(CompletionId id)
    {
        var c = FindCompletion(id);
        context.Remove(c);
        context.SaveChanges();
    }

    void IHabitService.UpdateCompletion(CompletionId id, CompletionData replacement)
    {
        var h = FindHabit(id.Habit);
        var c = FindCompletion(id);
        c.Type = FindCompletionCompletionType(id.Habit, replacement);
        c.Note = replacement.Note;
        c.Color = replacement.Color;
        c.CompletionDate = replacement.CompletionDate;
        c.IsExactTime = replacement.IsExactTime;
        context.SaveChanges();
    }

    /// before is exclusive, after inclusive
    List<CompletionDataId> IHabitService.GetCompletions(HabitId id, DateTime? before, DateTime? after, int? limit = null)
    {
        var h = FindHabit(id);
        var q = context.Entry(h).Collection(h => h.Completions).Query();
        if (before is DateTime b)
        {
            q = q.Where(c => c.CompletionDate < b);
        }
        if (after is DateTime a)
        {
            q = q.Where(c => c.CompletionDate >= a);
        }
        q = q.OrderByDescending(i => i.CompletionDate);
        if (limit is int l)
        {
            q = q.Take(l);
        }
        return q.Select(c => new CompletionDataId(c.Id, c.Type != null ? c.Type.Id : null, c.CompletionDate, c.IsExactTime, c.Note, c.Color)).ToList();
    }
}
