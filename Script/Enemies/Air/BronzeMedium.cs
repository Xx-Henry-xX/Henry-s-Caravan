using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class BronzeMedium : EnemyBase
{
    int shotCounter = 0;
    public BronzeMedium(GameManager gManager, SFPoint spawnpoint, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = spawnpoint;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)26, (sfloat)10)) };
        movement = new SFPoint((sfloat)0, (sfloat)(position.y < (sfloat)160 ? 2 : -2));
        hp = 80;
        baseValue = 1000;
        multiplierYAxisOffset = (sfloat)10;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)0, (sfloat)0) };
        shotSealed = new int[shotSealPoints.Length];
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        position += movement;
    }

    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        shotCounter++;
        if (shotCounter >= (1000000 - spawnRank) / 10000 / 2 && shootable)
        {
            shotCounter = 0;
            if (shotSealed[0] > 0)
            {
                gManager.rank += 5000;
                gManager.rank = Math.Min(gManager.rank, 999999);
            }
            else
            {
                SFPoint shotDir = (player.GetSFPosition() - position).Normalized();
                List<SFPoint> directs = Fan(shotDir, 5, (sfloat)(0.025f) * sfloat.FromRaw(0x40490fdb));
                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.TJO, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                ));
                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>
        {
            new DrawInfo(frames.GetFrame("BronzeMed", 0), Vector2.Zero, 0, Vector2.One, damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1))
        };
        if (lifeTime % 2 == 0) rtnValue.Add(new DrawInfo(frames.GetFrame("BronzeMed", 1), Vector2.Down * 11, 0, Vector2.One));
        rtnValue.AddRange(GetDrawSealPositions(frames));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
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

        SFPoint shotDir = new SFPoint((sfloat)0, (sfloat)1) * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
        List<SFPoint> dirs = Round(shotDir, 16);

        List<BulletInfo> rtnValue = new List<BulletInfo>();
        foreach (SFPoint dir in dirs)
        {
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter16px, position, dir)));
        }
        return rtnValue;
    }

    public override List<ExplosionFX> Explosions()
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

        return rtnValue;
    }
}
