using Exiled.API.Interfaces;

public class Config : IConfig
{
    public bool IsEnabled { get; set; } = true;
    public string[] Prefixes { get; set; } = { "!" };
    public bool Debug { get; set; } = true;

    public string FemurBreakerActivatedHint { get; set; } = "The Femur Breaker has already been activated.";
    public string SCP106NotAliveHint { get; set; } = "SCP-106 is not out of containment.";
    public string AudioFilePath { get; set; } = @"C:\Users\Administrator\AppData\Roaming\EXILED\Configs\FemurBreakerSound.ogg";
    public string FakePlayerName { get; set; } = "Femur Breaker";
    public string GameObjectName { get; set; } = "FemurBrakerButton";
}
