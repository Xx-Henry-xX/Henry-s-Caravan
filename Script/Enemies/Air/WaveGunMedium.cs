using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class WaveGunMedium : EnemyBase
{
    int shotCounter = 0;
    int mode = 0;
    public WaveGunMedium(GameManager gManager, SFPoint spawnpoint, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = spawnpoint;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)14, (sfloat)14)) };
        movement = new SFPoint((sfloat)0, (sfloat)5);
        hp = 30;
        baseValue = 1000;
        multiplierYAxisOffset = (sfloat)20;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)0, (sfloat)7) };
        shotSealed = new int[shotSealPoints.Length];
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        position += movement;
        switch (mode)
        {
            case 0:
                if (position.y >= (sfloat)50)
                {
                    movement.x = (sfloat)(position.x < (sfloat)120 ? 5 : -5);
                    mode++;
                }
                break;
            case 1:
                if (position.y >= (sfloat)100)
                {
                    movement = SFPoint.ZeroSFPoint;
                    mode++;
                }
                break;
            case 2:
                if (shotCounter >= (1000000 - spawnRank) / 10000 / 4 + 25)
                {
                    shotSealed[0] = 0;
                    movement = new SFPoint((sfloat)0, (sfloat)(-5));
                    mode++;
                }
                break;
        }
    }

    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        shotCounter++;
        if (mode == 2 && shootable)
        {
            if (shotSealed[0] > 0)
            {
                gManager.rank += 1000;
                gManager.rank = Math.Min(gManager.rank, 999999);
            }
            else
            {
                shotSealed[0] = -1;
                SFPoint bulletVel = new SFPoint((sfloat)0, (sfloat)8);
                rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Pulse, position + shotSealPoints[0], bulletVel)
                ));
                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.None, (sfloat)0));
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("Pulsar", lifeTime % (frames.GetFrameCount("Pulsar") * 2) / 2), Vector2.Zero, 0, Vector2.One, damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1)));
        rtnValue.AddRange(GetDrawSealPositions(frames));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        if (gManager.loop == 0) return new List<BulletInfo>();
        gManager.p1Score += 1760; //110 / bullet
        if ((position - player.GetSFPosition()).LengthSquared() < (sfloat)3600)
        {
            gManager.p1Score += 1600; //100 / bullet
            gManager.rank += 16000;
            gManager.rank = Math.Min(gManager.rank, 999999);
            return new List<BulletInfo>();
        }

        SFPoint shotDir = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
        List<SFPoint> dirs = Round(shotDir, 16);

        List <BulletInfo> rtnValue = new List<BulletInfo>();
        foreach(SFPoint dir in dirs)
        {
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter12px, position, dir)));
        }
        return rtnValue;
    }

    public override List<ExplosionFX> Explosions(AudioManager aManager)
    {
        SFPoint vel = SFPoint.ZeroSFPoint;
        sfloat randius;
        sfloat theta;
        int count = 55;
        sfloat decay;
        int timer;
        List<ExplosionFX> rtnValue = new List<ExplosionFX>();

        for (int i = 0; i < count; i++)
        {
            randius = libm.sqrtf((sfloat)rng.RandiRange(100, count * count)) / (sfloat)10;
            theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
            vel.x = randius * libm.sinf(theta);
            vel.y = randius * libm.cosf(theta);
            decay = (sfloat)rng.Randi() / (sfloat)uint.MaxValue / (sfloat)10 + (sfloat)0.8f;
            timer = rng.RandiRange(0, 16);

            rtnValue.Add(new ExplosionFX(position, vel, decay, timer));
            //count -= (sfloat)0.05f;
        }
        aManager.Play("expl_med", 3, 2);
        return rtnValue;
    }
}
