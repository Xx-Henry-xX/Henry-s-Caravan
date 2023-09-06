using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using BulletPack;
using System.Collections.Generic;

public class PlayerController : Node2D
{
    /*
	 * reminder when using SFPoint for position
	 * (0, 0) is upper left of the screen
	 * down is positive y axis
	 */

    public sfloat speed = (sfloat)6;
    public sfloat focusSpeed = (sfloat)3;
    public int firerate = 1;
    public SFPoint startPos = new SFPoint((sfloat)120, (sfloat)240);

    public readonly SpriteFrames playerAnimations = GD.Load<SpriteFrames>("res://Sprites/Player/Player.tres");



    private GameManager gManager;
    private BulletManager bManager;
    private EnemyManager eManager;
    private AudioManager aManager;


    private SFPoint direction = SFPoint.ZeroSFPoint;
	private int tiltValue = 2;

	private int inputState = 0;
	//up = 1		(inputState >> 0) & 1
	//down = 2		(inputState >> 1) & 1
	//left = 4		(inputState >> 2) & 1
	//right = 8		(inputState >> 3) & 1
	//shot = 16		(inputState >> 4) & 1
	//focus = 32	(inputState >> 5) & 1
	//bomb = 64		(inputState >> 6) & 1

    public PlayerShot[,] shotList = {
                                        { new PlayerShot(), new PlayerShot(), new PlayerShot(), new PlayerShot() },
                                        { new PlayerShot(), new PlayerShot(), new PlayerShot(), new PlayerShot() },
                                        { new PlayerShot(), new PlayerShot(), new PlayerShot(), new PlayerShot() },
                                        { new PlayerShot(), new PlayerShot(), new PlayerShot(), new PlayerShot() },
                                    };

    public List<PlayerFX> shotHitFX = new List<PlayerFX>();
    private int shotDelay = 0;
    public List<PlayerDeathShot> deathShots = new List<PlayerDeathShot>();
    public Bomber bomberObject = new Bomber();
    
    public int ang = 0;
    private bool flashbool = false;
    private PlayerFX deathFX = null;

	public int iframe = 0;

