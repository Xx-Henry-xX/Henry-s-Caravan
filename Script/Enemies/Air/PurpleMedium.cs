using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class PurpleMedium : EnemyBase
{
    int shotCounter = 0;
    int pattern = 0;
    int mode = 0;
    int progress = 0;
    public PurpleMedium(GameManager gManager, SFPoint spawnpoint, int pattern, int drawOrder = 0) : base(gManager, drawOrder)
    {
        this.pattern = pattern;
        position = spawnpoint;
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)10, (sfloat)10)) };
        movement = new SFPoint((sfloat)(position.x < (sfloat)120 ? 1 : -1), (sfloat)4);
        hp = 50;
        baseValue = 1000;
        multiplierYAxisOffset = (sfloat)10;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)(-12), (sfloat)3), new SFPoint((sfloat)(12), (sfloat)3) };
        shotSealed = new int[shotSealPoints.Length];
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        position += movement;
        switch (mode)
        {
            case 0:
                if ((player.GetSFPosition().y - position.y) <= (sfloat)90) mode++;
                break;
            case 1:
            case 3:
                movement.y -= (sfloat)0.2;
                progress++;
                if (progress >= 4)
                {
                    mode++;
                    progress = 0;
                }
                break;
            case 2:
            case 4:
                movement.y -= (sfloat)0.2;
                break;
        }
        movement.y = sfloat.Max(movement.y, (sfloat)(-4));
    }

    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        // 
        if (shootable) shotCounter++;
        if (mode == 2)
        {
            switch (pattern)
            {
                case 0:
                    if (shotCounter >= (1000000 - spawnRank) / 10000 / 8 && shootable)
                    {
                        shotCounter = 0;
                        progress++;
                        if (progress > 3)
                        {
                            mode++;
                            progress = 0;
                            shotSealed[0] = 0;
                            shotSealed[1] = 0;
                        }
                        else
                        {
                            for (int i = 0; i <= 1; i++)
                            {
                                if (shotSealed[i] > 0)
                                {
                                    gManager.rank += 1000 * progress;
                                    gManager.rank = Math.Min(gManager.rank, 999999);
                                }
                                else
                                {
                                    shotSealed[i] = -1;
                                    SFPoint shotDir = (player.GetSFPosition() - (position + shotSealPoints[i])).Normalized();
                                    List<SFPoint> directs = Fan(shotDir, progress, (sfloat)(0.025f) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs)
                                        rtnValue.Add(new BulletInfo(
                                            new Bullet(BltColor.Yellow, BltShape.Bean, position + shotSealPoints[i], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                                        ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[i], FlashType.Circle16px, (sfloat)0));
                                }
                            }
                        }
                    }
                    break;
                default:
                    if (shotCounter >= 0 && shootable)
                    {
                        shotCounter = 0;
                        progress++;
                        if (progress > spawnRank / 40000 + 10)
                        {
                            mode++;
                            progress = 0;
                            shotSealed[0] = 0;
                            shotSealed[1] = 0;
                        }
                        for (int i = 0; i <= 1; i++)
                        {
                            if (shotSealed[i] > 0)
                            {
                                gManager.rank += 1000;
                                gManager.rank = Math.Min(gManager.rank, 999999);
                            }
                            else
                            {
                                shotSealed[i] = -1;
                                SFPoint bulletVel = (player.GetSFPosition() - (position + shotSealPoints[i])).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
                                rtnValue.Add(new BulletInfo(
                                    new Bullet(BltColor.Yellow, BltShape.Bean, position + shotSealPoints[i], bulletVel)
                                ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[i], FlashType.Circle16px, (sfloat)0));
                            }
                        }
                    }
                    break;
            }
        }
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        int frame = 0;
        switch (mode)
        {
            case 1:
                frame = progress;
                break;
            case 3:
                frame = 4 - progress;
                break;
            case 2:
                frame = 4;
                break;
        }
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("PurpleMed", frame), Vector2.Zero, 0, Vector2.One, damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1)));
        if (mode == 4) rtnValue.AddRange(GetDrawSealPositions(frames));
        return rtnValue;
    }

    public override List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        if (gManager.loop == 0) return new List<BulletInfo>();
        gManager.p1Score += 2640; //110 / bullet
        if ((position - player.GetSFPosition()).LengthSquared() < (sfloat)3600)
        {
            gManager.p1Score += 2400; //100 / bullet
            gManager.rank += 24000;
            gManager.rank = Math.Min(gManager.rank, 999999);
            return new List<BulletInfo>();
        }

        SFPoint shotDir = new SFPoint((sfloat)0, (sfloat)1);
        List<SFPoint> dirs = Round(shotDir, 8);

        List<BulletInfo> rtnValue = new List<BulletInfo>();
        foreach (SFPoint dir in dirs)
        {
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter16px, position, dir * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5), 0)));
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter12px, position, dir * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.25), 1)));
            rtnValue.Add(new BulletInfo(new Bullet(BltColor.Purple, BltShape.Counter8px, position, dir * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2), 2)));
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
