using Godot;
using System;

public class Camera : Godot.Camera
{
    Player player;
    float lerpSpeed = 0.2f;
    int zOffset = 40;
    int yOffset = 15;

    Tween tween;

    float i_RotationY;

    public override void _Ready()
    {
        tween = new Tween();
        AddChild(tween);

        player = GetNode<Player>("../Player");
        Translation = Vector3.Zero;
        player.CalibrateCamera += Calibrate;
        player.CalibrateCameraY += CalibrateY;

        Calibrate();
        CalibrateY();

        i_RotationY = RotationDegrees.y;
    }

    void Calibrate()
    {
        var translation = new Vector3(0, 0, player.Translation.z - Translation.z + zOffset);
        Translate(translation);
    }

    void CalibrateY()
    {
        var newt = new Vector3(Translation.x, player.LowestPoint.y + yOffset, Translation.z);

        tween.InterpolateProperty(this, "translation:y", Translation.y, newt.y, 0.15f, Tween.TransitionType.Linear, Tween.EaseType.In, 0);
        tween.Start();
    }

    public override void _PhysicsProcess(float delta)
    {
        var t = Translation;
        var pt = player.Translation;

        t = t.LinearInterpolate(new Vector3(pt.x, t.y, t.z), lerpSpeed);
        Translation = t;
    }
}
