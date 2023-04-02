using Godot;
using System;
using System.Collections.Generic;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using BulletPack;



public class BulletManager : Node2D
{
    private GameManager gManager;
    private PlayerController player;
    public List<Bullet> bullets = new List<Bullet>();
    private List<FlashFX> flashFXs = new List<FlashFX>();
    private List<CancelFX> cancelFXs = new List<CancelFX>();
    private List<ExplosionFX> explfx = new List<ExplosionFX>();
    private List<ExhaustFX> exhaustFXs = new List<ExhaustFX>();
    public List<Item> items = new List<Item>();
    private readonly SpriteFrames bulletAnimations = GD.Load<SpriteFrames>("res://Sprites/Bullets/Bullet.tres");
    private readonly SpriteFrames fxAnimations = GD.Load<SpriteFrames>("res://Sprites/Effects/Cancel.tres");
    private readonly SpriteFrames otherFXAnimations = GD.Load<SpriteFrames>("res://Sprites/Effects/FX.tres");
    private readonly SpriteFrames itemAnimations = GD.Load<SpriteFrames>("res://Sprites/Items/Item.tres");

    public override void _Ready()
    {
        gManager = GetNode<GameManager>("/root/GameManager");
        player = GetNode<PlayerController>("/root/Main/Player");
    }

    public override void _PhysicsProcess(float delta)
    {
        Stack<int> cancelQueue = new Stack<int>();
        int index = 0;
        foreach (Bullet blt in bullets)
        {
            blt.Movement(this);
            if (!IntersectBoxVSPoint(new SFAABB(new SFPoint((sfloat)120, (sfloat)160), new SFPoint((sfloat)136 + blt.hitboxLength, (sfloat)176 + blt.hitboxLength)), blt.position) || blt.lifeTime >= blt.maxLifeTime)
            {
                Cancel(blt, blt.lifeTime >= blt.maxLifeTime, false);
                cancelQueue.Push(index);
            }
            else
            {
                bool bonk = false;
                if (blt.col == BltColor.Yellow)
                {
                    switch (blt.hitboxType)
                    {
                        case HitBoxType.Line:
                            SFPoint bltDelta = new SFPoint(blt.velocity);
                            bltDelta.Length(true);
                            bltDelta *= blt.hitboxLength;
                            foreach (PlayerShot child in player.shotList)
                            {
                                if (child.active && IntersectBoxVSSegment(child.GetHitBox(), blt.position, bltDelta))
                                {
                                    bonk = true;
                                    child.Hit();
                                }
                            }
                            break;
                        default:
                            foreach (PlayerShot child in player.shotList)
                            {
                                if (child.active && IntersectCircleVSBox(new SFCircle(blt.position, blt.hitboxLength), child.GetHitBox()))
                                {
                                    bonk = true;
                                    child.Hit();
                                }
                            }
                            break;
                    }
                }
                if (!bonk)
                    switch (blt.hitboxType)
                    {
                        case HitBoxType.Circle:
                            foreach (PlayerDeathShot child in player.deathShots)
                            {
                                if (IntersectCircleVSCircle(new SFCircle(blt.position, blt.hitboxLength), child.GetHitBox()))
                                {
                                    bonk = true;
                                    break;
                                }
                            }
                            if (player.bomberObject.active && IntersectCircleVSCircle(new SFCircle(blt.position, blt.hitboxLength), player.bomberObject.GetHitBox()))
                            {
                                bonk = true;
                            }
                            break;
                        case HitBoxType.Line:
                            SFPoint bltDelta = new SFPoint(blt.velocity);
                            bltDelta.Length(true);
                            bltDelta *= blt.hitboxLength;
                            foreach (PlayerDeathShot child in player.deathShots)
                            {
                                if (IntersectCircleVSSegment(child.GetHitBox(), blt.position, bltDelta))
                                {
                                    bonk = true;
                                    break;
                                }
                            }
                            if (player.bomberObject.active && IntersectCircleVSSegment(player.bomberObject.GetHitBox(), blt.position, bltDelta))
                            {
                                bonk = true;
                            }
                            break;
                        case HitBoxType.Pulse:
                            foreach (PlayerDeathShot child in player.deathShots)
                            {
                                if (IntersectCircleVSCircle(new SFCircle(blt.position + new SFPoint(blt.hitboxLength, (sfloat)0f), (sfloat)2), child.GetHitBox())
                                    || IntersectCircleVSCircle(new SFCircle(blt.position + new SFPoint(blt.hitboxLength * (sfloat)(-1f), (sfloat)0f), (sfloat)2), child.GetHitBox()))
                                {
                                    bonk = true;
                                    break;
                                }
                            }
                            if (player.bomberObject.active && (IntersectCircleVSCircle(new SFCircle(blt.position + new SFPoint(blt.hitboxLength, (sfloat)0f), (sfloat)2), player.bomberObject.GetHitBox())
                                    || IntersectCircleVSCircle(new SFCircle(blt.position + new SFPoint(blt.hitboxLength * (sfloat)(-1f), (sfloat)0f), (sfloat)2), player.bomberObject.GetHitBox())))
                            {
                                bonk = true;
                            }
                            break;
                    }
                if (bonk)
                {
                    Cancel(blt, true, false);
                    cancelQueue.Push(index);
                }
            }
            blt.lifeTime++;
            index++;
        }
        while (cancelQueue.Count != 0) bullets.RemoveAt(cancelQueue.Pop());

        index = 0;
        cancelQueue.Clear();
        foreach (CancelFX fx in cancelFXs)
        {
            fx.position += fx.velocity;
            fx.lifeTime++;
            if (fx.lifeTime >= 20) cancelQueue.Push(index);
            index++;
        }
        while (cancelQueue.Count != 0) cancelFXs.RemoveAt(cancelQueue.Pop());

        cancelQueue.Clear();
        index = 0;
        foreach (ExplosionFX fx in explfx)
        {
            fx.Process();
            if (fx.DelCheck()) cancelQueue.Push(index);
            fx.lifeTime++;
            index++;
        }
        while (cancelQueue.Count != 0) explfx.RemoveAt(cancelQueue.Pop());

        index = 0;
        cancelQueue.Clear();
        foreach (ExhaustFX fx in exhaustFXs)
        {
            fx.position += fx.velocity;
            fx.lifeTime++;
            if (fx.lifeTime >= 16) cancelQueue.Push(index);
            index++;
        }
        while (cancelQueue.Count != 0) exhaustFXs.RemoveAt(cancelQueue.Pop());

        index = 0;
        cancelQueue.Clear();
        foreach (FlashFX fx in flashFXs)
        {
            if (fx.done) cancelQueue.Push(index);
            index++;
        }
        while (cancelQueue.Count != 0) flashFXs.RemoveAt(cancelQueue.Pop());

        index = 0;
        cancelQueue.Clear();
        foreach (Item item in items)
        {
            if (item.Movement(player)) cancelQueue.Push(index);
            item.lifeTime++;
            index++;
        }
        while (cancelQueue.Count != 0) items.RemoveAt(cancelQueue.Pop());

        Update();
    }

