using Godot;
using System;

// should make a small tutorial on how to do this type of transition

public class Transition : ColorRect
{
    ShaderMaterial transitionShader;
    Tween tween;

    float Offset1
    {
        get
        {
            return (float)transitionShader.GetShaderParam("offset1");
        }
        set
        {
            transitionShader.SetShaderParam("offset1", value);
        }
    }

    float Offset2
    {
        get
        {
            return (float)transitionShader.GetShaderParam("offset2");
        }
        set
        {
            transitionShader.SetShaderParam("offset2", value);
        }
    }

    float offset1Duration = 0.7f;
    float offset2Duration = 0.3f;

    public override void _Ready()
    {
        tween = GetParent().GetNode<Tween>("Tween");
        transitionShader = (ShaderMaterial)Material;
        GetTree().Root.GetNode("World").GetNode<Player>("Player").CalibrateCamera += ScreenTransition;
    }

    void ScreenTransition()
    {
        Offset1 = 0;
        Offset2 = 0;
        tween.StopAll();
        tween.InterpolateProperty(this, "Offset1", Offset1, 1, offset1Duration, Tween.TransitionType.Quad, Tween.EaseType.InOut);
        tween.InterpolateProperty(this, "Offset2", Offset2, 1, offset2Duration, Tween.TransitionType.Quad, Tween.EaseType.InOut);
        tween.Start();
    }
}
