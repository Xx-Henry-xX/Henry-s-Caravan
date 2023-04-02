using Godot;
using System;
using SoftFloat;
using System.Collections.Generic;

namespace SFAABBCC_Prereqs //CC stands for Collision Checkers
{
    public struct SFPoint
    {
        public sfloat x;
        public sfloat y;

        public SFPoint(sfloat x, sfloat y)
        {
            this.x = x;
            this.y = y;
        }

        public SFPoint(SFPoint a)
        {
            x = a.x;
            y = a.y;
        }

        public static SFPoint ZeroSFPoint => new SFPoint((sfloat)0, (sfloat)0);

        public SFPoint Clone()
        {
            return new SFPoint(x, y);
        }

        public sfloat Length(bool normalize)
        {
            sfloat multiValue = x * x + y * y;
            sfloat length = libm.sqrtf(multiValue);
            if (length != (sfloat)0 && normalize)
            {
                multiValue = (sfloat)1 / length;
                x *= multiValue;
                y *= multiValue;
            }
            return length;
        }

        public sfloat LengthSquared()
        {
            return x * x + y * y;
        }

        public SFPoint Normalized()
        {
            SFPoint rtnValue = new SFPoint(this);
            rtnValue.Length(true);
            return rtnValue;
        }

        public SFPoint Rotated(sfloat angleInRadians)
        {
            return new SFPoint(x * libm.cosf(angleInRadians) - y * libm.sinf(angleInRadians),
                               y * libm.cosf(angleInRadians) + x * libm.sinf(angleInRadians));
        }

        public static SFPoint operator +(SFPoint point1, SFPoint point2)
        {
            return new SFPoint(point1.x + point2.x, point1.y + point2.y);
        }

        public static SFPoint operator -(SFPoint point1, SFPoint point2)
        {
            return new SFPoint(point1.x - point2.x, point1.y - point2.y);
        }

        public static SFPoint operator -(SFPoint point1)
        {
            return new SFPoint(-point1.x, -point1.y);
        }

        public static SFPoint operator *(SFPoint point1, sfloat z)
        {
            return new SFPoint(point1.x * z, point1.y * z);
        }
        public static SFPoint operator /(SFPoint point1, sfloat z)
        {
            return new SFPoint(point1.x / z, point1.y / z);
        }

        static public explicit operator Vector2(SFPoint point)
        {
            return new Vector2((float)point.x, (float)point.y);
        }
    }

    public struct SFAABB
    {
        public SFPoint center; // center of the SFAABB as a sfPoint
        public SFPoint halfLength; // 1/2 of length of each side as a sfPoint

        public SFAABB(SFPoint center, SFPoint halfLength)
        {
            this.center = center;
            this.halfLength = halfLength;
        }

        public SFAABB Offset(SFPoint offset)
        {
            return new SFAABB(center + offset, halfLength);
        }
    }

    public struct SFCircle
    {
        public SFPoint center; // center of the SFAABB as a sfPoint
        public sfloat radius; // radius of

        public SFCircle(SFPoint center, sfloat radius)
        {
            this.center = center;
            this.radius = radius;
        }

        public SFCircle Offset(SFPoint offset)
        {
            return new SFCircle(center + offset, radius);
        }
    }

    public static class CC {
        public static bool IntersectBoxVSPoint(SFAABB box, SFPoint point) // self against point
        {
            return (box.halfLength.x >= sfloat.Abs(point.x - box.center.x)) && (box.halfLength.y >= sfloat.Abs(point.y - box.center.y));
        }

        public static bool IntersectBoxVSSegment(SFAABB box, SFPoint point, SFPoint delta) // self against line segment (delta is forward speed)
        {
            sfloat scaleX = (sfloat)1 / delta.x;
            sfloat scaleY = (sfloat)1 / delta.y;
            int signX = scaleX.Sign();
            int signY = scaleY.Sign();
            sfloat nearTimeX = (box.center.x - (sfloat)signX * box.halfLength.x - point.x) * scaleX;
            sfloat nearTimeY = (box.center.y - (sfloat)signY * box.halfLength.y - point.y) * scaleY;
            sfloat farTimeX = (box.center.x + (sfloat)signX * box.halfLength.x - point.x) * scaleX;
            sfloat farTimeY = (box.center.y + (sfloat)signY * box.halfLength.y - point.y) * scaleY;
            if (nearTimeX > farTimeY || nearTimeY > farTimeX)
            {
                return false;
            }
            sfloat nearTime = nearTimeX > nearTimeY ? nearTimeX : nearTimeY;
            sfloat farTime = farTimeX < farTimeY ? farTimeX : farTimeY;
            if (nearTime >= (sfloat)1 || farTime <= (sfloat)0)
            {
                return false;
            }
            return true;
        }