    public override void _Draw()
    {
        foreach (ExplosionFX fx in explfx)
        {
            foreach (DrawInfo frame in fx.GetDrawFrames(otherFXAnimations))
            {
                DrawSetTransform((Vector2)fx.position, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2 + frame.drawOffset);
            }
        }
        foreach (CancelFX fx in cancelFXs)
        {
            Texture tex = fxAnimations.GetFrame(fx.col.ToString() + "_" + (fx.large ? "BigExplosion" : "Explosion"),
                                                fx.lifeTime % (fxAnimations.GetFrameCount(fx.col.ToString() + "_" + (fx.large ? "BigExplosion" : "Explosion")) * 4) / 4);
            DrawSetTransform((Vector2)fx.position, 0, Vector2.One);
            DrawTexture(tex, tex.GetSize() / -2, Color.ColorN("white", fx.lifeTime % 2));
        }
        foreach (ExhaustFX fx in exhaustFXs)
        {
            Texture tex = fxAnimations.GetFrame(fx.col.ToString() + "_Exhaust",
                                                fx.lifeTime % (fxAnimations.GetFrameCount(fx.col.ToString() + "_Exhaust") * 4) / 4);
            DrawSetTransform((Vector2)fx.position, 0, Vector2.One);
            DrawTexture(tex, tex.GetSize() / -2);
        }
        foreach (Item item in items)
        {
            foreach (DrawInfo frame in item.GetDrawFrames(itemAnimations))
            {
                DrawSetTransform((Vector2)item.itemBox.center, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2);
            }
        }
        foreach (Bullet blt in bullets)
        {
            foreach (DrawInfo frame in blt.GetDrawFrames(GetTree(), gManager, player, bulletAnimations))
            {
                DrawSetTransform((Vector2)blt.position, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2 + frame.drawOffset);
            }
        }
        foreach (FlashFX fx in flashFXs)
        {
            foreach (DrawInfo frame in fx.GetDrawFrames(fxAnimations))
            {
                DrawSetTransform((Vector2)fx.position, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.tex.GetSize() / -2 + frame.drawOffset);
            }
            fx.done = true;
        }
    }

    public void CreateBullet(Bullet blt, bool drawOnBottom = false)
    {
        int insertionPoint = blt.drawOrder;
        int index = 0;
        while (index < bullets.Count - 1)
        {
            if (drawOnBottom ? bullets[index].drawOrder >= insertionPoint : bullets[index].drawOrder > insertionPoint) break;
            index++;
        }
        bullets.Insert(index, blt);
    }

