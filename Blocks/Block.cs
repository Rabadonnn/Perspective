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

    Vector3 velocity = Vector3.Zero; 

    Vector3 initialPos;

    public override void _Ready()
    {
        tween = GetTree().Root.GetNode<Tween>("World/Tween");

        InitMaterial();

        GetTree().Root.GetNode("World").GetNode<Player>("Player").Respawn += Reset;

        initialPos = GlobalTransform.origin;
    }

    bool active = true;
    void Reset()
    {
        if (Ticking || DisableAfterTouch)
        {
            SetActive(true);
            timer = Ticking_ActiveTime;
            velocity = Vector3.Zero;
            Translation = initialPos;
            var mat = (MaterialOverride as SpatialMaterial);
            mat.AlbedoColor = new Color(mat.AlbedoColor.r, mat.AlbedoColor.r, mat.AlbedoColor.b, 1);
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

        if (DisableAfterTouch && Translation.y > -100)
        {
            Translation += velocity * delta;
        }
    }

    public void SetActive(bool _active, float easeTime = 0.1f)
    {
        GetNode<StaticBody>("StaticBody").GetNode<CollisionShape>("CollisionShape").Disabled = !_active;
        if (_active)
        {
            SetTransparency(1f, Tween.EaseType.In, easeTime);
        }
        else
        {
            SetTransparency(0f, Tween.EaseType.Out, easeTime);
        }
    }

    int fallSpeed = 50;
    public void MakeItFall()
    {
        SetActive(false, 2f);
        velocity = new Vector3(0, -fallSpeed, 0);
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