	private SFAABB playerBox;
	private sfloat boxOffset;
	public override void _Ready()
	{
        gManager = GetNode<GameManager>("/root/GameManager");
        bManager = GetNode<BulletManager>("/root/Main/BulletManager");
        eManager = GetNode<EnemyManager>("/root/Main/EnemyManager");
        aManager = GetNode<AudioManager>("/root/AudioManager");

        playerBox = new SFAABB(startPos, new SFPoint((sfloat)2, (sfloat)2));
    }
	public override void _Process(float delta)
    {
		if (true) //replay mode check goes here
        {
			inputState = 0;
			if (Input.IsActionPressed("move_up")) inputState += 1;
			if (Input.IsActionPressed("move_down")) inputState += 2;
			if (Input.IsActionPressed("move_left")) inputState += 4;
			if (Input.IsActionPressed("move_right")) inputState += 8;
			if (Input.IsActionPressed("shot")) inputState += 16;
			if (Input.IsActionPressed("focus")) inputState += 32;
			if (Input.IsActionPressed("bomb")) inputState += 64;
		}
        flashbool = !flashbool;
    }
	public override void _PhysicsProcess(float delta)
	{
		if (true)  //replay mode check goes here
		{
			//replay recording script goes here
		}
		else
		{
			//replay playback script goes here
		}

        direction = new SFPoint((sfloat)(((inputState >> 3) & 1) - ((inputState >> 2) & 1)), (sfloat)(((inputState >> 1) & 1) - ((inputState >> 0) & 1))).Normalized();
        
        if (((inputState >> 5) & 1) == 1)
		{
			direction *= focusSpeed;
		}
		else
		{
			direction *= speed;
		}

        switch (ang)
        {
            case 0:
                Movement(false);
                ItemCheck();
                Shoot();
                DamageCheck();
                if (iframe > 0) iframe--;
                break;
            case 1:
                direction.x = direction.x == (sfloat)0 ? (sfloat)0 : libm.sqrtf( (((inputState >> 5) & 1) == 1) ? focusSpeed * focusSpeed - (sfloat)2.56f : speed * speed - (sfloat)2.56f ) * (sfloat)direction.x.Sign();
                direction.y = (sfloat)(-1.6f);
                Movement(true);
                ItemCheck();
                Shoot();
                DamageCheck();
                if (playerBox.center.y <= startPos.y - (sfloat)2)
                {
                    ang = 0;
                    iframe = 120;
                }
                break;
            case 2:
                playerBox.center.y = (sfloat)(1972);
                if (deathShots.Count == 0)
                {
                    if (gManager.p1Lives == 0)
                    {
                        gManager.GameOver();
                        gManager.p1Lives = -1;
                    }
                    else if (gManager.p1Lives > 0)
                    {
                        ang = 1;
                        gManager.p1Lives--;
                        gManager.p1Bombs += 3;
                        gManager.p1Bombs = Mathf.Clamp(gManager.p1Bombs, 0, 5);
                        playerBox.center.y = (sfloat)(336);
                    }
                }
                break;
            case 3:
                tiltValue = 0;
                Shoot(true);
                if (iframe < 0) iframe++;
                if (iframe == 0)
                {
                    gManager.rank -= 100000;
                    gManager.rank = Math.Max(gManager.rank, 0);
                    for (sfloat i = (sfloat)0; i < (sfloat)360; i += (sfloat)20)
                    {
                        deathShots.Add(new PlayerDeathShot((sfloat)4, i, playerBox.center));
                    }
                    for (sfloat i = (sfloat)10; i < (sfloat)360; i += (sfloat)20)
                    {
                        deathShots.Add(new PlayerDeathShot((sfloat)4.5, i, playerBox.center));
                    }
                    for (sfloat i = (sfloat)0; i < (sfloat)360; i += (sfloat)20)
                    {
                        deathShots.Add(new PlayerDeathShot((sfloat)5, i, playerBox.center));
                    }
                    for (sfloat i = (sfloat)10; i < (sfloat)360; i += (sfloat)20)
                    {
                        deathShots.Add(new PlayerDeathShot((sfloat)5.5, i, playerBox.center));
                    }
                    for (sfloat i = (sfloat)0; i < (sfloat)360; i += (sfloat)20)
                    {
                        deathShots.Add(new PlayerDeathShot((sfloat)6, i, playerBox.center));
                    }
                    deathFX = new PlayerFX(3, playerBox.center, SFPoint.ZeroSFPoint);

                    ang = 2;
                    aManager.Play("ded", 2, 2);
                }
                if (iframe > 0) ang = 0;
                break;
        }

        foreach (PlayerShot shot in shotList)
        {
            if (shot.active) shot.Process();
        }
        Stack<int> cancelQueue = new Stack<int>();
        int counter = 0;
        foreach (PlayerDeathShot dshot in deathShots)
        {
            if (dshot.Process()) cancelQueue.Push(counter);
            counter++;
        }
        while (cancelQueue.Count != 0) deathShots.RemoveAt(cancelQueue.Pop());
        bomberObject.Process(this, aManager);

        counter = 0;
        foreach (PlayerFX fx in shotHitFX)
        {
            if (fx.Process()) cancelQueue.Push(counter);
            counter++;
        }
        while (cancelQueue.Count != 0) shotHitFX.RemoveAt(cancelQueue.Pop());
        if (deathFX != null)
            if (deathFX.Process()) deathFX = null;

        Update();
    }

