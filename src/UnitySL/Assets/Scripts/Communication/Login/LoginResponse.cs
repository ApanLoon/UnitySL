using System;

public class LoginResponse
{
    public bool LoginSucceeded { get; set; }
    public string LoginFailReason { get; set; }
    public string Message { get; set; }
    public string MessageId { get; set; }
    
    public Guid SessionId { get; set; }

    #region Agent
    public Guid AgentId { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string DisplayName { get; set; }
    #endregion Agent

    #region Region
    public RegionHandle RegionHandle { get; set; }
    public string SimIp { get; set; }
    public int SimPort { get; set; }
    public UInt32 CircuitCode { get; set; }
    public string SeedCapability { get; set; }
    #endregion Region
}
