using Godot;

public class Block : MeshInstance
{
    [Export]
    public bool IsPad { get; set; } = false;

    [Export]
    public bool IsBad { get; set; } = false;

    [Export]
    public bool Ticking { get; set; } = false;
    [Export]
    public float Ticking_ActiveTime { get; set; } = 1f;
    [Export]
    public float Ticking_DisabledTime { get; set; } = 1f;

    [Export]
    public bool DisableAfterTouch { get; set; } = false;

    float timer = 0;

    Tween tween;

    public override void _Ready()
    {
        if (IsBad || IsPad)
        {
            tween = GetParent().GetParent().GetNode<Tween>("Tween");
        }
        else
        {

            tween = GetParent().GetNode<Tween>("Tween");
        }

        if (!IsPad)
        {
            InitMaterial();
        }

        GetTree().Root.GetNode("World").GetNode<Player>("Player").Respawn += Reset;
    }

    bool active = true;
    void Reset()
    {
        if (Ticking || DisableAfterTouch)
        {
            SetActive(true);
            timer = Ticking_ActiveTime;
        }
    }

    public override void _Process(float delta)
    {
        if (Ticking)
        {
            timer -= delta;
            if (timer < 0)
            {
                active = !active;
                SetActive(active);
                timer = active ? Ticking_ActiveTime : Ticking_DisabledTime;
            }
        }
    }

    public void SetActive(bool _active)
    {
        GetNode<StaticBody>("StaticBody").GetNode<CollisionShape>("CollisionShape").Disabled = !_active;
        if (_active)
        {
            SetTransparency(1f, Tween.EaseType.In, 0.1f);
        }
        else
        {
            SetTransparency(0f, Tween.EaseType.Out, 0.1f);
        }
    }

    void InitMaterial()
    {
        MaterialOverride = new SpatialMaterial();
        (MaterialOverride as SpatialMaterial).FlagsTransparent = true;
    }

    public void SetTransparency(float value, Tween.EaseType easeType, float duration)
    {
        var currentColor = (MaterialOverride as SpatialMaterial).AlbedoColor;
        value = Mathf.Clamp(value, 0, 1);
        tween.InterpolateProperty(MaterialOverride, "albedo_color:a", currentColor.a, value, duration, Tween.TransitionType.Linear, easeType, 0);
        tween.Start();
    }
}
