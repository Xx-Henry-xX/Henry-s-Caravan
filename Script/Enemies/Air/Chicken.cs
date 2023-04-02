using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class Chicken : EnemyBase
{
    int shotCounter = 0;
    public Chicken(GameManager gManager, PlayerController player, SFPoint spawnpoint, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = spawnpoint;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)14, (sfloat)10)) };
        movement = new SFPoint((sfloat)0, (sfloat)0);
        hp = 5;
        baseValue = 250;
        multiplierYAxisOffset = (sfloat)10;
        movement.x = !(position.x > player.GetHitBox().center.x) ? (sfloat)2 : (sfloat)(-2);
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)0, (sfloat)5) };
        shotSealed = new int[shotSealPoints.Length];
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        position.x += movement.x;
        position.y += (sfloat)3;
        if (movement.y == (sfloat)0)
        {
            if (movement.x > (sfloat)0 && position.x > player.GetSFPosition().x)
            {
                shotCounter = 0;
                movement.y = (sfloat)(-1);
            }
            else if (movement.x < (sfloat)0 && position.x < player.GetSFPosition().x)
            {
                shotCounter = 0;
                movement.y = (sfloat)(1);
            }
        }
        else movement.x = Clamp(movement.x + movement.y, (sfloat)(-4), (sfloat)4);
    }

    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        if (shotCounter == 0 && shootable)
        {
            shotCounter = 1;
            if (shotSealed[0] > 0)
            {
                gManager.rank += 3000;
                gManager.rank = Math.Min(gManager.rank, 999999);
            }
            else
            {
                SFPoint bulletVel = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.3);
                rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Bean, position + shotSealPoints[0], bulletVel, 0)
                ));
                bulletVel = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.4);
                rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Bean, position + shotSealPoints[0], bulletVel, 1)
                ));
                bulletVel = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
                rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Bean, position + shotSealPoints[0], bulletVel, 2)
                ));
                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle8px, (sfloat)0));
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("Chicken", movement.y == (sfloat)0 ? 0 : (movement.y == (sfloat)1 ? 1 : 2)), Vector2.Zero, 0, Vector2.One));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        if (gManager.loop == 0) return new List<BulletInfo>();
        gManager.p1Score += 440; //110 / bullet
        if ((position - player.GetSFPosition()).LengthSquared() < (sfloat)3600)
        {
            gManager.p1Score += 400; //100 / bullet
            gManager.rank += 4000;
            gManager.rank = Math.Min(gManager.rank, 999999);
            return new List<BulletInfo>();
        }

        SFPoint shotDir = new SFPoint((sfloat)0, (sfloat)1) * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
        List<SFPoint> dirs = Fan(shotDir, 4, sfloat.FromRaw(0x3f490fdb)); //pi/4

        List<BulletInfo> rtnValue = new List<BulletInfo>();
        foreach (SFPoint dir in dirs)
        {
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter8px, position, dir)));
        }
        return rtnValue;
    }

    public override List<ExplosionFX> Explosions()
    {
        SFPoint vel = SFPoint.ZeroSFPoint;
        sfloat randius;
        sfloat theta;
        int count = 30;
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
