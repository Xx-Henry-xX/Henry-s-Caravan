using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class Search : EnemyBase
{
    int shotCounter = 0;
    public Search(GameManager gManager, SFPoint spawnpoint, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = spawnpoint;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)10, (sfloat)10)) };
        movement = new SFPoint((sfloat)0, (sfloat)3);
        hp = 1;
        baseValue = 100;
        multiplierYAxisOffset = (sfloat)12;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)0, (sfloat)(-2)) };
        shotSealed = new int[shotSealPoints.Length];
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        position += movement;
        if (player.GetSFPosition().y < position.y && movement.y != (sfloat)0)
        {
            movement.y = (sfloat)0;
            movement.x = (player.GetSFPosition().x > position.x ? (sfloat)3 : (sfloat)(-3));
        }
    }

    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        if (shootable && movement.y == (sfloat)0) shotCounter++;
        if (shotCounter >= (1000000 - spawnRank) / 40000 && shootable)
        {
            shotCounter = 0;
            if (shotSealed[0] > 0)
            {
                gManager.rank += 1000;
                gManager.rank = Math.Min(gManager.rank, 999999);
            }
            else
            {
                SFPoint bulletVel = (player.GetSFPosition() - position).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
                rtnValue.Add(new BulletInfo(
                    new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Bean, position + shotSealPoints[0], bulletVel)
                ));
                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle8px, (sfloat)0));
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("Crystal", lifeTime % (frames.GetFrameCount("Crystal") * 2) / 2), Vector2.Zero, 0, Vector2.One));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
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

    public override List<ExplosionFX> Explosions(AudioManager aManager)
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
        aManager.Play("expl_zako", 3, 1);
        return rtnValue;
    }
}
