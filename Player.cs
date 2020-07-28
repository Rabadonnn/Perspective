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

    public delegate void PlayerEvent();

    public event PlayerEvent Respawn;
    public event PlayerEvent CalibrateCamera;
    public event PlayerEvent CalibrateCameraY;
    public event PlayerEvent GroundHit;

    Vector3 velocity = new Vector3();

    public int RotationDir { get; set; } = 1;

    public int DeadYCoordinate { get; private set; } = -10;

    Spatial level;

    public Vector3 LowestPoint { get; private set; } = Vector3.Zero;

    Label debugLabel;

    public override void _Ready()
    {
        level = (Spatial)GetParent<World>().CurrentLevel;
        SetLevelSettings();

        debugLabel = GetNode<Label>("../UI/DebugLabel");
    }

    void SetLevelSettings()
    {
        var lvlSettings = level.GetNode<LevelSettings>("LevelSettings");
        DeadYCoordinate = lvlSettings.DeadYCoordinate;
        var mesh = GetNode<MeshInstance>("Mesh");
        (mesh.GetSurfaceMaterial(0) as SpatialMaterial).AlbedoColor = lvlSettings.PlayerColor;

        var particles = GetNode<CPUParticles>("CPUParticles");
        var oldColors = particles.ColorRamp.Colors;
        var newColors = new Color[]
        {
            new Color(lvlSettings.PlayerColor.r, lvlSettings.PlayerColor.g, lvlSettings.PlayerColor.b, 0.8f),
            new Color(lvlSettings.PlayerColor.r, lvlSettings.PlayerColor.g, lvlSettings.PlayerColor.b, 0)
        };
        particles.ColorRamp = new Gradient()
        {
            Colors = newColors
        };
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

        if (!ShouldKill)
        {
            Jump();
            Move();
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

    void Jump()
    {
        if (!IsOnFloor())
        {
            velocity.y -= gravity;
        }
        else
        {
            velocity.y = 0;
            velocity.y += jumpForce;
            GroundHit?.Invoke();
        }
    }

    void Move()
    {
        CheckSlideInput();
        if (Input.IsActionPressed("ui_left") || slideDir == -1)
        {
            velocity.x -= horizontalAcceleration;
        }
        else if (Input.IsActionPressed("ui_right") || slideDir == 1)
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
    }

    Vector2 lastPoint;
    int slideDir = 0;
    int slideThreshold = 100;
    void CheckSlideInput()
    {
        var mouse = GetViewport().GetMousePosition();
        if (Input.IsMouseButtonPressed((int)(ButtonList.Left)))
        {
            if (lastPoint == null)
            {
                lastPoint = mouse;
            }
            else
            {
                if (mouse.x - lastPoint.x > slideThreshold)
                {
                    slideDir = 1;
                }
                if (mouse.x - lastPoint.x < -slideThreshold)
                {
                    slideDir = -1;
                }
            }

        }
        if (!Input.IsMouseButtonPressed((int)(ButtonList.Left)))
        {
            lastPoint = mouse;
            slideDir = 0;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventScreenTouch evt)
        {
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
        Visible = true;
        CalibrateCamera?.Invoke();
    }

    Block lastPad;

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
                    if (lastPad == null)
                    {
                        level.Rotate(Vector3.Up, Mathf.Deg2Rad(90 * RotationDir));
                        RotationDir *= -1;
                        var pad = node.GetParent<Block>();

                        Translation = new Vector3(pad.GlobalTransform.origin.x, Translation.y, pad.GlobalTransform.origin.z);
                        velocity = Vector3.Zero;

                        CalibrateCamera?.Invoke();
                        lastPad = block;
                        velocity.x = 0;
                    }
                }
                else if (block.IsBad)
                {
                    ShouldKill = true;
                }
                else if (block.DisableAfterTouch && IsOnFloor())
                {
                    block.MakeItFall();
                }

                if (IsOnFloor() && !block.DisableAfterTouch && !block.IsBad)
                {
                    LowestPoint = GlobalTransform.origin;
                    CalibrateCameraY?.Invoke();
                }

                if (!block.IsPad)
                {
                    lastPad = null;
                }
            }
        }
    }
}