    public void CreateFlash(FlashFX fx)
    {
        flashFXs.Add(fx);
    }

    public void CreateExhaust(ExhaustFX fx, bool drawOnBottom = false)
    {
        exhaustFXs.Insert(0, fx);
    }

    public void CreateItem(Item item)
    {
        items.Add(item);
    }

    public void Cancel(Bullet blt, bool score, bool spawnItem)
    {
        if (score)
        {
            gManager.p1Score += 10u;
            gManager.rank += 10;
            gManager.rank = Math.Min(gManager.rank, 999999);
        }
        if (blt.col == BltColor.Yellow) gManager.medalDropCounter--;
        if (spawnItem)
        {
            CreateItem(new Item(gManager, ItemType.Gem, blt.position));
        }
        else
        {
            if (blt.col == BltColor.Yellow && blt.shp == BltShape.Missile)
            {
                RandomNumberGenerator rng = new RandomNumberGenerator
                {
                    Seed = (ulong)(gManager.p1Score + gManager.rank)
                };
                SFPoint vel = SFPoint.ZeroSFPoint;
                SFPoint off = blt.velocity.Normalized() * (sfloat)8;
                sfloat randius;
                sfloat theta;
                sfloat count = (sfloat)1.5;
                sfloat decay;
                int timer;
                List<ExplosionFX> explList = new List<ExplosionFX>();

                for (int i = 0; i < 15; i++)
                {
                    randius = (sfloat)rng.RandiRange(1, (int)libm.ceilf(count));
                    theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
                    vel.x = randius * libm.sinf(theta);
                    vel.y = randius * libm.cosf(theta);
                    decay = (sfloat)rng.Randi() / (sfloat)uint.MaxValue / (sfloat)10 + (sfloat)0.8f;
                    timer = rng.RandiRange(0, 16);

                    explList.Add(new ExplosionFX(blt.position + off, vel, decay, timer));
                    //count -= (sfloat)0.05f;
                }
                foreach (ExplosionFX fx in explList) explfx.Add(fx);
            }
            else
            {
                sfloat rad = libm.atan2f(blt.velocity.y, blt.velocity.x);
                SFPoint offset = new SFPoint(blt.drawOffset.x * libm.cosf(rad) - blt.drawOffset.y * libm.sinf(rad), blt.drawOffset.x * libm.sinf(rad) + blt.drawOffset.y * libm.cosf(rad));
                cancelFXs.Add(new CancelFX((FXColor)blt.col, blt.position + offset, blt.velocity, blt.shp == BltShape.LargeRugby || blt.shp == BltShape.Pulse || blt.shp == BltShape.SingleBeam));
            }
                
        }
    }

    public int CancelAll(bool spawnItem)
    {
        int rtnValue = bullets.Count;
        foreach (Bullet blt in bullets)
        {
            if (blt.col == BltColor.Yellow) gManager.medalDropCounter--;
            if (spawnItem)
            {
                CreateItem(new Item(gManager, ItemType.Gem, blt.position));
            }
            else
            {
                if (blt.col == BltColor.Yellow && blt.shp == BltShape.Missile)
                {
                    RandomNumberGenerator rng = new RandomNumberGenerator
                    {
                        Seed = (ulong)(gManager.p1Score + gManager.rank)
                    };
                    SFPoint vel = SFPoint.ZeroSFPoint;
                    SFPoint off = blt.velocity.Normalized() * (sfloat)8;
                    sfloat randius;
                    sfloat theta;
                    sfloat count = (sfloat)1.5;
                    sfloat decay;
                    int timer;
                    List<ExplosionFX> explList = new List<ExplosionFX>();

                    for (int i = 0; i < 15; i++)
                    {
                        randius = (sfloat)rng.RandiRange(1, (int)libm.ceilf(count));
                        theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
                        vel.x = randius * libm.sinf(theta);
                        vel.y = randius * libm.cosf(theta);
                        decay = (sfloat)rng.Randi() / (sfloat)uint.MaxValue / (sfloat)10 + (sfloat)0.8f;
                        timer = rng.RandiRange(0, 16);

                        explList.Add(new ExplosionFX(blt.position + off, vel, decay, timer));
                        //count -= (sfloat)0.05f;
                    }
                    foreach (ExplosionFX fx in explList) explfx.Add(fx);
                }
                else
                    cancelFXs.Add(new CancelFX((FXColor)blt.col, blt.position, blt.velocity, blt.shp == BltShape.LargeRugby || blt.shp == BltShape.Pulse));
            }
        }
        bullets.Clear();
        return rtnValue;
    }
}
