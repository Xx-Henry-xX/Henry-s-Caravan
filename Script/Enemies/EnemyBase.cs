using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;
using BulletPack;

public abstract class EnemyBase
{
    public SFAABB[] hitboxGroup;
    public SFPoint position;
    public SFPoint movement;
    public int hp;
    public int baseValue;
    public SFPoint[] shotSealPoints;
    public int[] shotSealed;
    public int lifeTime = -1;
    public int leaveTimer = 60;
    public int spawnRank;
    public int drawOrder = 0;

    protected RandomNumberGenerator rng;
    protected sfloat multiplierYAxisOffset;

    protected readonly SFAABB bufferArea = new SFAABB(new SFPoint((sfloat)120, (sfloat)128), new SFPoint((sfloat)96, (sfloat)112));
    protected readonly SFAABB damagableArea = new SFAABB(new SFPoint((sfloat)120, (sfloat)160), new SFPoint((sfloat)96, (sfloat)128));
    protected readonly SFAABB leaveArea = new SFAABB(new SFPoint((sfloat)120, (sfloat)160), new SFPoint((sfloat)136, (sfloat)176));
    protected bool damagable = false;
    protected bool shootable = false;
    protected bool damagedOnThisFrame = false;

    protected EnemyBase(GameManager gManager, int drawOrder = 0)
    {
        rng = new RandomNumberGenerator
        {
            Seed = (ulong)(gManager.p1Score + gManager.rank)
        };
        spawnRank = gManager.rank;
        this.drawOrder = drawOrder;
    }

    public virtual bool DamageCheck(GameManager gManager, PlayerController player)
    {
        damagedOnThisFrame = false;
        damagable = false;
        shootable = false;
        foreach (SFAABB nmeBox in hitboxGroup)
        {
            if (IntersectBoxVSBox(nmeBox.Offset(position), damagableArea)) {
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
                        if (damagable)
                        {
                            damagedOnThisFrame = true;
                            hp--;
                        }
                        gManager.p1Score += 10u;
                    }
                    player.shotHitFX.Add(child.Hit());
                }
            }

