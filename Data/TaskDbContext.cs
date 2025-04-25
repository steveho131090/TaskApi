using Microsoft.EntityFrameworkCore;
using TaskApi.Models;

namespace TaskApi.Data;

public class TaskDbContext : Microsoft.EntityFrameworkCore.DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options)
    {}

    public DbSet<Item> Tasks { get; set; }
}