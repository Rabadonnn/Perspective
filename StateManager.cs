using Godot;

public class StateManager : Node
{
    public enum States
    {
        Menu,
        Game
    }

    [Export]
    public States CurrenState { get; set; } = States.Menu;

    public World world;

    public override void _Ready()
    {
        world = GetParent<World>();
    }

    public void SetGameState()
    {
    }
}
