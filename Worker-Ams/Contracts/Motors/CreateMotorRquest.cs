namespace Worker_Ams.Contracts.Motors;

public sealed record CreateMotorRquest(
    string Nombre,
    string Descripcion,
    string Tipo
);