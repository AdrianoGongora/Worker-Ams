using Worker_Ams.Contracts.Motors;
using Worker_Ams.Database;
using Worker_Ams.Entities;
using Worker_Ams.Repositories.Motors;

namespace Worker_Ams.Endpoints;

public static class MotorEndpoints
{
    public static void MapMotorEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("api/motores", async (
            CreateMotorRquest request,
            IMotorRepository motorRepository,
            CancellationToken cancellationToken) =>
        {

            var motor = new Motor
            {
                Nombre = request.Nombre,
                Descripcion = request.Descripcion,
                Tipo = request.Tipo,
            };

            await motorRepository.SaveAsync(motor, cancellationToken);

            return Results.Ok(motor);
        })
        .RequireAuthorization()
        .WithTags(Tags.Motores);

        app.MapPut("api/motores", async (
            UpdateMotorRequest request,
            IMotorRepository motorRepository,
            ApplicationDbContext dbContext) =>
        {

            var motorExists = await motorRepository.GetById(request.MotorId);

            if (motorExists is null)
            {
                return Results.NotFound("El motor no existe");
            }

            motorExists.Update(
                request.Nombre,
                request.Descripcion,
                request.Tipo
            );

            await dbContext.SaveChangesAsync();

            return Results.Ok();
        })
        .RequireAuthorization()
        .WithTags(Tags.Motores);

        app.MapGet("api/motores", async (
            IMotorRepository motorRepository) =>
        {

            var motores = await motorRepository.GetMotors();

            return Results.Ok(motores);
        }).WithTags(Tags.Motores);

        app.MapGet("motores/{motorId}", async (
            int motorId,
            IMotorRepository motorRepository) =>
        {

            var motorExists = await motorRepository.GetById(motorId);

            if (motorExists is null)
            {
                return Results.NotFound("El motor que esta buscando no existe");
            }

            return Results.Ok(motorExists);
        })
        .RequireAuthorization()
        .WithTags(Tags.Motores);

        app.MapDelete("api/motores/{motorId}", async (
            int motorId,
            IMotorRepository motorRepository,
            CancellationToken cancellationToken) =>
        {

            var motorExists = await motorRepository.GetById(motorId);

            if (motorExists is null)
            {
                return Results.NotFound("El motor que esta buscando no existe");
            }

            await motorRepository.DeleteAsync(motorExists, cancellationToken);

            return Results.Ok(motorExists);
        })
        .RequireAuthorization()
        .WithTags(Tags.Motores);
    }
}