            foreach (PlayerDeathShot child in player.deathShots)
            {
                if (IntersectCircleVSBox(child.GetHitBox(), nmeBox.Offset(position)))
                {
                    dmg = child.damage;
                    while (hp > 0 && dmg > 0)
                    {
                        dmg--;
                        if (damagable)
                        {
                            damagedOnThisFrame = true;
                            hp--;
                        }
                        gManager.p1Score += 20u;
                    }
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
                        if (damagable)
                        {
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

    public virtual void Movement(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        return;
    }

    public virtual List<BulletInfo> Shoot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        return new List<BulletInfo>();
    }

    public virtual List<BulletInfo> RevengeShot(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
    {
        return new List<BulletInfo>();
    }


    public virtual bool LeaveCheck()
    {
        leaveTimer--;
        foreach (SFAABB nmeBox in hitboxGroup) if (IntersectBoxVSBox(leaveArea, nmeBox.Offset(position))) return false;
        return leaveTimer <= 0;
    }

    public virtual List<BulletInfo> Kill(GameManager gManager, EnemyManager eManager, BulletManager bManager, PlayerController player)
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
        return RevengeShot(gManager, eManager, bManager, player);
    }

    public virtual List<ExplosionFX> Explosions()
    {
        return new List<ExplosionFX>();
    }

    protected void SealCheck(SFPoint playerPos)
    {
        for (int i = 0; i < shotSealPoints.Length; i++)
        {
            if (shotSealed[i] < 0)
            {
                shotSealed[i] = ((shotSealPoints[i] + position - playerPos).LengthSquared() < (sfloat)3600 ? -2 : -1);
                continue;
            }
            if (shotSealed[i] > 0) shotSealed[i]--;
            shotSealed[i] = ((shotSealPoints[i] + position - playerPos).LengthSquared() < (sfloat)3600 ? 15 : shotSealed[i]);
        }
    }

    public virtual List<DrawInfo> GetDrawFrames(GameManager gManager, PlayerController player, SpriteFrames frames)
    {
        return new List<DrawInfo>();
    }

    protected List<DrawInfo> GetDrawSealPositions(SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        int index = 0;
        foreach (SFPoint point in shotSealPoints)
        {
            if (shotSealed[index] > 0 && shotSealed[index] % 2 == 1) rtnValue.Add(new DrawInfo(frames.GetFrame("SealIcon", 0), (Vector2)point, 0, Vector2.One));
            if (shotSealed[index] == -2) rtnValue.Add(new DrawInfo(frames.GetFrame("SealIcon", 3), (Vector2)point, 0, Vector2.One));
            index++;
        }
        return rtnValue;
    }
}

public class ExplosionFX 
{
    public SFPoint position;
    public SFPoint velocity;
    public sfloat decayRate;
    public int type;
    public int lifeTime;
    public int hiddenTimer;

    public ExplosionFX(SFPoint position, SFPoint velocity, sfloat decayRate, int delay, int type = 0, int hiddenTimer = 0)
    {
        this.position = position;
        this.velocity = velocity;
        this.decayRate = decayRate;
        this.hiddenTimer = hiddenTimer;
        this.type = type;
        lifeTime = -delay - hiddenTimer;
    }
    public void Process()
    {
        if (hiddenTimer > 0) hiddenTimer--;
        else
        {
            position += velocity;
            velocity *= decayRate;
        }
    }

    public DrawInfo[] GetDrawFrames(SpriteFrames frames)
    {
        if (hiddenTimer > 0) return new DrawInfo[0];

        string name = type switch
        {
            0 => "Explosion16px",
            1 => "Explosion32px",
            2 => "Smoke",
            _ => "",
        };

        DrawInfo[] rtnValue = new DrawInfo[1]
        {
            new DrawInfo(frames.GetFrame(name, Math.Max(0, lifeTime) % (frames.GetFrameCount(name) * 2) / 2), Vector2.Zero, 0, Vector2.One)
        };
        return rtnValue;
    }

    public bool DelCheck()
    {
        return type switch
        {
            0 => lifeTime >= 15,
            1 => lifeTime >= 15,
            2 => lifeTime >= 15,
            _ => lifeTime >= 0,
        };
    }
}

//todo: redo
public class DeadEnemyFX
{
    public SFPoint position;
    public SFPoint velocity;
    public sfloat decayRate;
    public string name;
    public int lifeTime;
    public int redTimer;

    public DeadEnemyFX(SFPoint position, SFPoint velocity, sfloat decayRate, string name, int delay, int redTimer = 0)
    {
        this.position = position;
        this.velocity = velocity;
        this.decayRate = decayRate;
        this.redTimer = redTimer;
        this.name = name;
        lifeTime = -delay - redTimer;
    }
    public void Process()
    {
        position += velocity;
        velocity *= decayRate;
    }

    public DrawInfo[] GetDrawFrames(SpriteFrames frames)
    {
        DrawInfo[] rtnValue = new DrawInfo[1]
        {
            new DrawInfo(frames.GetFrame(name, Math.Max(0, lifeTime) % (frames.GetFrameCount(name) * 2) / 2), Vector2.Zero, 0, Vector2.One)
        };
        return rtnValue;
    }

    public bool DelCheck()
    {
        return lifeTime >= 0;
    }
}

public class ScoreFX
{
    public int type = 0;
    public int lifeTime = 0;
    public SFPoint pos;
    public SFPoint vel;

    /*
     * type and what value does for each type
     * 0 = multiplier fx       multiplier number (vel becomes strech factor here)
     * 1 = medal score         medal value order
     */

    public ScoreFX(int type, int value, SFPoint pos, SFPoint vel)
    {
        this.type = type switch
        {
            0 => value * -1,
            _ => value,
        };
        this.pos = pos;
        this.vel = vel;
    }

    public bool Process()
    {
        if (type < 0)
        {
            vel = lifeTime switch
            {
                int n when (n < 5) => new SFPoint((sfloat)n / (sfloat)5, (sfloat)2 - ((sfloat)n / (sfloat)5)),
                int n when (n > 34) => new SFPoint((sfloat)1 + ((sfloat)(n - 34) / (sfloat)10), (sfloat)(40 - n) / (sfloat)5),
                _ => new SFPoint((sfloat)1, (sfloat)1),
            };
            if (lifeTime > 40) return true;
            return false;
        }
        if (lifeTime > 60) return true;
        return false;
    }

    public DrawInfo[] GetDrawFrames(SpriteFrames frames)
    {
        string name;
        int frame;
        switch (type)
        {
            case -10:
                name = "Multiplier10";
                frame = lifeTime / 4 % 4;
                break;
            case int n when (n < 0):
                name = "Multiplier";
                frame = type * -1;
                break;
            default:
                name = "MedalScore";
                frame = type;
                break;
        }

        DrawInfo[] rtnValue = new DrawInfo[1]
        {
            new DrawInfo(frames.GetFrame(name, frame), Vector2.Zero, 0, type < 0 ? (Vector2)vel : Vector2.One)
        };
        return rtnValue;
    }

    public bool DelCheck()
    {
        return type switch
        {
            int n when (n < 0) => lifeTime >= 40,
            _ => lifeTime >= 60,
        };
    }
}

