using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public class BossGreatBummer : EnemyBase
{
    int shotCounter = 0;
    int mode = -1;
    readonly int startShootingAt;
    SFPoint directionCache = SFPoint.ZeroSFPoint;
    public BossGreatBummer(GameManager gManager, int drawOrder = 0) : base(gManager, drawOrder)
    {
        position = new SFPoint((sfloat)120, (sfloat)(-40));
        hitboxGroup = new SFAABB[] {
            new SFAABB(new SFPoint((sfloat)0, (sfloat)0), new SFPoint((sfloat)40, (sfloat)40)),
            new SFAABB(new SFPoint((sfloat)0, (sfloat)(-16)), new SFPoint((sfloat)80, (sfloat)16))
        };
        movement = new SFPoint((sfloat)0, (sfloat)15);
        hp = 5000;
        baseValue = 10000;
        multiplierYAxisOffset = (sfloat)10;
        leaveTimer = 30;
        shotSealPoints = new SFPoint[] {
            new SFPoint((sfloat)(-40), (sfloat)0), //left thruster
            new SFPoint((sfloat)40, (sfloat)0), //right thruster
            new SFPoint((sfloat)0, (sfloat)(-4)), //center
            new SFPoint((sfloat)(-8), (sfloat)32), //left shotgun
            new SFPoint((sfloat)8, (sfloat)32), //right shotgun
            new SFPoint((sfloat)0, (sfloat)(-24)) //penalizer
        };
        shotSealed = new int[] {
            0, //left thruster
            0, //right thruster
            0, //center
            0, //left shotgun
            0, //right shotgun
            -1 //penalizer
        };
        startShootingAt = (1000000 - spawnRank) / 20000;
    }

    public override void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        position += movement;
        if (movement.y > (sfloat)0) movement.y -= (sfloat)1;
        else if (mode == -1) mode = 0;
        
    }

    public override bool LeaveCheck()
    {
        leaveTimer--;
        foreach (SFAABB nmeBox in hitboxGroup) if (IntersectBoxVSBox(leaveArea, nmeBox.Offset(position))) return false;
        return leaveTimer <= 0;
    }
    public override List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        List<BulletInfo> rtnValue = new List<BulletInfo>();
        SealCheck(player.GetSFPosition());
        if (mode == -1) return rtnValue;
        if (shootable)
        {
            if (shotSealed[5] == -2 || player.GetHitBox().center.y < position.y)
            {
                List<SFPoint> directs = Fan(new SFPoint((sfloat)0, (sfloat) (-10)), 19 + lifeTime % 2, (sfloat)0.1);
                foreach (SFPoint dir in directs)
                {
                    rtnValue.Add(new BulletInfo(
                                     new Bullet(BulletPack.BltColor.Pink, BulletPack.BltShape.Spark, position + shotSealPoints[5], dir)
                    ));
                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[5], FlashType.Circle24px, (sfloat)0));
                }
            }

            shotCounter++;
            switch (mode)
            {
                case 0:
                    shotSealed[2] = -1;
                    if (shotCounter >= startShootingAt + 374)
                    {
                        if (shotSealed[0] < 0) shotSealed[0] = 0;
                        if (shotSealed[1] < 0) shotSealed[1] = 0;
                        if (shotSealed[2] < 0) shotSealed[2] = 0;
                        if (shotSealed[3] < 0) shotSealed[3] = 0;
                        if (shotSealed[4] < 0) shotSealed[4] = 0;
                        shotCounter = 0;
                        mode++;
                        mode %= 3;
                        directionCache = SFPoint.ZeroSFPoint;
                        break;
                    }

                    //center shot
                    if (shotCounter >= startShootingAt && (shotCounter - startShootingAt) % 5 == 0)
                    {
                        SFPoint bulletVel = new SFPoint((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5, (sfloat)0);
                        bulletVel = bulletVel.Rotated((sfloat)shotCounter / (sfloat)11);
                        rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[2], bulletVel, 0)
                            ));
                        bulletVel.x *= (sfloat)(-1);
                        rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Strobe16px, position + shotSealPoints[2], bulletVel, 0)
                            ));
                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[2], FlashType.Circle16px, (sfloat)0));
                    }

                    //3ways
                    if (shotCounter >= startShootingAt + 120 && (shotCounter - startShootingAt - 120) % 5 == 0)
                    {
                        List<SFPoint> directs;
                        switch ((shotCounter - startShootingAt - 120) / 5 % 18)
                        {
                            case 0:
                                directionCache = (player.GetSFPosition() - (position + shotSealPoints[0])).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)3);
                                if (shotSealed[0] > 0)
                                {
                                    if (shotSealed[1] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 7)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 7)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                }
                                break;
                            case 2:
                                if (shotSealed[0] > 0)
                                {
                                    if (shotSealed[1] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.125) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 6)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.125) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 6)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                }

                                
                                break;
                            case 4:
                                if (shotSealed[0] > 0)
                                {
                                    if (shotSealed[1] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.15) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 5)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.15) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 5)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                }
                                break;
                            case 9:
                                directionCache = (player.GetSFPosition() - (position + shotSealPoints[1])).Normalized() * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2.5);
                                if (shotSealed[1] > 0)
                                {
                                    if (shotSealed[0] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 7)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 7)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                }
                                break;
                            case 11:
                                if (shotSealed[1] > 0)
                                {
                                    if (shotSealed[0] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.125) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 6)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.125) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 6)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                }
                                break;
                            case 13:
                                if (shotSealed[1] > 0)
                                {
                                    if (shotSealed[0] > 0)
                                    {
                                        gManager.rank += 3000;
                                        gManager.rank = Math.Min(gManager.rank, 999999);
                                    }
                                    else
                                    {
                                        directs = Fan(directionCache, 3, (sfloat)(0.15) * sfloat.FromRaw(0x40490fdb));
                                        foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                            new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[0], shootTo, 5)
                                        ));
                                        bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle16px, (sfloat)0));
                                    }
                                }
                                else
                                {
                                    directs = Fan(directionCache, 3, (sfloat)(0.15) * sfloat.FromRaw(0x40490fdb));
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                        new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[1], shootTo, 5)
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle16px, (sfloat)0));
                                }
                                break;
                        }

                    }
                    break;


                case 1:
                    if (shotCounter >= startShootingAt + 120 + (spawnRank * 3 / 50000))
                    {
                        if (shotSealed[0] < 0) shotSealed[0] = 0;
                        if (shotSealed[1] < 0) shotSealed[1] = 0;
                        if (shotSealed[2] < 0) shotSealed[2] = 0;
                        if (shotSealed[3] < 0) shotSealed[3] = 0;
                        if (shotSealed[4] < 0) shotSealed[4] = 0;
                        shotCounter = 0;
                        mode++;
                        mode %= 3;
                        directionCache = SFPoint.ZeroSFPoint;
                        break;
                    }
                    if (shotCounter >= startShootingAt + 60)
                    {
                        if (shotCounter % 2 == 0)
                        {
                            if (shotSealed[0] > 0 && shotSealed[1] > 0)
                            {
                                directionCache = (player.GetSFPosition() - (position + shotSealPoints[5])).Normalized() * (sfloat)20;
                                List<SFPoint> directs = Fan(directionCache, 7, (sfloat)(0.05) * sfloat.FromRaw(0x40490fdb));
                                directs.Reverse();
                                foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                          new Bullet(BulletPack.BltColor.Pink, BulletPack.BltShape.Spark, position + shotSealPoints[5], shootTo),
                                          true
                                    ));
                                bManager.CreateFlash(new FlashFX(position + shotSealPoints[5], FlashType.Circle24px, (sfloat)0));
                            }
                            else
                            {
                                if (shotSealed[0] <= -1)
                                {
                                    List<SFPoint> directs = Fan(directionCache, 3, (sfloat)(0.01) * sfloat.FromRaw(0x40490fdb));
                                    directs.Reverse();
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                          new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.LargeRugby, position + shotSealPoints[0], shootTo),
                                          true
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[0], FlashType.Circle24px, (sfloat)0));
                                }
                                if (shotSealed[1] <= -1)
                                {
                                    List<SFPoint> directs = Fan(directionCache, 3, (sfloat)(-0.01) * sfloat.FromRaw(0x40490fdb));
                                    directs.Reverse();
                                    foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                          new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.LargeRugby, position + shotSealPoints[1], shootTo),
                                          true
                                    ));
                                    bManager.CreateFlash(new FlashFX(position + shotSealPoints[1], FlashType.Circle24px, (sfloat)0));
                                }

                            }
                        }
                            
                    }
                    else if (shotCounter >= startShootingAt)
                    {
                        if (shotCounter % 2 == 0)
                        {
                            if (shotSealed[0] > 0 && shotSealed[1] > 0)
                            {
                                List<SFPoint> directs = Fan(directionCache, 7, (sfloat)(0.05) * sfloat.FromRaw(0x40490fdb));
                                foreach (SFPoint shootTo in directs)
                                    bManager.CreateExhaust(new ExhaustFX(FXColor.Orange, position + shotSealPoints[5], shootTo), true);
                            }
                            else
                            {
                                if (shotSealed[0] <= -1) bManager.CreateExhaust(new ExhaustFX(FXColor.Orange, position + shotSealPoints[0], directionCache), true);
                                if (shotSealed[1] <= -1) bManager.CreateExhaust(new ExhaustFX(FXColor.Orange, position + shotSealPoints[1], directionCache), true);
                            }
                        }
                    }
                    else
                    {
                        if (shotSealed[0] <= 0) shotSealed[0] = -1;
                        if (shotSealed[1] <= 0) shotSealed[1] = -1;
                        if (shotSealed[0] <= 0 && shotSealed[1] <= 0) directionCache = new SFPoint((sfloat)0, (sfloat)20);
                        else if (shotSealed[0] <= 0) directionCache = (player.GetSFPosition() - (position + shotSealPoints[0])).Normalized() * (sfloat)20;
                        else if (shotSealed[1] <= 0) directionCache = (player.GetSFPosition() - (position + shotSealPoints[1])).Normalized() * (sfloat)20;
                        else directionCache = (player.GetSFPosition() - (position + shotSealPoints[5])).Normalized() * (sfloat)20;
                    }
                    break;
                default:
                    shotSealed[3] = -1;
                    shotSealed[4] = -1;
                    //end time check
                    if (shotCounter >= startShootingAt + 480)
                    {
                        if (shotSealed[0] < 0) shotSealed[0] = 0;
                        if (shotSealed[1] < 0) shotSealed[1] = 0;
                        if (shotSealed[2] < 0) shotSealed[2] = 0;
                        if (shotSealed[3] < 0) shotSealed[3] = 0;
                        if (shotSealed[4] < 0) shotSealed[4] = 0;
                        shotCounter = -30;
                        mode++;
                        mode %= 3;
                        directionCache = SFPoint.ZeroSFPoint;
                        break;
                    }

                    //shoot aimed shots
                    if (shotCounter >= startShootingAt + 120 && (shotCounter - startShootingAt) % 60 == 0)
                    {
                        if (shotSealed[1] > 0)
                        {
                            gManager.rank += 3000;
                            gManager.rank = Math.Min(gManager.rank, 999999);
                        }
                        else
                        {
                            SFPoint shotDir = (player.GetSFPosition() - (position + shotSealPoints[2])).Normalized();
                            List<SFPoint> directs = Fan(shotDir * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)3), 5, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Whirl, position + shotSealPoints[2], shootTo, 1)
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[2], FlashType.Circle16px, (sfloat)0));
                            directs = Fan(shotDir * ((sfloat)spawnRank / (sfloat)400000 + (sfloat)2), 4, (sfloat)(0.1) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Blue, BulletPack.BltShape.Whirl, position + shotSealPoints[2], shootTo, 1)
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[2], FlashType.Circle16px, (sfloat)0));
                        }
                    }
                    //shoot fixed shots
                    if (shotCounter >= startShootingAt && (shotCounter - startShootingAt) % 30 == 0)
                    {
                        if ((shotCounter - startShootingAt) % 60 == 0)
                        {
                            List<SFPoint> directs = Fan(new SFPoint((sfloat)(-2.5), (sfloat)0), 50, (sfloat)(0.02) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[3], shootTo, 0)
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[3], FlashType.Circle16px, (sfloat)0));
                        }
                        else
                        {
                            List<SFPoint> directs = Fan(new SFPoint((sfloat)2.5, (sfloat)0), 50, (sfloat)(0.02) * sfloat.FromRaw(0x40490fdb));
                            foreach (SFPoint shootTo in directs) rtnValue.Add(new BulletInfo(
                                new Bullet(BulletPack.BltColor.Red, BulletPack.BltShape.Strobe16px, position + shotSealPoints[4], shootTo, 0)
                            ));
                            bManager.CreateFlash(new FlashFX(position + shotSealPoints[4], FlashType.Circle16px, (sfloat)0));
                        }
                    }
                    break;
            }
        }
        SealCheck(player.GetSFPosition());
        return rtnValue;
    }

    public override bool DamageCheck(GameManager gManager, PlayerController player)
    {
        damagedOnThisFrame = false;
        damagable = false;
        shootable = false;
        foreach (SFAABB nmeBox in hitboxGroup)
        {
            if (IntersectBoxVSBox(nmeBox.Offset(position), damagableArea))
            {
                damagable = true;
                break;
            }
        }
        foreach (SFAABB nmeBox in hitboxGroup)
        {
            if (IntersectBoxVSBox(nmeBox.Offset(position), bufferArea))
            {
                shootable = true;
                break;
            }
        }
        int cap = 5;

        foreach (SFAABB nmeBox in hitboxGroup)
        {

            int dmg;
            foreach (PlayerShot child in player.shotList)
            {
                if (child.active && IntersectBoxVSBox(nmeBox.Offset(position), child.GetHitBox()))
                {
                    dmg = child.damage;
                    while (hp > 0 && dmg > 0)
                    {
                        dmg--;
                        if (damagable) {
                            damagedOnThisFrame = true;
                            hp--;
                        }
                        gManager.p1Score += 10u;
                    }
                    player.shotHitFX.Add(child.Hit());
                }
            }

            if (cap > 0)
                foreach (PlayerDeathShot child in player.deathShots)
                {
                    if (IntersectCircleVSBox(child.GetHitBox(), nmeBox.Offset(position)))
                    {
                        dmg = child.damage;
                        while (hp > 0 && dmg > 0 && cap > 0)
                        {
                            dmg--;
                            if (damagable) {
                                damagedOnThisFrame = true;
                                hp--;
                            }
                            gManager.p1Score += 20u;
                            cap--;
                        }
                        if (cap <= 0) break;
                    }
                }
            {
                Bomber child = player.bomberObject;
                if (child.active && IntersectCircleVSBox(child.GetHitBox(), nmeBox.Offset(position)))
                {
                    dmg = child.damage;
                    while (hp > 0 && dmg > 0)
                    {
                        dmg--;
                        if (damagable) {
                            damagedOnThisFrame = true;
                            hp--;
                        }
                        gManager.p1Score += 20u;
                    }
                }
            }
        }

        return hp < 1;
    }

    public override List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        rtnValue.Add(new DrawInfo(frames.GetFrame("Boss", 0), Vector2.Zero, 0, Vector2.One, damagedOnThisFrame ? new Color(0, 1, 1) : new Color(1, 1, 1)));
        rtnValue.AddRange(GetDrawSealPositions(frames));
        return rtnValue;
    }

    public override List<BulletInfo> Kill(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        int multi = 10;
        if (!player.GetFreeX10())
        {
            sfloat diff = player.GetHitBox().center.y - (position.y + multiplierYAxisOffset);
            bool brk = false;
            while (multi > 1 && !brk)
            {
                brk = diff <= (sfloat)(11 - multi) * (sfloat)10 + (sfloat)35;
                if (!brk) multi--;
            }
        }
        gManager.p1Score += (uint)(baseValue * multi);

        eManager.NewScoreFX(new ScoreFX(0, multi, position, SFPoint.ZeroSFPoint));

        gManager.medalDropCounter -= 10;
        if (multi == 10) gManager.medalDropCounter -= 10;
        if (gManager.waitingExtends > 0)
        {
            bManager.CreateItem(new Item(gManager, gManager.p1Lives > 0 ? ItemType.OneUp : ItemType.TwoUp, position));
            gManager.waitingExtends--;
        }
        else if (gManager.medalDropCounter <= 0)
        {
            bManager.CreateItem(new Item(gManager, (ItemType)gManager.currentMedal, position));
            gManager.medalDropCounter += 100;
        }

        bManager.CancelAll(true);
        gManager.timerOn = false;



        return RevengeShot(gManager, eManager, bManager, player);
    }

    public override List<ExplosionFX> Explosions()
    {
        List<SFPoint> offsets;
        SFPoint vel = SFPoint.ZeroSFPoint;
        sfloat randius;
        sfloat theta;
        int count = 55;
        sfloat decay;
        int timer;
        List<ExplosionFX> rtnValue = new List<ExplosionFX>();

        timer = 18;
        offsets = Round(new SFPoint((sfloat)10, (sfloat)0).Rotated((sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb)), 36);
        foreach (SFPoint direct in offsets)
        {
            rtnValue.Add(new ExplosionFX(position, direct, (sfloat)0.9, Math.Abs(timer) / 2, 2));
            timer--;
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
            randius = libm.sqrtf((sfloat)rng.RandiRange(1600, 2500));
            theta += (sfloat)rng.RandiRange(30, 120);
            theta %= (sfloat)360;
            vel.x = randius * libm.sinf(theta / (sfloat)360 * sfloat.FromRaw(0x40c90fdb));
            vel.y = randius * libm.cosf(theta / (sfloat)360 * sfloat.FromRaw(0x40c90fdb));
            offsets.Add(vel);
        }
        for (int i = 0; i < 8; i++)
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
            return rtnValue;
    }
}
