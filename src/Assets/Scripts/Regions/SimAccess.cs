
/// <summary>
/// Maturity ratings for simulators 
/// </summary>
public enum SimAccess : byte
{
    Min    = 0,     // Treated as 'unknown', usually ends up being PG
    PG     = 13,
    Mature = 21,
    Adult  = 42,    // Seriously Adult Only
    Down   = 254,
    Max    = Adult
}
