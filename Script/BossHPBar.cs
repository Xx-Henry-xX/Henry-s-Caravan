using Godot;
using System;
using SoftFloat;

public class BossHPBar : NinePatchRect
{
    private GameManager gManager;
    private TextureRect p1LivesRect;
    private RichTextLabel lastlifeLabel;
    //private readonly int[] livesPos = { 16, 18, 20, 22, 24, 24, 24, 24 };
    private readonly SpriteFrames barFrames = GD.Load<SpriteFrames>("res://Sprites/HUD/BossHPBar.tres");

    public int animationFrame = 0; //(30FPS; divide by 2 when in use) 0: hidden / 1~4: expanding/shrinking / 5~7: content expanding
    public int initialHP = -1;
    //public int[] marks = {-1, -1}; //will be used later for segmented hp bar

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
        p1LivesRect = GetNode<TextureRect>("../1PLives");
        lastlifeLabel = GetNode<RichTextLabel>("../LastLifeText"); 
    }

    public override void _PhysicsProcess(float delta)
    {
        if (gManager.boss == null)
        {
            initialHP = -1;
            animationFrame--;
        }
        else
        {
            if (initialHP == -1)
            {
                initialHP = gManager.boss.hp;
                GD.Print("boss initial:" + initialHP);
            }
            animationFrame++;
        }

        animationFrame = Math.Clamp(animationFrame, 0, 16);
        p1LivesRect.SetPosition(new Vector2(0, 16 + animationFrame / 2));
        lastlifeLabel.SetPosition(new Vector2(0, 16 + animationFrame / 2));
        if (animationFrame / 2 == 0)
        {
            Visible = false;
            return;
        }
        Visible = true;
        Texture = barFrames.GetFrame("Under", Math.Min(animationFrame / 2 - 1, 3));
        Update();
    }

    public override void _Draw()
    {
        base._Draw();

        if (animationFrame / 2 >= 5 && gManager.boss != null)
        {
            sfloat length = libm.ceilf((sfloat)gManager.boss.hp / (sfloat)initialHP * (sfloat)224);
            DrawSetTransform(Vector2.Right * 8, 0, new Vector2((float)length, 1));
            DrawTexture(
                barFrames.GetFrame("Progress", Math.Min(animationFrame / 2 - 5, 2)),
                Vector2.Zero,
                new Color(1, 0, 0)
            );
        }
    }
}