    public override void _Draw()
    {
        foreach (PlayerFX fx in shotHitFX)
        {
            DrawInfo frame = fx.GetDrawFrames(playerAnimations);
            DrawSetTransform((Vector2)fx.pos, frame.angle, frame.scale);
            DrawTexture(frame.tex, frame.drawOffset - frame.tex.GetSize() / 2);
        }

        foreach (PlayerShot shot in shotList)
        {
            if (shot.active)
            {
                DrawInfo frame = shot.GetDrawFrames(playerAnimations);
                DrawSetTransform((Vector2)shot.GetHitBox().center, frame.angle, frame.scale);
                DrawTexture(frame.tex, frame.drawOffset - frame.tex.GetSize() / 2);
            }
        }

        foreach (PlayerDeathShot dshot in deathShots)
        {
            DrawInfo frame = dshot.GetDrawFrames(playerAnimations);
            DrawSetTransform((Vector2)dshot.GetHitBox().center, frame.angle, frame.scale);
            DrawTexture(frame.tex, frame.drawOffset - frame.tex.GetSize() / 2);
        }

        if (deathFX != null)
        {
            DrawInfo frame = deathFX.GetDrawFrames(playerAnimations);
            DrawSetTransform((Vector2)deathFX.pos, frame.angle, frame.scale);
            DrawTexture(frame.tex, frame.drawOffset - frame.tex.GetSize() / 2);
        }

        if (ang != 2)
        {
            int playerframe = tiltValue switch
            {
                int n when (n <= -8) => 0 + (ang == 3 ? 10 : (flashbool && (ang == 1 || iframe > 0) ? 5 : 0)),
                int n when (n <= -3 && n > -8) => 1 + (ang == 3 ? 10 : (flashbool && (ang == 1 || iframe > 0) ? 5 : 0)),
                int n when (n >= 8) => 4 + (ang == 3 ? 10 : (flashbool && (ang == 1 || iframe > 0) ? 5 : 0)),
                int n when (n >= 3 && n < 8) => 3 + (ang == 3 ? 10 : (flashbool && (ang == 1 || iframe > 0) ? 5 : 0)),
                _ => 2 + (ang == 3 ? 10 : (flashbool && (ang == 1 || iframe > 0) ? 5 : 0)),
            };
            DrawInfo playerFrame = new DrawInfo(playerAnimations.GetFrame("Player", playerframe), Vector2.Zero, 0, Vector2.One);
            DrawSetTransform((Vector2)playerBox.center, playerFrame.angle, playerFrame.scale);
            DrawTexture(playerFrame.tex, playerFrame.drawOffset - playerFrame.tex.GetSize() / 2);

            if (((inputState >> 5) & 1) == 1 && ang < 2)
            {
                playerFrame = new DrawInfo(playerAnimations.GetFrame("HitboxOverlay", playerframe % 5), Vector2.Zero, 0, Vector2.One);
                DrawSetTransform((Vector2)playerBox.center, playerFrame.angle, playerFrame.scale);
                DrawTexture(playerFrame.tex, playerFrame.drawOffset - playerFrame.tex.GetSize() / 2);
            }
        }
    }

    public SFPoint GetSFPosition()
    {
        return playerBox.center;
    }

    public SFAABB GetHitBox()
    {
        return new SFAABB(new SFPoint(playerBox.center.x + boxOffset, playerBox.center.y), playerBox.halfLength);
    }

    public bool GetBombHeld()
    {
        return ((inputState >> 6) & 1) == 1;
    }

    public bool GetFreeX10()
    {
        if (ang == 0 && iframe <= 0) return false;
        return true;
    }

