using Godot;
using System;
using SoftFloat;
using SFAABBCC_Prereqs;
using static SFAABBCC_Prereqs.CC;
using System.Collections.Generic;

namespace BulletPack
{
    public enum HitBoxType
    {
        Circle = 0,
        Line,
        Pulse //2 circles w/ 1u radius whose centers are (hitboxLength) away (x axis, one w/ +, the other w/ -) from (position)
    }
    public enum BltColor
    {
        Yellow = 0,
        Blue,
        Red,
        Purple,
        Pink
    }
    public enum FXColor
    {
        Yellow = 0,
        Blue,
        Red,
        Purple,
        Pink,
        Orange,
        Cyan
    }
    public enum BltShape
    {
        Bean = 0,
        Counter8px,
        Counter12px,
        Counter16px,
        DoubleBeam,
        LargeRugby,
        Missile,
        Needle,
        Pulse,
        Ring,
        SingleBeam,
        Spark,
        Strobe6px,
        Strobe8px,
        Strobe16px,
        TJO,
        Whirl
    }
    public enum ItemType
    {
        Gem = -4,
        Bomb,
        TwoUp,
        OneUp,
        Medal100,
        Medal200,
        Medal300,
        Medal400,
        Medal500,
        Medal600,
        Medal700,
        Medal800,
        Medal900,
        Medal1000,
        Medal2000,
        Medal3000,
        Medal4000,
        Medal5000,
        Medal6000,
        Medal7000,
        Medal8000,
        Medal9000,
        Medal10000
    }

    public class BulletInfo
    {
        public Bullet blt;
        public bool drawOnBottom;

        public BulletInfo(Bullet blt, bool drawOnBottom = false)
        {
            this.blt = blt;
            this.drawOnBottom = drawOnBottom;
        }
    }

    public class DrawInfo
    {
        public Texture tex;
        public Vector2 drawOffset;
        public float angle;
        public Vector2 scale;
        public Color color;
        public DrawInfo(Texture tex, Vector2 drawOffset, float angle, Vector2 scale)
        {
            this.tex = tex;
            this.drawOffset = drawOffset;
            this.angle = angle;
            this.scale = scale;
            color = new Color(1, 1, 1);
        }
        public DrawInfo(Texture tex, Vector2 drawOffset, float angle, Vector2 scale, Color color)
        {
            this.tex = tex;
            this.drawOffset = drawOffset;
            this.angle = angle;
            this.scale = scale;
            this.color = color;
        }
    }

    public class Bullet
    {
        public SFPoint position;
        public SFPoint velocity;
        public BltColor col;
        public BltShape shp;
        public HitBoxType hitboxType;
        public sfloat hitboxLength;
        public int spawnRank;
        public int lifeTime;
        public int maxLifeTime;
        public int drawOrder;
        public SFPoint drawOffset;

        public Bullet(BltColor color, BltShape shape, SFPoint position, SFPoint velocity, int drawOrder = 0, int maxLifeTime = 600)
        {
            this.position = position;
            this.velocity = velocity;
            this.drawOrder = drawOrder;
            this.maxLifeTime = maxLifeTime;
            lifeTime = -1;
            col = color;
            shp = shape;
            
            switch (shape)
            {
                case BltShape.Bean:
                case BltShape.Strobe6px:
                    hitboxType = HitBoxType.Circle;
                    hitboxLength = (sfloat)1;
                    break;
                case BltShape.Counter8px:
                case BltShape.Strobe8px:
                case BltShape.TJO:
                    hitboxType = HitBoxType.Circle;
                    hitboxLength = (sfloat)2;
                    break;
                case BltShape.Counter12px:
                case BltShape.Spark:
                    hitboxType = HitBoxType.Circle;
                    hitboxLength = (sfloat)4;
                    break;
                case BltShape.Counter16px:
                case BltShape.DoubleBeam:
                case BltShape.Ring:
                case BltShape.Strobe16px:
                case BltShape.Whirl:
                    hitboxType = HitBoxType.Circle;
                    hitboxLength = (sfloat)6;
                    break;
                case BltShape.LargeRugby:
                    hitboxType = HitBoxType.Circle;
                    hitboxLength = (sfloat)8;
                    break;
                case BltShape.Missile:
                case BltShape.Needle:
                    hitboxType = HitBoxType.Line;
                    hitboxLength = (sfloat)12;
                    break;
                case BltShape.SingleBeam:
                    hitboxType = HitBoxType.Line;
                    hitboxLength = (sfloat)10;
                    break;
                case BltShape.Pulse:
                    hitboxType = HitBoxType.Pulse;
                    hitboxLength = (sfloat)9;
                    break;
            }

            switch (shape) {
                case BltShape.Bean:
                case BltShape.Strobe6px:
                case BltShape.Counter8px:
                case BltShape.Strobe8px:
                case BltShape.TJO:
                case BltShape.Counter12px:
                case BltShape.Spark:
                case BltShape.Counter16px:
                case BltShape.DoubleBeam:
                case BltShape.Ring:
                case BltShape.Strobe16px:
                case BltShape.Whirl:
                case BltShape.LargeRugby:
                case BltShape.Pulse:
                    drawOffset = SFPoint.ZeroSFPoint;
                    break;
                case BltShape.SingleBeam:
                    drawOffset = new SFPoint((sfloat)4, (sfloat)0);
                    break;
                case BltShape.Missile:
                    drawOffset = new SFPoint((sfloat)8, (sfloat)0);
                    break;
                case BltShape.Needle:
                    drawOffset = new SFPoint((sfloat)6, (sfloat)0);
                    break;
            }
            if (col == BltColor.Yellow && shp != BltShape.Missile) drawOffset = SFPoint.ZeroSFPoint;
        }

