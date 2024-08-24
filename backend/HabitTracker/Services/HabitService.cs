using HabitTracker.DTOs;
using HabitTracker.Exceptions;
using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;
namespace HabitTracker.Services;

public interface IHabitService
{
    Task<HabitId> addHabit(HabitNameDescription habit, UserId user);
    Task UpdateHabit(HabitId id, HabitNameDescription replacement);
    Task RemoveHabit(HabitId id);
    Task<HabitNameDescriptionId> getHabitDetails(HabitId id);
    Task<List<HabitNameId>> getHabits(UserId user);

    Task<CompletionTypeId> AddCompletionType(HabitId habit, CompletionTypeData type);
    Task RemoveCompletionType(CompletionTypeId id);
    Task RemoveCompletionType(CompletionTypeId id, CompletionTypeRemovalStrategy options);
    Task UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement);
    Task<List<CompletionTypeDataId>> GetCompletionTypes(HabitId id);

    Task<CompletionId> AddCompletion(HabitId habit, CompletionData completion);
    Task RemoveCompletion(CompletionId id);
    Task UpdateCompletion(CompletionId id, CompletionData replacement);
    Task<List<CompletionDataId>> GetCompletions(HabitId id, DateTime? before, DateTime? after, int? limit = null);
};


public class HabitService(HabitTrackerContext context) : IHabitService
{
    async Task<HabitId> IHabitService.addHabit(HabitNameDescription habit, UserId user)
    {
        Habit h = new() { UserId = user.Id, Name = habit.Name, Description = habit.Description };
        context.Habits.Add(h);
        await context.SaveChangesAsync();
        return new(h.Id, user);
    }