    private void Movement(bool ignoreY)
    {
        playerBox.center += direction;
        playerBox.center.x = Clamp(playerBox.center.x, (sfloat)16, (sfloat)224);
        switch (!ignoreY ? ang : -1)
        {
            case 0:
                playerBox.center.y = Clamp(playerBox.center.y, (sfloat)48, (sfloat)296);
                break;
            case 1:
                playerBox.center.y = sfloat.Max(playerBox.center.y, (sfloat)48);
                break;
        }   

        tiltValue += (((inputState >> 3) & 1) - ((inputState >> 2) & 1) == 0) ? Math.Sign(tiltValue) * -1 : ((inputState >> 3) & 1) - ((inputState >> 2) & 1);

        tiltValue = Mathf.Clamp(tiltValue, -12, 12);
        switch (tiltValue)
        {
            case int n when (n <= -8):
                playerBox.halfLength = new SFPoint((sfloat)1, (sfloat)2);
                boxOffset = (sfloat)(-1);
                break;
            case int n when (n <= -3 && n > -8):
                playerBox.halfLength = new SFPoint((sfloat)1.5, (sfloat)2);
                boxOffset = (sfloat)(-0.5);
                break;
            case int n when (n >= 8):
                playerBox.halfLength = new SFPoint((sfloat)1, (sfloat)2);
                boxOffset = (sfloat)(1);
                break;
            case int n when (n >= 3 && n < 8):
                playerBox.halfLength = new SFPoint((sfloat)1.5, (sfloat)2);
                boxOffset = (sfloat)(0.5);
                break;
            default:
                playerBox.halfLength = new SFPoint((sfloat)2, (sfloat)2);
                boxOffset = (sfloat)(0);
                break;
        }
    }

    private void Shoot(bool bombOnly = false)
    {
        if (shotDelay <= 0 && ((inputState >> 4) & 1) == 1 && !bombOnly)
        {
            shotDelay = firerate;
            for (int dir = 0; dir < 4; dir++)
            {
                for (int i = 0; i < 4; i++)
                {
                    if (!shotList[dir, i].active)
                    {
                        gManager.rank += 6;
                        gManager.rank = Math.Min(gManager.rank, 999999);
                        shotList[dir, i].Setup(dir, playerBox.center);
                        break;
                    }
                }
            }
        }
        else shotDelay--;

        if (((inputState >> 6) & 1) == 1 && !bomberObject.active && gManager.p1Bombs > 0)
        {
            gManager.rank += 1000;
            gManager.rank = Math.Min(gManager.rank, 999999);
            bomberObject.Setup(playerBox.center, gManager, aManager);
            gManager.p1Bombs--;
            bManager.CancelAll(true);
        }
        if (bomberObject.active) iframe = 60;
    }

    private void ItemCheck()
    {
        List<Item> todel = new List<Item>();
        foreach (Item item in bManager.items)
        {
            if (IntersectBoxVSBox(GetHitBox(), item.itemBox))
            {
                switch ((int)item.type)
                {
                    case -4:
                        gManager.p1Score += 100;
                        gManager.rank += 100;
                        gManager.rank = Math.Min(gManager.rank, 999999);
                        
                        eManager.NewScoreFX(new ScoreFX(1, 0, item.itemBox.center, SFPoint.ZeroSFPoint));

                        aManager.Play("item_gem", 1);

                        break;
                    case -3:
                        gManager.p1Bombs++;
                        if (gManager.p1Bombs > 5)
                        {
                            gManager.p1Bombs--;
                            gManager.p1Score += 5000;
                            eManager.NewScoreFX(new ScoreFX(1, 13, item.itemBox.center, SFPoint.ZeroSFPoint));
                            gManager.rank -= 20000;
                            gManager.rank = Math.Max(gManager.rank, 0);
                            aManager.Play("item_medal10k", 1, 2);
                        }
                        else
                        {
                            gManager.rank += 20000;
                            gManager.rank = Math.Min(gManager.rank, 999999);
                            aManager.Play("item_bomb", 1, 2);
                        }
                        break;
                    case -2:
                        gManager.p1Lives += 2;
                        gManager.rank += 125000;
                        gManager.rank = Math.Min(gManager.rank, 999999);
                        gManager.rankPerFrame += 7;
                        aManager.Play("item_2up", 1, 5);
                        break;
                    case -1:
                        gManager.p1Lives++;
                        gManager.rank += 50000;
                        gManager.rank = Math.Min(gManager.rank, 999999);
                        gManager.rankPerFrame += 3;
                        aManager.Play("item_1up", 1, 4);
                        break;
                    case int n when n >= 0 && n < 19:
                        
                        eManager.NewScoreFX(new ScoreFX(1, n, item.itemBox.center, SFPoint.ZeroSFPoint));
                        gManager.p1Score += gManager.medalScore[n];
                        gManager.rank += gManager.medalRank[n];
                        gManager.rank = Math.Min(gManager.rank, 999999);
                        gManager.currentMedal = Mathf.Clamp(n + 1, 0, 18);
                        gManager.rankPerFrame += 1;
                        aManager.Play(n == 18 ? "item_medal10k" : n >= 9 ? "item_medal1k" : "item_medal100", 1, 3);
                        break;
                }
                todel.Add(item);
            }
        }
        foreach (Item item in todel)
        {
            bManager.items.Remove(item);
        }
    }

