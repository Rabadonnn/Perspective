using Godot;
using System;

public class LevelSettings : Node
{
    // each level should have a settings node
    [Export]
    public int DeadYCoordinate { get; set; } = -10;
    [Export]
    public Color EnvColor { get; set; } = Colors.RebeccaPurple;
    [Export]
    public Color LightColor { get; set; } = Colors.White;
    [Export]
    public Color AmbientColor { get; set; } = Colors.Black;
    [Export]
    public Color PlayerColor { get; set; } = Colors.White;
    [Export]
    public bool Glow { get; set; } = false;
}