        public virtual void Movement(BulletManager bManager)
        {
            position += velocity;
            if (shp == BltShape.Missile)
            {
                if (lifeTime % 3 == 0)
                    bManager.CreateExhaust(new ExhaustFX(col == BltColor.Yellow ? FXColor.Cyan : (FXColor)col, position, SFPoint.ZeroSFPoint));
            }
        }

        public virtual List<DrawInfo> GetDrawFrames(SceneTree tree, GameManager gManager, PlayerController player, SpriteFrames frames)
        {
            List<DrawInfo> rtnValue = new List<DrawInfo>();
            string typeText = shp.ToString();
            if (col == BltColor.Yellow && shp != BltShape.Missile) typeText = "Rugby";
            int frame = (int)(60f / frames.GetAnimationSpeed(col.ToString() + "_" + typeText));
            frame = lifeTime % (frames.GetFrameCount(col.ToString() + "_" + typeText) * frame) / frame;
            SFPoint norm = velocity.Normalized();
            float angle = 0;
            if (hitboxType == HitBoxType.Line || shp == BltShape.DoubleBeam) angle = Mathf.Atan2((float)norm.y, (float)norm.x);
            else if (col == BltColor.Yellow && shp != BltShape.Missile)
            {
                angle = Mathf.Pi * 2 / 16 * (lifeTime % 16);
                frame = 0;
            }
            Texture frameTex = frames.GetFrame(col.ToString() + "_" + typeText, frame);
            rtnValue.Add(
                new DrawInfo(
                    frameTex,
                    (Vector2)drawOffset,
                    angle,
                    Vector2.One
                )
            );
            return rtnValue;
        }
    }

    public class CancelFX
    {
        public FXColor col;
        public SFPoint position;
        public SFPoint velocity;
        public bool large;
        public int lifeTime;

        public CancelFX(FXColor m_color, SFPoint m_position, SFPoint m_velocity, bool m_large)
        {
            col = m_color;
            position = m_position;
            velocity = m_velocity;
            large = m_large;
            lifeTime = -1;
        }
    }

    public class ExhaustFX
    {
        public FXColor col;
        public SFPoint position;
        public SFPoint velocity;
        public int lifeTime;

        public ExhaustFX(FXColor m_color, SFPoint m_position, SFPoint m_velocity)
        {
            col = m_color;
            position = m_position;
            velocity = m_velocity;
            lifeTime = -1;
        }
    }

    public enum FlashType
    {
        None = -1,
        Circle8px,
        Circle16px,
        Circle24px,
        AimedSmall,
        AimedDouble,
        AimedLarge
    }

    public class FlashFX
    {
        public SFPoint position;
        public bool done;
        public FlashType type;
        public sfloat angle;

        public FlashFX(SFPoint m_position, FlashType m_type, sfloat m_angle)
        {
            position = m_position;
            type = m_type;
            angle = m_angle;
            done = false;
        }

        public virtual List<DrawInfo> GetDrawFrames(SpriteFrames frames)
        {
            List<DrawInfo> rtnValue = new List<DrawInfo>();

            if (type == FlashType.None) return rtnValue;

            Vector2 drawOffset = type switch
            {
                FlashType.AimedSmall => Vector2.Right * 3.5f,
                FlashType.AimedDouble => Vector2.Right * 3.5f,
                FlashType.AimedLarge => Vector2.Right * 12,
                _ => Vector2.Zero,
            };

            rtnValue.Add(
                new DrawInfo(
                    frames.GetFrame("Flash", (int)type),
                    drawOffset,
                    (int)type >= 3 ? (float)angle : 0,
                    Vector2.One
                )
            );
            return rtnValue;
        }
    }

    public class Item
    {
        public ItemType type;
        public SFAABB itemBox;
        public SFPoint movement = new SFPoint((sfloat)0, (sfloat)(-3));
        public int lifeTime = 0;

        readonly sfloat maxSpeed;
        readonly GameManager gManager;

        public Item(GameManager gManager, ItemType type, SFPoint pos)
        {
            this.gManager = gManager;
            maxSpeed = ((sfloat)gManager.rank / (sfloat)250000 + (sfloat)2);
            this.type = type;
            itemBox = new SFAABB(pos, new SFPoint((sfloat)32, (sfloat)32));
        }

        public bool Movement(PlayerController player)
        {
            if (type == ItemType.Gem && lifeTime > 30 && player.ang <= 1)
            {
                movement = (player.GetHitBox().center - itemBox.center).Normalized() * (sfloat)8;
            }
            else
            {
                movement.y += (sfloat)0.1f;
                movement.y = sfloat.Min(movement.y, maxSpeed);
            }
            itemBox.center += movement;
            if (itemBox.center.y > (sfloat)336)
            {
                if ((int)type >= 0) gManager.currentMedal = 0;
                return true;
            }
            return false;
        }

        public void Rebound()
        {
            movement.y = (sfloat)(-4.5);
        }

        public List<DrawInfo> GetDrawFrames(SpriteFrames frames)
        {
            List<DrawInfo> rtnValue = new List<DrawInfo>();
            Vector2 drawOffset = Vector2.Zero;
            string anim = "";
            int frame = 0;

            switch ((int)type)
            {
                case -4:
                    anim = "Gem";
                    frame = lifeTime / 6 % 4;
                    break;
                case -3:
                    anim = "Bomb";
                    break;
                case -2:
                    anim = "2up";
                    break;
                case -1:
                    anim = "1up";
                    break;
                case int n when n >= 0 && n < 19:
                    anim = "Medal";
                    frame = n;
                    break;
            }

            rtnValue.Add(
                new DrawInfo(
                    frames.GetFrame(anim, frame),
                    drawOffset,
                    0,
                    Vector2.One
                )
            );

            return rtnValue;
        }
    }
}