    private void DamageCheck()
    {
        Stack<int> cancelQueue = new Stack<int>();
        int index = 0;
        foreach (Bullet blt in bManager.bullets)
        {
            switch (blt.hitboxType)
            {
                case BulletPack.HitBoxType.Circle:
                    if (IntersectCircleVSBox(new SFCircle(blt.position, blt.hitboxLength), GetHitBox()))
                    {
                        bManager.Cancel(blt, true, false);
                        cancelQueue.Push(index);
                    }
                    break;
                case BulletPack.HitBoxType.Line:
                    SFPoint dir = blt.velocity.Normalized() * blt.hitboxLength;
                    if (IntersectBoxVSSegment(GetHitBox(), blt.position, dir))
                    {
                        bManager.Cancel(blt, true, false);
                        cancelQueue.Push(index);
                    }
                    break;
                case BulletPack.HitBoxType.Pulse:
                    if (IntersectCircleVSBox(new SFCircle(blt.position + new SFPoint(blt.hitboxLength, (sfloat)0f), (sfloat)2), GetHitBox())
                     || IntersectCircleVSBox(new SFCircle(blt.position + new SFPoint(blt.hitboxLength * (sfloat)(-1f), (sfloat)0f), (sfloat)2), GetHitBox()))
                    {
                        bManager.Cancel(blt, true, false);
                        cancelQueue.Push(index);
                    }
                    break;
            }
            index++;
        }
        if (cancelQueue.Count != 0)
        {
            if (iframe <= 0 && ang == 0)
            {
                ang = 3;
                iframe = -15;
                foreach (Item item in bManager.items) item.Rebound();
            }
            while (cancelQueue.Count != 0) bManager.bullets.RemoveAt(cancelQueue.Pop());
        }
    }
}

public class PlayerShot
{
    public bool active = false;
    public int damage = 3;
    private int dir = 1;
    private readonly sfloat speed = (sfloat)16;
    private SFPoint movement = SFPoint.ZeroSFPoint;
    private SFAABB shotBox;

    public void Setup(int dir, SFPoint startPos)
    {
        active = true;
        this.dir = dir;
        shotBox = new SFAABB(startPos, new SFPoint((sfloat)8, (sfloat)9));
        switch (dir)
        {
            case 0:
                movement.x = speed * libm.sinf((sfloat)(0.05f) * sfloat.FromRaw(0x40490fdb)) * (sfloat)(-1); //no degToRad() or pi()? really??????
                movement.y = (sfloat)(-1) * speed * libm.cosf((sfloat)(0.05f) * sfloat.FromRaw(0x40490fdb));
                shotBox.center += new SFPoint((sfloat)(-6), (sfloat)0);
                break;
            case 1:
                movement.x = (sfloat)0;
                movement.y = (sfloat)(-1) * speed;
                shotBox.center += new SFPoint((sfloat)(-6), (sfloat)0);
                break;
            case 2:
                movement.x = (sfloat)0;
                movement.y = (sfloat)(-1) * speed;
                shotBox.center += new SFPoint((sfloat)6, (sfloat)0);
                break;
            case 3:
                movement.x = speed * libm.sinf((sfloat)(0.05f) * sfloat.FromRaw(0x40490fdb));
                movement.y = (sfloat)(-1) * speed * libm.cosf((sfloat)(0.05f) * sfloat.FromRaw(0x40490fdb));
                shotBox.center += new SFPoint((sfloat)6, (sfloat)0);
                break;
        }
    }
    
