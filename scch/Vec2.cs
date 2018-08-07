

namespace scch
{
    public struct Vec2
    {
        public float x;
        public float y;

        public Vec2(float x, float y)
        {
            this.x = x;
            this.y = y;
        }

        public static Vec2 operator -(Vec2 c1, Vec2 c2)
        {
            return new Vec2(c1.x - c2.x, c1.y - c2.y);
        }

        public static Vec2 operator +(Vec2 c1, Vec2 c2)
        {
            return new Vec2(c1.x + c2.x, c1.y + c2.y);
        }

        public static Vec2 operator / (Vec2 c1, int p)
        {
            return new Vec2(c1.x / p, c1.y / p);
        }

        public static Vec2 operator /(Vec2 c1, double p)
        {
            return new Vec2(c1.x / (float)p, c1.y / (float)p);
        }

        public static Vec2 operator *(Vec2 c1, int p)
        {
            return new Vec2(c1.x * p, c1.y * p);
        }

        public static Vec2 operator *(Vec2 c1, float p)
        {
            return new Vec2(c1.x * p, c1.y * p);
        }

        public static bool operator <(Vec2 c1, int p)
        {
            return (c1.x < p) && (c1.y < p);
        }

        public static bool operator >(Vec2 c1, int p)
        {
            return (c1.x > p) && (c1.y > p);
        }
    }
}
