
namespace scch
{
    public struct Vector3
    {
        public float x, y, z;

        public static implicit operator Vector3(Vec3 real)
        {
            return new Vector3() { x = real.x, y = real.y, z = real.z };
        }
    }

    public struct Vec3
    {
        public float x;
        public float y;
        public float z;

        public Vec3(float x, float y, float z)
        {
            this.x = x;
            this.y = y;
            this.z = z;
        }

        public override string ToString()
        {
            return "X: " + x + ", Y: " + y + ", Z: " + z;
        }

        public bool isZero()
        {
            if (x == 0 && y == 0 && z == 0)
                return true;
            else
                return false;
        }

        public static float Distance(Vec3 u, Vec3 v)
        {
            return (u - v).Length;
        }

        public float Length
        {
            get { return (int)System.Math.Sqrt(x * x + y * y + z * z); }
        }

        public float DotProduct(Vec3 v)
        {
            return x * v.x + y * v.y + z * v.z;
        }

        public static Vec3 operator +(Vec3 u, Vec3 v)
        {
            return new Vec3(u.x + v.x, u.y + v.y, u.z + v.z);
        }

        public static Vec3 operator -(Vec3 u, Vec3 v)
        {
            return new Vec3(u.x - v.x, u.y - v.y, u.z - v.z);
        }

        public static implicit operator Vec3(Vector3 real)
        {
            return new Vec3() { x = real.x, y = real.y, z = real.z };
        }
    }
}
