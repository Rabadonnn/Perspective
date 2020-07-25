using Godot;

public class Player : KinematicBody
{
    [Export]
    float gravity = 2.3f;
    [Export]
    float jumpForce = 45;
    [Export]
    int horizontalAcceleration = 5;
    [Export]
    int maxHorizontalSpeed = 18;
    float xFriction = 0.5f;

    public delegate void CameraCalibrateEvent();
    public delegate void RespawnEvent();

    public event RespawnEvent Respawn;

    public event CameraCalibrateEvent CalibrateCamera;
    public event CameraCalibrateEvent CalibrateCameraY;

    Vector3 velocity = new Vector3();

    public int RotationDir { get; set; } = 1;

    public int DeadYCoordinate { get; private set; } = -10;

    Spatial level;

    public Vector3 LowestPoint { get; private set; } = Vector3.Zero;

    public override void _Ready()
    {
        level = (Spatial)GetParent<World>().CurrentLevel;
        SetLevelSettings();
    }

    void SetLevelSettings()
    {
        var lvlSettings = level.GetNode<LevelSettings>("LevelSettings");
        DeadYCoordinate = lvlSettings.DeadYCoordinate;
        var mesh = GetNode<MeshInstance>("Mesh");
        (mesh.GetSurfaceMaterial(0) as SpatialMaterial).AlbedoColor = lvlSettings.PlayerColor;
    }

    bool firstReset = false;

    [Export]
    public float MinBlockTransparency { get; set; } = 0.2f;

    public override void _PhysicsProcess(float delta)
    {
        if (!firstReset)
        {
            Reset();
            firstReset = true;
        }

        if (!IsOnFloor())
        {
            velocity.y -= gravity;
        }
        else
        {
            velocity.y = 0;
            velocity.y += jumpForce;
        }

        if (Input.IsActionPressed("ui_left") || touchDir == -1)
        {
            velocity.x -= horizontalAcceleration;
        }
        else if (Input.IsActionPressed("ui_right") || touchDir == 1)
        {
            velocity.x += horizontalAcceleration;
        }
        else
        {
            if (Mathf.Abs(velocity.x) > MinBlockTransparency)
            {
                velocity.x += xFriction * Mathf.Sign(velocity.x) * -1;
            }
            else
            {
                velocity.x = 0;
            }
        }

        velocity.x = Mathf.Clamp(velocity.x, -maxHorizontalSpeed, maxHorizontalSpeed);

        if (!ShouldKill)
        {
            MoveAndSlide(velocity, Vector3.Up);
            CheckForCollisions();
        }

        if (GlobalTransform.origin.y < DeadYCoordinate)
        {
            ShouldKill = true;
        }

        if (ShouldKill)
        {
            Reset();
        }
    }

    int touchDir = 0;
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch evt)
        {
            var vpw = GetViewport().Size.x;
            if (evt.Position.x > vpw / 2)
            {
                touchDir = 1;
            }
            else
            {
                touchDir = -1;
            }

            if (!evt.Pressed)
            {
                touchDir = 0;
            }
        }
    }

    public bool ShouldKill { get; set; } = false;

    void Reset()
    {
        ShouldKill = false;
        level.Rotation = Vector3.Zero;
        Translation = level.GetNode<Spatial>("SpawnPoint").GlobalTransform.origin;
        velocity = Vector3.Zero;
        Respawn?.Invoke();
        RotationDir = 1;
        MakeBlocksTransparent();
        Visible = true;
        CalibrateCamera?.Invoke();
    }

    void CheckForCollisions()
    {
        if (GetSlideCount() > 0)
        {
            var collision = GetSlideCollision(0);
            if (collision.Collider is Node node)
            {
                var block = node.GetParent<Block>();
                if (block.IsPad)
                {
                    level.Rotate(Vector3.Up, Mathf.Deg2Rad(90 * RotationDir));
                    RotationDir *= -1;
                    var pad = node.GetParent<Block>();

                    Translation = new Vector3(pad.GlobalTransform.origin.x, Translation.y, pad.GlobalTransform.origin.z);
                    velocity = Vector3.Zero;

                    CalibrateCamera?.Invoke();

                    MakeBlocksTransparent();
                }
                else if (block.IsBad)
                {
                    ShouldKill = true;
                }
                else if (block.DisableAfterTouch)
                {
                    block.SetActive(false);
                }
            }

            if (collision.Collider is StaticBody body && IsOnFloor())
            {
                LowestPoint = GlobalTransform.origin;
                CalibrateCameraY?.Invoke();
            }
        }
    }

    void SetBlockMinTransparency(Block b)
    {
        b.SetTransparency(MinBlockTransparency, Tween.EaseType.Out, 0.8f);
    }
    void SetBlockMaxTransparency(Block b)
    {
        b.SetTransparency(1f, Tween.EaseType.In, 0.3f);
    }

    void MakeBlocksTransparent()
    {
        var nodes = level.GetChildren();

        foreach (var node in nodes)
        {
            if (node is Block block && !block.IsPad)
            {
                if (RotationDir == 1)
                {
                    if (block.RotationDegrees.y == 90)
                    {
                        SetBlockMinTransparency(block);
                    }
                    else
                    {
                        SetBlockMaxTransparency(block);
                    }
                }
                else if (RotationDir == -1)
                {
                    if (block.RotationDegrees.y == 0)
                    {
                        SetBlockMinTransparency(block);
                    }
                    else
                    {
                        SetBlockMaxTransparency(block);
                    }
                }
            }
        }
    }
}