    public void Process()
    {
        shotBox.center += movement;
        if (shotBox.center.y > (sfloat)336 || shotBox.center.y < (sfloat)(-16) || shotBox.center.x > (sfloat)256 || shotBox.center.x < (sfloat)(-16)) active = false;
    }
    public SFAABB GetHitBox()
    {
        return shotBox;
    }

    public DrawInfo GetDrawFrames(SpriteFrames frames)
    {
        Texture tex = frames.GetFrame("Shot", dir);
        return new DrawInfo(tex, Vector2.Zero, 0, Vector2.One);
    }

    public PlayerFX Hit()
    {
        active = false;
        return new PlayerFX(0, shotBox.center, movement);
    }
}

public class Bomber
{
    public bool active = false;
    public int damage = 2;

    private SFCircle shotBox;
    private sfloat speed = (sfloat)1;
    private int lifetime = 0;
    private bool flying = true;
    private RandomNumberGenerator rng;

    private List<PlayerFX> fxs = new List<PlayerFX>();
    private SFPoint offset;

    public Bomber()
    {
        rng = new RandomNumberGenerator();
    }

    public void Setup(SFPoint startPos, GameManager gManager, AudioManager aManager)
    {
        active = true;
        shotBox = new SFCircle(startPos, (sfloat)32);
        rng.Seed = (ulong)(gManager.p1Score + gManager.rank);
        flying = true;
        aManager.Play("bombfired", 2, 1);
    }

    public void Process(PlayerController player, AudioManager aManager)
    {
        if (active)
        {
            if (flying)
            {
                shotBox.center.y -= (sfloat)8;
                if (!player.GetBombHeld() || shotBox.center.y < (sfloat)64)
                {
                    flying = false;
                    lifetime = 0;
                    shotBox.radius = (sfloat)80;
                    aManager.Play("bombexpl", 2, 1);
                }
            }
            else
            {
                switch (lifetime)
                {
                    case int n when (n < 110 && n >= 18):
                        offset = SFPoint.ZeroSFPoint;
                        sfloat randius;
                        sfloat theta;

                        randius = libm.sqrtf((sfloat)rng.RandiRange(0, 2500));
                        theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
                        offset.x = randius * libm.sinf(theta);
                        offset.y = randius * libm.cosf(theta);

                        fxs.Insert(0, new PlayerFX(2, offset, SFPoint.ZeroSFPoint));

                        SFPoint vel = SFPoint.ZeroSFPoint;

                        randius = libm.sqrtf((sfloat)rng.RandiRange(11025, 14400));
                        theta = (sfloat)rng.Randi() / (sfloat)int.MaxValue * sfloat.FromRaw(0x40c90fdb);
                        offset.x = randius * libm.sinf(theta);
                        offset.y = randius * libm.cosf(theta);
                        vel.x = libm.sinf(theta);
                        vel.y = libm.cosf(theta);
                        vel *= (sfloat)(-1);

                        fxs.Add(new PlayerFX(1, offset, vel));

                        randius = libm.sqrtf((sfloat)rng.RandiRange(0, 36));
                        theta = (sfloat)rng.Randi() / (sfloat)uint.MaxValue * sfloat.FromRaw(0x40c90fdb);
                        offset.x = randius * libm.sinf(theta);
                        offset.y = randius * libm.cosf(theta);

                        break;
                    case 120:
                        active = false;
                        break;
                }
            }
            lifetime++;
        }

        Stack<int> cancelQueue = new Stack<int>();
        int counter = 0;
        foreach (PlayerFX fx in fxs)
        {
            if (fx.Process()) cancelQueue.Push(counter);
            counter++;
        }
        while (cancelQueue.Count != 0) fxs.RemoveAt(cancelQueue.Pop());
    }

