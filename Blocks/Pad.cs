using Godot;
using System;

public class Pad : Spatial
{
    [Export]
    public Vector3 Gravity { get; set; } = new Vector3(0, -10, 0); 
    [Export]
    public float SpeedScale { get; set; } = 0.4f;
    [Export]
    public float Lifetime { get; set; } = 1f;

    public override void _Ready()
    {
        var particles = GetNode<CPUParticles>("CPUParticles");
        particles.Gravity = Gravity;
        particles.SpeedScale = SpeedScale;
        particles.Lifetime = Lifetime;
    }
}
