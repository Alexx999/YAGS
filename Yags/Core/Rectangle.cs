using System;

namespace Yags.Core
{
    public struct Rectangle : IEquatable<Rectangle>
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;

        public Rectangle(int x, int y, int width, int height)
            : this()
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Intersects(Rectangle toIntersect)
        {
            if (IsEmpty() || toIntersect.IsEmpty())
            {
                return (false);
            }
            int resultx = Math.Max(X, toIntersect.X);
            int resulty = Math.Max(Y, toIntersect.Y);
            int resultwidth = Math.Min(X + Width, toIntersect.X + toIntersect.Width) - resultx;
            int resultheight = Math.Min(Y + Height, toIntersect.Y + toIntersect.Height) - resulty;
            return (resultwidth > 0) && (resultheight > 0);
        }

        private bool IsEmpty()
        {
            return (Width <= 0) || (Height <= 0);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Rectangle && Equals((Rectangle)obj);
        }

        public bool Equals(Rectangle other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = X;
                hashCode = (hashCode * 397) ^ Y;
                hashCode = (hashCode * 397) ^ Width;
                hashCode = (hashCode * 397) ^ Height;
                return hashCode;
            }
        }

        public static bool operator ==(Rectangle left, Rectangle right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Rectangle left, Rectangle right)
        {
            return !left.Equals(right);
        }
    }
}