    public SFCircle GetHitBox()
    {
        return shotBox;
    }

    public List<DrawInfo> GetDrawFrames(SpriteFrames frames)
    {
        List<DrawInfo> rtnValue = new List<DrawInfo>();
        Texture tex;
        if (flying)
            tex = frames.GetFrame("Bomb_Shell", lifetime / 2 % 2);
        else
        {
            if (lifetime < 111) tex = frames.GetFrame("Bomb_In", Math.Min(lifetime / 2, 8));
            else tex = frames.GetFrame("Bomb_Out", Math.Min((lifetime - 111) / 2, 5));
        }

        rtnValue.Add(new DrawInfo(tex, (Vector2)offset, 0, Vector2.One));

        foreach (PlayerFX fx in fxs)
            rtnValue.Add(fx.GetDrawFrames(frames));

        return rtnValue;
    }
}

public class PlayerFX
{
    /*
     * type
     * 0 = shot hit
     * 1 = bomb zoom
     * 2 = bomb flash
     * 3 = death
     */

    readonly int type;
    int lifetime = -1;
    public SFPoint pos;
    SFPoint vel = SFPoint.ZeroSFPoint;

    public PlayerFX(int type, SFPoint pos, SFPoint vel)
    {
        this.type = type;
        this.pos = pos;
        this.vel = vel;
    }

    public bool Process()
    {
        lifetime++;
        if (type == 1) vel = vel.Normalized() * (sfloat)(14 - lifetime);
        pos += vel;
        switch (type)
        {
            case 0:
                if (lifetime >= 4) return true;
                break;
            case 3:
                if (lifetime > 25) return true;
                break;
            default:
                if (lifetime >= 15) return true;
                break;
        }
        return false;
    }

    public DrawInfo GetDrawFrames(SpriteFrames frames)
    {
        Texture tex = null;
        switch (type)
        {
            case 0:
                tex = frames.GetFrame("ShotHit", lifetime % 5);
                break;
            case 1:
                tex = frames.GetFrame("Bomb_Zoom", Math.Min(lifetime, 9));
                break;
            case 2:
                tex = frames.GetFrame("Bomb_Flash", lifetime / 3 % 5);
                break;
            case 3:
                tex = frames.GetFrame("Dead", Math.Min(lifetime / 2, 25));
                break;
        }
        return new DrawInfo(tex, type == 0 || type == 3 ? Vector2.Zero : (Vector2)pos, 0, Vector2.One);
    }
}

public class PlayerDeathShot
{
    public int damage = 1;
    private int lifetime = -1;
    private readonly sfloat speed = (sfloat)16;
    private SFPoint movement = SFPoint.ZeroSFPoint;
    private SFCircle shotBox;

    public PlayerDeathShot(sfloat speed, sfloat dir, SFPoint startPos)
    {
        shotBox = new SFCircle(startPos, (sfloat)12);
        movement.x = speed * libm.sinf(dir / (sfloat)180 * sfloat.FromRaw(0x40490fdb));
        movement.y = speed * libm.cosf(dir / (sfloat)180 * sfloat.FromRaw(0x40490fdb));
    }

    public bool Process()
    {
        lifetime++;
        shotBox.center += movement;
        if (shotBox.center.y > (sfloat)336 || shotBox.center.y < (sfloat)(-16) || shotBox.center.x > (sfloat)256 || shotBox.center.x < (sfloat)(-16)) return true;
        return false;
    }
    public SFCircle GetHitBox()
    {
        return shotBox;
    }

    public DrawInfo GetDrawFrames(SpriteFrames frames)
    {
        Texture tex = frames.GetFrame("DeathShot", lifetime / 4 % 4);
        return new DrawInfo(tex, Vector2.Zero, 0, Vector2.One);
    }
}