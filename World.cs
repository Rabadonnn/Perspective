using Godot;
using System.Diagnostics;

public class World : Spatial
{
    public StateManager stateManager;

    public Node CurrentLevel { get; private set; }

    public Stopwatch LevelTimer { get; private set; }

    [Export]
    public PackedScene DebugLevel { get; set; }

    Player player;

    public override void _Ready()
    {
        player = GetNode<Player>("Player");
        player.SetPhysicsProcess(false);

        ChangeLevel(DebugLevel);
    }

    void ChangeLevel(PackedScene lvl)
    {
        CurrentLevel = lvl.Instance();
        AddChild(CurrentLevel);

        var settings = CurrentLevel.GetNode<LevelSettings>("LevelSettings");
        SetLevelSettings(settings);

        player.InitializeLevel(CurrentLevel as Spatial, settings);

        LevelTimer = new Stopwatch();
    }

    void SetLevelSettings(LevelSettings settings)
    {
        var cam = GetNode<Camera>("Camera");
        cam.Environment.BackgroundColor = settings.EnvColor;
        cam.Environment.FogColor = settings.EnvColor;
        cam.Environment.AmbientLightColor = settings.AmbientColor;
        cam.Environment.GlowEnabled = settings.Glow;
        var light = GetNode<DirectionalLight>("DirectionalLight");
        light.LightColor = settings.LightColor;
    }

    void Restart()
    {
        LevelTimer.Reset();
        LevelTimer.Start();
    }
}
