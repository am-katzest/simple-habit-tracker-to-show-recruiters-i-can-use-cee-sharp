using HabitTracker.Controllers;
using HabitTracker.Helpers;
using HabitTracker.Models;
using Microsoft.AspNetCore.Mvc;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.RegisterServicesProduction();
var ctx = builder.Services.BuildServiceProvider().GetRequiredService<HabitTrackerContext>();
ctx.Database.EnsureCreated(); // temporary

var app = builder.Build();
app.UseRouting();
app.UseHttpsRedirection();
app.UseEndpoints(e => e.MapControllers());
app.Run();
