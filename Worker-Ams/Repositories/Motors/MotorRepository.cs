using Microsoft.EntityFrameworkCore;
using Worker_Ams.Database;
using Worker_Ams.Entities;

namespace Worker_Ams.Repositories.Motors;

public class MotorRepository(ApplicationDbContext dbContext) : IMotorRepository
{
    public async Task DeleteAsync(Motor motor, CancellationToken cancellationToken)
    {
        dbContext.Motors.Remove(motor);
        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Motor?> GetById(int motorId)
    {
        return await dbContext.Motors.SingleOrDefaultAsync(a => a.Id == motorId);
    }

    public async Task<List<Motor>> GetMotors()
    {
        return await dbContext.Motors.ToListAsync();
    }

    public async Task SaveAsync(Motor motor, CancellationToken cancellationToken)
    {
        dbContext.Add(motor);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