        public static bool IntersectBoxVSBox(SFAABB a, SFAABB b) // self against box
        {
            return (a.halfLength.x + b.halfLength.x >= sfloat.Abs(b.center.x - a.center.x)) && (a.halfLength.y + b.halfLength.y >= sfloat.Abs(b.center.y - a.center.y));
        }

        public static bool IntersectCircleVSBox(SFCircle a, SFAABB b)
        {
            SFPoint clamped = a.center - b.center;
            clamped.x = Clamp(clamped.x, -b.halfLength.x, b.halfLength.x);
            clamped.y = Clamp(clamped.y, -b.halfLength.y, b.halfLength.y);
            clamped += b.center;
            return (clamped - a.center).LengthSquared() <= a.radius * a.radius;
        }

        public static bool IntersectCircleVSCircle(SFCircle a, SFCircle b)
        {
            return (a.center - b.center).LengthSquared() <= (a.radius + b.radius) * (a.radius + b.radius);
        }

        public static bool IntersectCircleVSSegment(SFCircle a, SFPoint point, SFPoint delta)
        {
            sfloat buffer = (sfloat)0.1f;

            if (a.radius * a.radius > (a.center - point).LengthSquared() || a.radius * a.radius > (a.center - point - delta).LengthSquared()) return true;
            sfloat len = (delta - point).Length(true);
            sfloat dot = (((a.center.x - point.x) * (delta.x)) + ((a.center.y - point.y) * (delta.y))) / (len * len);
            SFPoint closest = new SFPoint(point.x + (dot * delta.x), point.y + (dot * delta.y));
            if (sfloat.Abs((closest - point).Length(false) + (closest - point - delta).Length(false) - delta.Length(false)) > buffer) return false;
            if ((closest - a.center).LengthSquared() < a.radius * a.radius) return true;
            return false;
        }

        public static sfloat Clamp(sfloat a, sfloat min, sfloat max)
        {
            if (min > max) return sfloat.Min(sfloat.Max(a, max), min);
            else return sfloat.Min(sfloat.Max(a, min), max);
        }

        public static List<SFPoint> Fan(SFPoint initial, int count, sfloat gapInRadians)
        {
            List<SFPoint> rtnValue = new List<SFPoint>();
            if (count % 2 == 1)
            {
                rtnValue.Add(initial);
                for (int i = 0; i < count/2; i++)
                {
                    rtnValue.Add(initial.Rotated( gapInRadians * (sfloat)(i + 1) ));
                    rtnValue.Add(initial.Rotated( gapInRadians * (sfloat)(i + 1) * (sfloat)(-1) ));
                }
            }
            else
            {
                for (int i = 0; i < count / 2; i++)
                {
                    rtnValue.Add(initial.Rotated( gapInRadians * ((sfloat)i + (sfloat)0.5)));
                    rtnValue.Add(initial.Rotated( gapInRadians * ((sfloat)i + (sfloat)0.5) * (sfloat)(-1) ));
                }
            }
            return rtnValue;
        }

        public static List<SFPoint> Round(SFPoint initial, int count)
        {
            List<SFPoint> rtnValue = new List<SFPoint>();
            for (int i = 0; i < count; i++) rtnValue.Add(new SFPoint(initial.x * libm.cosf(sfloat.FromRaw(0x40c90fdb) * (sfloat)i / (sfloat)count) - initial.y * libm.sinf(sfloat.FromRaw(0x40c90fdb) * (sfloat)i / (sfloat)count),
                                                                     initial.y * libm.cosf(sfloat.FromRaw(0x40c90fdb) * (sfloat)i / (sfloat)count) + initial.x * libm.sinf(sfloat.FromRaw(0x40c90fdb) * (sfloat)i / (sfloat)count)));
            return rtnValue;
        }
    }
}
