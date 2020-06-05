using System;

namespace ThreeDMaze
{
    struct Coordinate : IEquatable<Coordinate>
    {
        public int X { get; private set; }
        public int Y { get; private set; }

        public Coordinate(int x, int y) { X = x; Y = y; }

        public bool Equals(Coordinate other)
        {
            return other.X == X && other.Y == Y;
        }

        public override int GetHashCode()
        {
            return unchecked(X * 37 + Y);
        }

        public override bool Equals(object obj)
        {
            return (obj is Coordinate) && Equals((Coordinate) obj);
        }
    }
}
