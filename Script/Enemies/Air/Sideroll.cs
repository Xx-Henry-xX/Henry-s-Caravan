using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class Sideroll : EnemyBase
{
    int shotCounter = 0;
    bool rollRight = true;
    public Sideroll(GameManager gManager, SFPoint spawnpoint, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = spawnpoint;
        rollRight = position.x < (sfloat)120;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)10, (sfloat)10)) };
        movement = new SFPoint((sfloat)(rollRight ? 2 : -2), (sfloat)0);
        hp = 1;
        baseValue = 120;
        multiplierYAxisOffset = (sfloat)10;
        movement.x = (position.x < (sfloat)120) ? (sfloat)2 : (sfloat)(-2);
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
        if (shootable) shotCounter++;
        if (shotCounter >= (1000000 - spawnRank) / 10000 / 3 * 2 && shootable)
        {
            shotCounter = 0;
            if (shotSealed[0] > 0)
            {
                gManager.rank += 3000;
                gManager.rank = Math.Min(gManager.rank, 999999);
            }
            else
            {
                List<SFPoint> directs;
                SFPoint shotDir = (player.GetSFPosition() - position).Normalized();
                directs = Fan(shotDir, 3, (sfloat)0.4);
                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Strobe6px, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.3), 0)
                ));
                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Strobe8px, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5), 1)
                ));
                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle8px, (sfloat)0));
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("Sideroll", lifeTime % (frames.GetFrameCount("Sideroll") * 3) / 3), Vector2.Zero, 0, Vector2.One));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        if (gManager.loop == 0) return new List<BulletInfo>();
        gManager.p1Score += 110; //110 / bullet
        if ((position - player.GetSFPosition()).LengthSquared() < (sfloat)3600)
        {
            gManager.p1Score += 100; //100 / bullet
            gManager.rank += 1000;
            gManager.rank = Math.Min(gManager.rank, 999999);
            return new List<BulletInfo>();
        }

        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SFPoint shotDir = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2);
        rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Counter8px, position, shotDir)
        ));

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
