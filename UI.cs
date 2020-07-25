using Godot;
using System;

public class UI : Control
{
    Label timerLabel;
    World world;

    public override void _Ready()
    {
        world = GetTree().Root.GetNode<World>("World");
        timerLabel = GetNode<Label>("TimerLabel");
    }

    public override void _Process(float delta)
    {
        var ts = world.LevelTimer.Elapsed;
        var timeText = String.Format("{0:00}:{1:00}:{2:00}", ts.Minutes, ts.Seconds, ts.Milliseconds.ToString().Substr(0, 2));
        timerLabel.Text = timeText;
    }
}
