using Microsoft.EntityFrameworkCore;

namespace AA_App.Data;

public class CollegeDbContext : DbContext
{
    public CollegeDbContext(DbContextOptions<CollegeDbContext> options) : base(options) { }

    public DbSet<GroupRecord> Groups => Set<GroupRecord>();
    public DbSet<StudentRecord> Students => Set<StudentRecord>();
    public DbSet<TeacherRecord> Teachers => Set<TeacherRecord>();
    public DbSet<GradeRecord> Grades => Set<GradeRecord>();
    public DbSet<AttendanceRecord> Attendance => Set<AttendanceRecord>();
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<TaskRun> TaskRuns => Set<TaskRun>();
}