    private async Task<Habit> FindHabit(HabitId habit)
    {
        try
        {
            return await context.Habits.SingleAsync(h => h.Id == habit.Id && h.UserId == habit.User.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchHabitException();
        }
    }
    async Task<HabitNameDescriptionId> IHabitService.getHabitDetails(HabitId habit)
    {
        var h = await FindHabit(habit);
        return new(h.Name, h.Id, h.Description);
    }

    async Task<List<HabitNameId>> IHabitService.getHabits(UserId user)
    {
        var u = await context.Users.Include(u => u.Habits).SingleAsync(u => u.Id == user.Id);
        return u.Habits.AsEnumerable().Select(h => new HabitNameId(h.Name, h.Id)).ToList();
    }

    async Task IHabitService.RemoveHabit(HabitId id)
    {
        var h = await FindHabit(id);
        context.Habits.Remove(h);
        await context.SaveChangesAsync();
    }

    async Task IHabitService.UpdateHabit(HabitId id, HabitNameDescription replacement)
    {
        var h = await FindHabit(id);
        h.Name = replacement.Name;
        h.Description = replacement.Description;
        await context.SaveChangesAsync();
    }

    private async Task<CompletionType> FindCompletionType(CompletionTypeId id)
    {
        var h = await FindHabit(id.Habit);
        try
        {
            return await context.Entry(h).Collection(h => h.CompletionTypes).Query().SingleAsync(c => c.Id == id.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchCompletionTypeException();
        }
    }

    private async Task<Completion> FindCompletion(CompletionId id)
    {
        var h = await FindHabit(id.Habit);
        try
        {
            return await context.Entry(h).Collection(h => h.Completions).Query().SingleAsync(c => c.Id == id.Id);
        }
        catch (InvalidOperationException)
        {
            throw new NoSuchCompletionException();
        }
    }

    private async Task<CompletionType?> FindCompletionCompletionType(HabitId habit, CompletionData completion)
    {
        return completion.CompletionTypeId switch
        {
            int ctid => await FindCompletionType(new(ctid, habit)),
            null => null,
        };
    }

    async Task<CompletionTypeId> IHabitService.AddCompletionType(HabitId habit, CompletionTypeData ctype)
    {
        var h = await FindHabit(habit);
        var t = new CompletionType() { Habit = h, Name = ctype.Name, Description = ctype.Description, Color = ctype.Color };
        context.Add(t);
        await context.SaveChangesAsync();
        return new(t.Id, habit);
    }
    async Task IHabitService.RemoveCompletionType(CompletionTypeId id)
    {
        try
        {
            var c = await FindCompletionType(id);
            context.Remove(c);
            await context.SaveChangesAsync();
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException)
        {
            throw new UnableToDeleteCompletionTypeWithExistingCompletions();
        }
    }

    async Task IHabitService.RemoveCompletionType(CompletionTypeId id, CompletionTypeRemovalStrategy options)
    {
        using var t = await context.Database.BeginTransactionAsync();
        var ct = await FindCompletionType(id);
        var selected = context.Completions.Where(c => c.Type == ct);
        if (options.DeleteCompletions)
        {
            await selected.ExecuteDeleteAsync();
        }
        else
        {
            if (options.Note is string note)
            {
                await selected.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Note, c => c.Note == null ? note : c.Note + "\n\n" + note));
            }
            switch (options.ColorStrategy)
            {
                case ColorReplacementStrategy.AlwaysReplace:
                    await selected.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Color, ct.Color));
                    break;
                case ColorReplacementStrategy.ReplaceOnlyIfNotSet:
                    await selected.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Color, c => c.Color == null ? ct.Color : c.Color));
                    break;
                case ColorReplacementStrategy.NeverReplace:
                    break;
            };
            await selected.ExecuteUpdateAsync(setters => setters.SetProperty(c => c.TypeId, c => null));
        }
        context.Remove(ct);
        await context.SaveChangesAsync();
        await t.CommitAsync();
    }

    async Task IHabitService.UpdateCompletionType(CompletionTypeId id, CompletionTypeData replacement)
    {
        var c = await FindCompletionType(id);
        c.Name = replacement.Name;
        c.Color = replacement.Color;
        c.Description = replacement.Description;
        await context.SaveChangesAsync();
    }
    async Task<List<CompletionTypeDataId>> IHabitService.GetCompletionTypes(HabitId id)
    {
        var h = await FindHabit(id);
        await context.Entry(h).Collection(h => h.CompletionTypes).LoadAsync();
        return h.CompletionTypes.Select(ct => new CompletionTypeDataId(ct.Color, ct.Name, ct.Description, ct.Id)).ToList();
    }

    async Task<CompletionId> IHabitService.AddCompletion(HabitId habit, CompletionData completion)
    {
        var h = await FindHabit(habit);
        var ct = await FindCompletionCompletionType(habit, completion);
        var c = new Completion() { Habit = h, Note = completion.Note, Type = ct, Color = completion.Color, CompletionDate = completion.CompletionDate, IsExactTime = completion.IsExactTime };
        context.Add(c);
        await context.SaveChangesAsync();
        return new(c.Id, habit);
    }

    async Task IHabitService.RemoveCompletion(CompletionId id)
    {
        var c = await FindCompletion(id);
        context.Remove(c);
        await context.SaveChangesAsync();
    }

    async Task IHabitService.UpdateCompletion(CompletionId id, CompletionData replacement)
    {
        var h = await FindHabit(id.Habit);
        var c = await FindCompletion(id);
        c.Type = await FindCompletionCompletionType(id.Habit, replacement);
        c.Note = replacement.Note;
        c.Color = replacement.Color;
        c.CompletionDate = replacement.CompletionDate;
        c.IsExactTime = replacement.IsExactTime;
        await context.SaveChangesAsync();
    }

    /// before is exclusive, after inclusive
    async Task<List<CompletionDataId>> IHabitService.GetCompletions(HabitId id, DateTime? before, DateTime? after, int? limit)
    {
        var h = await FindHabit(id);
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
        return await q.Select(c => new CompletionDataId(c.Id, c.Type != null ? c.Type.Id : null, c.CompletionDate, c.IsExactTime, c.Note, c.Color)).ToListAsync();
    }
}
