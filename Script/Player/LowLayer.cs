using BulletPack;
using Godot;
using System;

public class LowLayer : Node2D
{
    //this thing is *purely* for drawing bomb below the enemies

    PlayerController parent;
    public override void _Ready()
    {
        parent = GetParent<PlayerController>();
    }

    public override void _PhysicsProcess(float delta)
    {
        Update();
    }

    public override void _Draw()
    {
        if (parent.bomberObject.active)
            foreach (DrawInfo frame in parent.bomberObject.GetDrawFrames(parent.playerAnimations))
            {
                DrawSetTransform((Vector2)parent.bomberObject.GetHitBox().center, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.drawOffset - frame.tex.GetSize() / 2);
            }
    }
}
