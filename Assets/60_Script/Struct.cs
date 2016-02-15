using UnityEngine;
using System.Collections;
using System;

namespace Commons
{
    public struct Point : IEquatable<Point>
    {
        public int x, z;
        private int m_HashCode;

        public Point(int x, int z)
        {
            this.x = x;
            this.z = z;
            m_HashCode = (31 * x) + z.GetHashCode();
        }

        public override int GetHashCode()
        {
            return m_HashCode;
        }

        bool IEquatable<Point>.Equals(Point other)
        {
            return x == other.x && z == other.z;
        }


    };

    public enum UnitSelectStatus
    {
        Neutral,
        Selected,
        Moving,
        AfterMoved,
        MenuOpened,
    }

    public enum Direction
    {
        North,
        East,
        South,
        West,
    }
}

