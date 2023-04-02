using Godot;
using System;
using System.Collections.Generic;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using BulletPack;

public class EnemyManager : Node2D
{
    public List<EnemyBase> enemies = new List<EnemyBase>();
    private List<ExplosionFX> explfx = new List<ExplosionFX>();
    private List<ScoreFX> scoreFXs = new List<ScoreFX>();

    private GameManager gManager;
    private BulletManager bManager;
    private PlayerController player;
    private readonly SpriteFrames enemyAnimations = GD.Load<SpriteFrames>("res://Sprites/Enemies/Enemies.tres");
    private readonly SpriteFrames fxAnimations = GD.Load<SpriteFrames>("res://Sprites/Effects/FX.tres");

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
        bManager = GetNode<BulletManager>("/root/Main/BulletManager");
        player = GetNode<PlayerController>("/root/Main/Player");
    }

    public override void _PhysicsProcess(float delta)
    {
        Stack<int> removeQueue = new Stack<int>();
        int index = 0;
        foreach (EnemyBase nme in enemies)
        {
            nme.Movement(gManager, this, bManager, player);
            if (nme.LeaveCheck()) removeQueue.Push(index);
            else if (nme.DamageCheck(gManager, player))
            {
                foreach (BulletInfo bltNfo in nme.Kill(gManager, this, bManager, player))
                {
                    bManager.CreateBullet(bltNfo.blt, bltNfo.drawOnBottom);
                }
                foreach (ExplosionFX fx in nme.Explosions())
                {
                    explfx.Add(fx);
                }
                removeQueue.Push(index);
            }
            else
            {
                foreach (BulletInfo bltNfo in nme.Shoot(gManager, this, bManager, player))
                {
                    bManager.CreateBullet(bltNfo.blt, bltNfo.drawOnBottom);
                }
            }
            nme.lifeTime++;
            index++;
        }
        while (removeQueue.Count != 0) enemies.RemoveAt(removeQueue.Pop());

        removeQueue.Clear();
        index = 0;
        foreach (ExplosionFX fx in explfx)
        {
            fx.Process();
            if (fx.DelCheck()) removeQueue.Push(index);
            fx.lifeTime++;
            index++;
        }
        while (removeQueue.Count != 0) explfx.RemoveAt(removeQueue.Pop());

        removeQueue.Clear();
        index = 0;
        foreach (ScoreFX fx in scoreFXs)
        {
            fx.Process();
            if (fx.DelCheck()) removeQueue.Push(index);
            fx.lifeTime++;
            index++;
        }
        while (removeQueue.Count != 0) scoreFXs.RemoveAt(removeQueue.Pop());

        Update();
    }

    public override void _Draw()
    {
        foreach (ExplosionFX fx in explfx)
        {
            foreach (DrawInfo frame in fx.GetDrawFrames(fxAnimations))
            {
                DrawSetTransform((Vector2)fx.position, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2 + frame.drawOffset);
            }
        }

        foreach (EnemyBase nme in enemies)
        {
            foreach (DrawInfo frame in nme.GetDrawFrames(gManager, player, enemyAnimations))
            {
                DrawSetTransform((Vector2)nme.position, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2 + frame.drawOffset, frame.color);
            }
        }

        foreach (ScoreFX fx in scoreFXs)
        {
            foreach (DrawInfo frame in fx.GetDrawFrames(fxAnimations))
            {
                DrawSetTransform((Vector2)fx.pos, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2);
            }
        }
    }

    public void NewEnemy(EnemyBase nme, bool drawOnBottom = false)
    {
        int insertionPoint = nme.drawOrder;
        int index = 0;
        while (index < enemies.Count - 1)
        {
            if (drawOnBottom ? enemies[index].drawOrder >= insertionPoint : enemies[index].drawOrder > insertionPoint) break;
            index++;
        }
        enemies.Insert(index, nme);
    }

    public void NewScoreFX(ScoreFX fx)
    {
        scoreFXs.Add(fx);
    }
}
