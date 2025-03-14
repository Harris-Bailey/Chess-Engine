
namespace Chess_Application {
    public static class CompassDirections {
        public const int Up = 8;
        public const int Down = -8;
        public const int Left = -1;
        public const int Right = 1;
        public const int TopRight = 9;
        public const int TopLeft = 7;
        public const int BottomRight = -7;
        public const int BottomLeft = -9;
        
        public static int GetDirectionIndex(int direction) {
            switch (direction) {
                case Up:
                    return 0;
                case Down: 
                    return 1;
                case Left: 
                    return 2;
                case Right: 
                    return 3;
                case TopRight: 
                    return 4;
                case TopLeft: 
                    return 5;
                case BottomRight: 
                    return 6;
                case BottomLeft: 
                    return 7;
                default: 
                    return -1;
            }
        }

        public static bool IsDiagonalDirection(int direction) {
            return direction == TopRight || direction == TopLeft || direction == BottomRight || direction == BottomLeft;
        }

        public static bool IsCardinalDirection(int direction) {
            return IsHoriztonal(direction) || IsVertical(direction);
        }

        public static bool IsHoriztonal(int direction) {
            return direction == Right || direction == Left;
        }

        public static bool IsVertical(int direction) {
            return direction == Up || direction == Down;
        }

        public static readonly int[] Cardinals = {
            Up,
            Right,
            Down,
            Left
        };

        public static readonly int[] Diagonals = {
            TopRight,
            BottomRight,
            BottomLeft,
            TopLeft
        };
        
        public static readonly int[] CardinalsAndDiagonals = {
            Up,
            Right,
            Down,
            Left,
            TopRight,
            BottomRight,
            BottomLeft,
            TopLeft
        };
        
        public static int NumDirections => CardinalsAndDiagonals.Length;
    }
}