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

    RandomNumberGenerator random;

    public override void _Ready()
    {
        random = new RandomNumberGenerator();
        tween = new Tween();
        AddChild(tween);

        player = GetNode<Player>("../Player");
        Translation = Vector3.Zero;

        player.CalibrateCamera += Calibrate;
        player.CalibrateCameraY += CalibrateY;
        player.GroundHit += () =>
        {
            shake = true;
        };

        Calibrate();
        CalibrateY();

        i_RotationY = RotationDegrees.y;

        SetBlur(!Blurred, 1f);
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

    Vector3 lastRotation;
    bool shake = false;
    float shakeCd = 0.05f;
    float c_shakeCd = 0.08f;

    float shakeAmount = 0.6f;

    void UpdateShake(float delta)
    {
        if (shake)
        {
            c_shakeCd -= delta;

            if (lastRotation != null)
            {
                RotationDegrees = lastRotation;
            }
            lastRotation = RotationDegrees;

            var x = random.RandfRange(-shakeAmount, shakeAmount);
            var y = random.RandfRange(-shakeAmount, shakeAmount);
            var z = random.RandfRange(-shakeAmount, shakeAmount);
            var offset = new Vector3(x, y, z);

            RotationDegrees += offset;

            if (c_shakeCd < 0)
            {
                shake = false;
                c_shakeCd = shakeCd;
                // to prevent the rotation from not resetting when the c_shakeCd is too small
                RotationDegrees = lastRotation;
            }
        }
    }

    public override void _Process(float delta)
    {
        var t = Translation;
        var pt = player.Translation;

        t = t.LinearInterpolate(new Vector3(pt.x, t.y, t.z), lerpSpeed);
        Translation = t;

        UpdateShake(delta);

        if (c_blurringTime > 0)
        {
            c_blurringTime -= delta;
            if (c_blurringTime < 0)
            {
                if (Environment.DofBlurNearAmount == 0)
                {
                    Environment.DofBlurNearEnabled = false;
                }
                SetBlur(!Blurred, 1f);
            }
        }
    }

    float maxBlur = 0.3f;
    float minBlur = 0.0f;
    float c_blurringTime;

    public bool Blurred => Environment.DofBlurNearEnabled;

    public void SetBlur(bool value, float duration)
    {
        if (value)
        {
            if (!Blurred)
            {
                Environment.DofBlurNearEnabled = true;
            }
            tween.InterpolateProperty(Environment, "dof_blur_near_amount", minBlur, maxBlur, duration, Tween.TransitionType.Linear, Tween.EaseType.In, 0);
        }
        else
        {
            tween.InterpolateProperty(Environment, "dof_blur_near_amount", maxBlur, minBlur, duration, Tween.TransitionType.Linear, Tween.EaseType.Out, 0);
        }
        c_blurringTime = duration;
        tween.Start();
    }
}
