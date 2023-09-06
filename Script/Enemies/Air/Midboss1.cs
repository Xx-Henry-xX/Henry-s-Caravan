using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class Midboss1 : EnemyBase
{
    int shotCounter = 0;
    int mode = 0;
    int impatience = 1200;
    readonly int startShootingAt;
    public Midboss1(GameManager gManager, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = new SFPoint((sfloat)120, (sfloat)(-24));
        hitboxGroup = new SFAABB[] { new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)26, (sfloat)26)) };
        movement = new SFPoint((sfloat)0, (sfloat)12);
        hp = 750;
        baseValue = 5000;
        multiplierYAxisOffset = (sfloat)30;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] { new SFPoint((sfloat)(-18), (sfloat)18), new SFPoint((sfloat)(18), (sfloat)18) };
        shotSealed = new int[shotSealPoints.Length];
        startShootingAt = (1000000 - spawnRank) / 20000;
        Array.Fill(shotSealed, 0);
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        position += movement;
        impatience--;
        if (impatience <= 0) movement.y = (sfloat)(-3);
        else if (movement.y > (sfloat)0) movement.y -= (sfloat)1;
        
        if (position.x > player.GetSFPosition().x)
        {
            movement.x -= (sfloat)0.2;
            movement.x = sfloat.Max(movement.x, (sfloat)(-3));
        }
        else
        {
            movement.x += (sfloat)0.2;
            movement.x = sfloat.Min(movement.x, (sfloat)(3));
        }
    }

    public override bool LeaveCheck()
    {
        leaveTimer--;
        foreach (SFAABB nmeBox in hitboxGroup) if (IntersectBoxVSBox(leaveArea, nmeBox.Offset(position))) return false;
        return leaveTimer <= 0 && impatience <= 0;
    }
    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();

        SealCheck(player.GetSFPosition());
        if (shootable)
        {
            shotCounter++;
            switch (mode)
            {
                case 0:
                    if (shotCounter == startShootingAt)
                    {
                        if (shotSealed[0] > 0)
                        {
                            if (shotSealed[1] > 0)
                            {
                                gManager.rank += 5000;
                                gManager.rank = Math.Min(gManager.rank, 999999);
                            }
                            else
                            {
                                SFPoint shotDir;
                                List<SFPoint> directs;
                                shotDir = (player.GetSFPosition() - (position + shotSealPoints[1])).Normalized();
                                directs = Fan(shotDir, 5, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                    new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                                ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                            }
                        }
                        else
                        {
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = (player.GetSFPosition() - (position + shotSealPoints[0])).Normalized();
                            directs = Fan(shotDir, 5, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                        }
                    }
                    else if (shotCounter == startShootingAt + 10)
                    {
                        if (shotSealed[1] > 0)
                        {
                            if (shotSealed[0] > 0)
                            {
                                gManager.rank += 5000;
                                gManager.rank = Math.Min(gManager.rank, 999999);
                            }
                            else
                            {
                                SFPoint shotDir;
                                List<SFPoint> directs;
                                shotDir = (player.GetSFPosition() - (position + shotSealPoints[0])).Normalized();
                                directs = Fan(shotDir, 5, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                    new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                                ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                            }
                        }
                        else
                        {
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = (player.GetSFPosition() - (position + shotSealPoints[1])).Normalized();
                            directs = Fan(shotDir, 5, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))                                
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                        }
                        shotCounter = 0;
                        mode++;
                        mode %= 3;
                    }
                    break;
                case 1:
                    if (shotCounter == startShootingAt)
                    {
                        if (shotSealed[0] > 0)
                        {
                            if (shotSealed[1] > 0)
                            {
                                gManager.rank += 16000;
                                gManager.rank = Math.Min(gManager.rank, 999999);
                            }
                            else
                            {
                                SFPoint shotDir;
                                List<SFPoint> directs;
                                shotDir = new SFPoint(libm.sinf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)), libm.cosf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)));
                                directs = Round(shotDir, 16);
                                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                    new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                                ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                            }
                        }
                        else
                        {
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = new SFPoint(libm.sinf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)), libm.cosf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)));
                            directs = Round(shotDir, 16);
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                        }
                    }
                    else if (shotCounter == startShootingAt + 10)
                    {
                        if (shotSealed[1] > 0)
                        {
                            if (shotSealed[0] > 0)
                            {
                                gManager.rank += 16000;
                                gManager.rank = Math.Min(gManager.rank, 999999);
                            }
                            else
                            {
                                SFPoint shotDir;
                                List<SFPoint> directs;
                                shotDir = new SFPoint((sfloat)1, (sfloat)0);
                                directs = Round(shotDir, 16);
                                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                    new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                                ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                            }
                        }
                        else
                        {
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = new SFPoint(libm.sinf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)), libm.cosf((sfloat)0.0625 * sfloat.FromRaw(0x40490fdb)));
                            directs = Round(shotDir, 16);
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5)) 
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                        }
                        shotCounter = 0;
                        mode++;
                        mode %= 3;
                    }
                    break;
                default:
                    if (shotCounter >= startShootingAt + 90)
                    {
                        if (shotSealed[0] < 0) shotSealed[0] = 0;
                        if (shotSealed[1] < 0) shotSealed[1] = 0;
                        shotCounter = 0;
                        mode++;
                        mode %= 3;
                        break;
                    }
                    if (shotCounter >= startShootingAt && (shotCounter - startShootingAt) % 10 == 0)
                    {
                        sfloat angleDet = (sfloat)((shotCounter - startShootingAt) / 10);
                        if (shotSealed[0] > 0)
                        {
                            gManager.rank += 3000;
                            gManager.rank = Math.Min(gManager.rank, 999999);
                        }
                        else
                        {
                            shotSealed[0] = -1;
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = new SFPoint(libm.sinf((sfloat)angleDet / (sfloat)16 * sfloat.FromRaw(0x40490fdb)), libm.cosf((sfloat)angleDet / (sfloat)16 * sfloat.FromRaw(0x40490fdb)));
                            directs = Round(shotDir, 3);
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Whirl, position + shotSealPoints[0], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                        }
                        if (shotSealed[1] > 0)
                        {
                            gManager.rank += 3000;
                            gManager.rank = Math.Min(gManager.rank, 999999);
                        }
                        else
                        {
                            shotSealed[1] = -1;
                            SFPoint shotDir;
                            List<SFPoint> directs;
                            shotDir = new SFPoint(libm.sinf((sfloat)angleDet / (sfloat)(-16) * sfloat.FromRaw(0x40490fdb)), libm.cosf((sfloat)angleDet / (sfloat)16 * sfloat.FromRaw(0x40490fdb)));
                            directs = Round(shotDir, 3);
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Purple, BulletPack.BltShape.Whirl, position + shotSealPoints[1], shootTo * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5))
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                        }
                    }
                    break;
            }

        }
        SealCheck(player.GetSFPosition());
        return rtnValue;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        if (lifeTime % 2 == 0) rtnValue.Add(new DrawInfo(frames.GetFrame("Midbosses", 2), Vector2.Up * 33, 0, Vector2.One));
        rtnValue.Add(new DrawInfo(frames.GetFrame("Midbosses", 0), Vector2.Zero, 0, Vector2.One, damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1)));
        rtnValue.AddRange(GetDrawSealPositions(frames));
        return rtnValue;

        //(hp <= 240 && lifetime % (hp / 16 + 1) == 0) ? new Color(1, 0, 0) :
        //damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1)
    }

    public override List<BulletInfo> Kill(GameManager gManager, EnemyManager eManager, BulletManager bManager, AudioManager aManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = RevengeShot(gManager, eManager, bManager, aManager, player);

        int multi = 10;
        if (!player.GetFreeX10())
        {
            sfloat diff = player.GetHitBox().center.y - (position.y + multiplierYAxisOffset);
            bool brk = false;
            while (multi > 1 && !brk)
            {
                brk = diff <= (sfloat)(11 - multi) * (sfloat)20;
                if (!brk) multi--;
            }
        }
        gManager.p1Score += (uint)(baseValue * multi);

        eManager.NewScoreFX(new ScoreFX(0, multi, position, SFPoint.ZeroSFPoint));

        gManager.medalDropCounter -= 10;
        if (multi == 10) gManager.medalDropCounter -= 10;

        bManager.CreateItem(new Item(gManager, ItemType.Bomb, position));

        return rtnValue;
    }

    public override List<ExplosionFX> Explosions(AudioManager aManager)
    {
        List<SFPoint> offsets;
        SFPoint vel = SFPoint.ZeroSFPoint;
        sfloat randius;
        sfloat theta;
        int count = 55;
        sfloat decay;
        int timer;
        List<ExplosionFX> rtnValue = new List<ExplosionFX>();

        timer = 60;
        offsets = Round(new SFPoint((sfloat)10, (sfloat)0).Rotated((sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb)), 40);
        foreach (SFPoint direct in offsets)
        {
            rtnValue.Add(new ExplosionFX(position, direct, (sfloat)0.9, Math.Abs(timer) / 2, 2));
            timer -= 3;
        }

        for (int i = 0; i < count; i++)
        {
            randius = libm.sqrtf((sfloat)rng.RandiRange(100, count * count)) / (sfloat)10;
            theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
            vel.x = randius * libm.sinf(theta);
            vel.y = randius * libm.cosf(theta);
            decay = (sfloat)rng.Randi() / (sfloat)uint.MaxValue / (sfloat)10 + (sfloat)0.8f;
            timer = rng.RandiRange(0, 16);

            rtnValue.Add(new ExplosionFX(position, vel, decay, timer, 1));
            //count -= (sfloat)0.05f;
        }

        offsets = new List<SFPoint>();
        theta = (sfloat)rng.RandiRange(0, 359);
        for (int i = 0; i < 4; i++)
        {
            randius = libm.sqrtf((sfloat)rng.RandiRange(900, 1600));
            theta += (sfloat)rng.RandiRange(30, 120);
            theta %= (sfloat)360;
            vel.x = randius * libm.sinf(theta / (sfloat)360 * sfloat.FromRaw(0x40c90fdb));
            vel.y = randius * libm.cosf(theta / (sfloat)360 * sfloat.FromRaw(0x40c90fdb));
            offsets.Add(vel);
        }
        for (int i = 0; i < 4; i++)
        {
            for (int j = 0; j < count; j++)
            {
                randius = libm.sqrtf((sfloat)rng.RandiRange(100, count * count)) / (sfloat)10;
                theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
                vel.x = randius * libm.sinf(theta);
                vel.y = randius * libm.cosf(theta);
                decay = (sfloat)rng.Randi() / (sfloat)uint.MaxValue / (sfloat)10 + (sfloat)0.8f;
                timer = rng.RandiRange(0, 16);

                rtnValue.Add(new ExplosionFX(position + offsets[i], vel, decay, timer, 0, i * 6 + 6));
            } 
        }
        aManager.Play("expl_large", 3, 3);
        return rtnValue;
    }
}
