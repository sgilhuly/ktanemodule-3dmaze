using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace ThreeDMaze
{
    sealed class Map
    {
        public const int WIDTH = 8;

        // Add 1 to go clockwise, subtract 1 to go ccw
        public const int NORTH = 0;
        public const int EAST = 1;
        public const int SOUTH = 2;
        public const int WEST = 3;

        public const int RESULT_OK = 0;
        public const int RESULT_SUCCESS = 1;
        public const int RESULT_FAILURE = 2;

        private MapNode[,] mapData;

        private int win_x1;
        private int win_x2;
        private int win_y1;
        private int win_y2;
        private int sol_x;
        private int sol_y;

        private int pl_x;
        private int pl_y;
        private int pl_dir;
        private int end_dir;

        public void turnLeft()
        {
            pl_dir = bound(pl_dir - 1, 4);
        }

        public void turnRight()
        {
            pl_dir = bound(pl_dir + 1, 4);
        }

        public int moveForward()
        {
            if (!getEdge(pl_x, pl_y, pl_dir, 0, 0, pl_dir))
            {
                pl_x = bound(pl_x + xMod(1, 0, pl_dir), WIDTH);
                pl_y = bound(pl_y + yMod(1, 0, pl_dir), WIDTH);

                return RESULT_OK;
            }
            else
            {
                int new_x = bound(pl_x + xMod(1, 0, pl_dir), WIDTH);
                int new_y = bound(pl_y + yMod(1, 0, pl_dir), WIDTH);

                if ((pl_x == win_x1 && pl_y == win_y1 && new_x == win_x2 && new_y == win_y2) ||
                    (pl_x == win_x2 && pl_y == win_y2 && new_x == win_x1 && new_y == win_y1))
                {
                    return RESULT_SUCCESS;
                }
                else
                {
                    return RESULT_FAILURE;
                }
            }
        }

        public MapView getDefaultMapView()
        {
            return getMapView(pl_x, pl_y, pl_dir);
        }

        public MapView getMapView(int x, int y, int dir)
        {
            MapView view = new MapView();

            view.ble = getEdge(x, y, dir - 1, 1, 0, dir);
            view.blw = getEdge(x, y, dir, 1, -1, dir);
            view.bre = getEdge(x, y, dir + 1, 1, 0, dir);
            view.brw = getEdge(x, y, dir, 1, 1, dir);
            view.bw = getEdge(x, y, dir, 1, 0, dir);

            view.fle = getEdge(x, y, dir - 1, 0, 0, dir);
            view.flw = getEdge(x, y, dir, 0, -1, dir);
            view.fre = getEdge(x, y, dir + 1, 0, 0, dir);
            view.frw = getEdge(x, y, dir, 0, 1, dir);
            view.fw = getEdge(x, y, dir, 0, 0, dir);

            view.letter = getLetter(x, y);
            view.letter_far = getLetter(x + xMod(1, 0, dir), y + yMod(1, 0, dir)) != ' ';

            return view;
        }

        private int xMod(int ahead, int right, int dir)
        {
            switch (dir)
            {
                case NORTH:
                    return right;
                case SOUTH:
                    return -right;
                case EAST:
                    return ahead;
                case WEST:
                    return -ahead;
                default:
                    return 0;
            }
        }

        private int yMod(int ahead, int right, int dir)
        {
            switch (dir)
            {
                case NORTH:
                    return -ahead;
                case SOUTH:
                    return ahead;
                case EAST:
                    return right;
                case WEST:
                    return -right;
                default:
                    return 0;
            }
        }

        internal string GetLog()
        {
            return join("\n", Enumerable.Range(0, WIDTH * 2 + 2).Select(y =>
                join("", Enumerable.Range(0, WIDTH * 4 + 3).Select(x =>
                {
                    var xWall = (x - 2) % 4 == 0;
                    var yWall = (y - 1) % 2 == 0;
                    var xx = (x - 2) / 4;
                    var yy = (y - 1) / 2;

                    if (x == 0)
                        return (y - 1) % 2 == 1 && yy == sol_y ? '→' : ' ';
                    else if (x == 1)
                        return ' ';
                    else if (y == 0)
                        return (x - 2) % 4 == 2 && xx == sol_x ? '↓' : ' ';
                    else if (xWall && yWall)
                    {
                        var upWall = mapData[xx % WIDTH, (yy + WIDTH - 1) % WIDTH].w_wall;
                        var downWall = mapData[xx % WIDTH, yy % WIDTH].w_wall;
                        var leftWall = mapData[(xx + WIDTH - 1) % WIDTH, yy % WIDTH].n_wall;
                        var rightWall = mapData[xx % WIDTH, yy % WIDTH].n_wall;
                        return " ╨╥║╡╝╗╣╞╚╔╠═╩╦╬"[(upWall ? 1 : 0) + (downWall ? 2 : 0) + (leftWall ? 4 : 0) + (rightWall ? 8 : 0)];
                    }
                    else if (xWall)
                    {
                        var isSolution = yy % WIDTH == win_y1 && yy % WIDTH == win_y2 &&
                            ((xx % WIDTH == win_x1 && (xx + WIDTH - 1) % WIDTH == win_x2)
                            || (xx % WIDTH == win_x2 && (xx + WIDTH - 1) % WIDTH == win_x1));
                        return mapData[xx % WIDTH, yy % WIDTH].w_wall ? (isSolution ? '█' : '║') : ' ';
                    }
                    else if (yWall)
                    {
                        var isSolution = xx % WIDTH == win_x1 && xx % WIDTH == win_x2 &&
                            ((yy % WIDTH == win_y1 && (yy + WIDTH - 1) % WIDTH == win_y2)
                            || (yy % WIDTH == win_y2 && (yy + WIDTH - 1) % WIDTH == win_y1));
                        return mapData[xx % WIDTH, yy % WIDTH].n_wall ? (isSolution ? '■' : '═') : ' ';
                    }
                    else
                        return (x - 2) % 4 == 2 && (y - 1) % 2 == 1 ? mapData[xx, yy].label : xx == pl_x && yy == pl_y ? "▲►▼◄"[pl_dir] : ' ';
                }))));
        }

        private string join<T>(string separator, IEnumerable<T> pieces)
        {
            var sb = new StringBuilder();
            var first = true;
            foreach (var piece in pieces)
            {
                if (first)
                    first = false;
                else
                    sb.Append(separator);
                sb.Append(piece);
            }
            return sb.ToString();
        }

        private int bound(int a, int b)
        {
            a = a % b;
            if (a < 0)
            {
                a += b;
            }

            return a;
        }

        private bool getEdge(int x, int y, int dir, int ahead, int right, int facing)
        {
            dir = bound(dir, 4);

            x += xMod(ahead, right, facing);
            y += yMod(ahead, right, facing);

            if (dir == NORTH || dir == SOUTH)
            {
                if (dir == SOUTH)
                {
                    y++;
                }

                return mapData[bound(x, WIDTH), bound(y, WIDTH)].n_wall;
            }

            if (dir == EAST || dir == WEST)
            {
                if (dir == EAST)
                {
                    x++;
                }

                return mapData[bound(x, WIDTH), bound(y, WIDTH)].w_wall;
            }

            return false;
        }

        private char getLetter(int x, int y)
        {
            return mapData[bound(x, WIDTH), bound(y, WIDTH)].label;
        }

        public Map(string serialNumber, string[] litIndicators, string[] unlitIndicators, int moduleId, MonoRandom rnd)
        {
            var mazes = new List<MapNode[,]>();
            string rowPhrase, colPhrase;

            Debug.LogFormat("[3D Maze #{0}] Using rule seed: {1}", moduleId, rnd.Seed);
            if (rnd.Seed == 1)
            {
                string[] labels;
                string[] n_walls;
                string[] w_walls;

                for (var mapIndex = 0; mapIndex < 10; mapIndex++)
                {
                    var rawMapData = new MapNode[WIDTH, WIDTH];
                    switch (mapIndex)
                    {
                        // ABC
                        case 0:
                            labels = new string[] {
                                "     A  ",
                                " *A    B",
                                "A  B C  ",
                                " C  *  B",
                                "    A   ",
                                " B C  B ",
                                "* C     ",
                                "    A C "
                            };
                            n_walls = new string[] {
                                "11000110",
                                "00001000",
                                "01011100",
                                "00100011",
                                "10011001",
                                "00000100",
                                "00110010",
                                "11000001"
                            };
                            w_walls = new string[] {
                                "10101001",
                                "10100100",
                                "00010001",
                                "01010110",
                                "01010001",
                                "01100100",
                                "01001101",
                                "00101100"
                            };
                            break;

                        // ABD
                        case 1:
                            labels = new string[] {
                                "A  B  A*",
                                "  D     ",
                                "     D B",
                                " A B    ",
                                "  *   A ",
                                "D   A   ",
                                "  B  D  ",
                                " D  *  B"
                            };
                            n_walls = new string[] {
                                "10100011",
                                "01100100",
                                "00011010",
                                "00100000",
                                "11100110",
                                "00010100",
                                "10100001",
                                "00011000"
                            };
                            w_walls = new string[] {
                                "10000010",
                                "11000100",
                                "00010001",
                                "01000100",
                                "10010001",
                                "00010100",
                                "01000101",
                                "00010100"
                            };
                            break;

                        // ABH
                        case 2:
                            labels = new string[] {
                                "B    A H",
                                "* H     ",
                                "B   B   ",
                                "    * HA",
                                " A H    ",
                                "    A B ",
                                " B   *  ",
                                "A  H    "
                            };
                            n_walls = new string[] {
                                "11101011",
                                "01011000",
                                "10001101",
                                "01010000",
                                "00001000",
                                "00110100",
                                "00101110",
                                "01100000"
                            };
                            w_walls = new string[] {
                                "10010000",
                                "01000001",
                                "00110011",
                                "11010101",
                                "11010010",
                                "11100000",
                                "00010101",
                                "00000001"
                            };
                            break;

                        // ACD
                        case 3:
                            labels = new string[] {
                                "D       ",
                                "  C D* C",
                                " *   C  ",
                                " A      ",
                                "D  C D  ",
                                "  A  * A",
                                "   A  D ",
                                "A    C  "
                            };
                            n_walls = new string[] {
                                "10111011",
                                "01100110",
                                "00000111",
                                "01000000",
                                "11101110",
                                "01100100",
                                "00010110",
                                "01101100"
                            };
                            w_walls = new string[] {
                                "00001000",
                                "11001100",
                                "01110010",
                                "10011001",
                                "10001001",
                                "01100101",
                                "01010000",
                                "10000010"
                            };
                            break;

                        // ACH
                        case 4:
                            labels = new string[] {
                                "H C   A ",
                                "*   H   ",
                                "      *C",
                                " A   H  ",
                                "C H C A ",
                                " *     A",
                                "   C H  ",
                                "  A     "
                            };
                            n_walls = new string[] {
                                "00111100",
                                "11010000",
                                "01100110",
                                "00000000",
                                "01000100",
                                "00000001",
                                "11100010",
                                "01011011"
                            };
                            w_walls = new string[] {
                                "10000110",
                                "10010001",
                                "01010101",
                                "10000001",
                                "11000000",
                                "01010101",
                                "10010000",
                                "01000010"
                            };
                            break;

                        // ADH
                        case 5:
                            labels = new string[] {
                                "D D  *  ",
                                "    H  A",
                                " *H   A ",
                                "A  D    ",
                                "    HD  ",
                                "* H    A",
                                "D       ",
                                "   A H  "
                            };
                            n_walls = new string[] {
                                "01110101",
                                "00001110",
                                "00000111",
                                "01001100",
                                "00110101",
                                "11100000",
                                "01100000",
                                "11001000"
                            };
                            w_walls = new string[] {
                                "10100100",
                                "11110000",
                                "11011000",
                                "01110000",
                                "10000101",
                                "10001110",
                                "00011111",
                                "10000101"
                            };
                            break;

                        // BCD
                        case 6:
                            labels = new string[] {
                                "     B  ",
                                "C D   * ",
                                " * B  C ",
                                " C    B ",
                                "    C  D",
                                "B    D  ",
                                " C  * D ",
                                "D  B    "
                            };
                            n_walls = new string[] {
                                "01011110",
                                "01101011",
                                "01100100",
                                "10000111",
                                "10110011",
                                "10011001",
                                "01100010",
                                "10111001"
                            };
                            w_walls = new string[] {
                                "10010000",
                                "00001010",
                                "11101000",
                                "00001000",
                                "00100010",
                                "00010001",
                                "00000110",
                                "00100001"
                            };
                            break;

                        // BCH
                        case 7:
                            labels = new string[] {
                                "C   H   ",
                                "  C    H",
                                "  * B   ",
                                "B  H*   ",
                                " H   B C",
                                "   *    ",
                                "  B C   ",
                                " C   H B"
                            };
                            n_walls = new string[] {
                                "10110011",
                                "01010111",
                                "10101100",
                                "00000110",
                                "01111110",
                                "00100010",
                                "10010100",
                                "00000110"
                            };
                            w_walls = new string[] {
                                "10000100",
                                "01001000",
                                "01111001",
                                "10001000",
                                "10001000",
                                "01101101",
                                "01101001",
                                "00010010"
                            };
                            break;

                        // BDH
                        case 8:
                            labels = new string[] {
                                "  D B  H",
                                "   *  D ",
                                "  H *  B",
                                "D    B  ",
                                "    D  H",
                                "  B     ",
                                "   H  H*",
                                "D    B  "
                            };
                            n_walls = new string[] {
                                "00100001",
                                "00010001",
                                "11011000",
                                "11011011",
                                "00010011",
                                "10000000",
                                "01101100",
                                "01001111"
                            };
                            w_walls = new string[] {
                                "01101000",
                                "11010111",
                                "00000110",
                                "00100000",
                                "00110100",
                                "10001110",
                                "11000000",
                                "00011010"
                            };
                            break;

                        // CDH
                        default:
                            labels = new string[] {
                                "  H  D  ",
                                "    C*  ",
                                "   H   D",
                                "H    D  ",
                                "  C     ",
                                "C  D C H",
                                "*D  H * ",
                                "       C"
                            };
                            n_walls = new string[] {
                                "01011010",
                                "00100100",
                                "00000000",
                                "01000010",
                                "01000010",
                                "01100110",
                                "01011010",
                                "10100101"
                            };
                            w_walls = new string[] {
                                "11001001",
                                "11110111",
                                "00100010",
                                "00011100",
                                "10011100",
                                "10001000",
                                "11000001",
                                "00101010"
                            };
                            break;
                    }

                    for (int y = 0; y < WIDTH; y++)
                        for (int x = 0; x < WIDTH; x++)
                            rawMapData[x, y] = new MapNode(labels[y][x], n_walls[y][x] == '1', w_walls[y][x] == '1');
                    mazes.Add(rawMapData);
                }

                rowPhrase = "MAZEGAMER";
                colPhrase = "HELPIMLOST";
            }
            else
            {
                // RULE SEEDED CODE starts here
                var mazeLabels = "ABC,ABD,ABH,ACD,ACH,ADH,BCD,BCH,BDH,CDH".Split(',');
                foreach (var labels in mazeLabels)
                {
                    var mapData = new MapNode[WIDTH, WIDTH];
                    for (var x = 0; x < WIDTH; x++)
                        for (var y = 0; y < WIDTH; y++)
                            mapData[x, y] = new MapNode(' ', true, true);
                    var todo = Enumerable.Range(0, WIDTH * WIDTH).Select(i => new Coordinate(i % WIDTH, i / WIDTH)).ToList();
                    var visited = new List<Coordinate>();
                    var done = new List<Coordinate>();

                    // Choose a random square to start from
                    var startIx = rnd.Next(0, todo.Count);
                    visited.Add(todo[startIx]);
                    todo.RemoveAt(startIx);

                    // Generate a maze
                    while (todo.Count > 0)
                    {
                        var cellIx = rnd.Next(0, visited.Count);
                        var cell = visited[cellIx];

                        var validWalls = Enumerable.Range(0, 4).Select(dir => new { Dir = dir, Cell = Neighbor(cell, dir) }).Where(c => todo.Contains(c.Cell)).ToArray();
                        if (validWalls.Length == 0)
                        {
                            visited.RemoveAt(cellIx);
                            done.Add(cell);
                            continue;
                        }

                        var wallIx = rnd.Next(0, validWalls.Length);
                        var wall = validWalls[wallIx];
                        switch (wall.Dir)
                        {
                            case NORTH: mapData[cell.X, cell.Y].n_wall = false; break;
                            case WEST: mapData[cell.X, cell.Y].w_wall = false; break;
                            case SOUTH: mapData[wall.Cell.X, wall.Cell.Y].n_wall = false; break;
                            default: mapData[wall.Cell.X, wall.Cell.Y].w_wall = false; break;
                        }
                        todo.Remove(wall.Cell);
                        visited.Add(wall.Cell);
                    }

                    // Remove 20–35% of the remaining walls to make the maze more spacious
                    var remainingWalls = new List<int>();
                    for (var x = 0; x < WIDTH; x++)
                        for (var y = 0; y < WIDTH; y++)
                        {
                            if (mapData[x, y].n_wall)
                                remainingWalls.Add((WIDTH * y + x) * 2);
                            if (mapData[x, y].w_wall)
                                remainingWalls.Add((WIDTH * y + x) * 2 + 1);
                        }
                    var percentage = (rnd.NextDouble() * .15) + .2;
                    var removeWalls = (int) (remainingWalls.Count * percentage);
                    while (removeWalls > 0)
                    {
                        var wallIx = rnd.Next(0, remainingWalls.Count);
                        var wall = remainingWalls[wallIx];
                        remainingWalls.RemoveAt(wallIx);
                        var x = (wall / 2) % WIDTH;
                        var y = (wall / 2) / WIDTH;

                        if (wall % 2 == 1)
                        {
                            // Make sure not to remove a wall that would leave an entire row devoid of walls
                            if (Enumerable.Range(0, WIDTH).Count(xx => mapData[xx, y].w_wall) == 1)
                                continue;
                            mapData[x, y].w_wall = false;
                        }
                        else
                        {
                            // Make sure not to remove a wall that would leave an entire column devoid of walls
                            if (Enumerable.Range(0, WIDTH).Count(yy => mapData[x, yy].n_wall) == 1)
                                continue;
                            mapData[x, y].n_wall = false;
                        }
                        removeWalls--;
                    }

                    // Select 33 random cells for the labels (letters and cardinals)
                    done.AddRange(visited);
                    rnd.ShuffleFisherYates(done);
                    // Add the labels (5 of each letter)
                    for (var letterIx = 0; letterIx < labels.Length; letterIx++)
                        for (var i = 0; i < 5; i++)
                            mapData[done[5 * letterIx + i].X, done[5 * letterIx + i].Y].label = labels[letterIx];
                    // Add the 3 asterisks
                    for (var i = 0; i < 3; i++)
                        mapData[done[30 + i].X, done[30 + i].Y].label = '*';

                    mazes.Add(mapData);
                }

                var phrases = new List<string>
                {
                    "MAZE GAMER",
                    "MAZE TRAVELER",
                    "MAZE CORRIDOR",
                    "MAZE SOJOURNER",
                    "MAZE VOYAGER",
                    "HELP IM LOST",
                    "WINDING MAZE",
                    "TWISTY PASSAGES",
                    "ADVENTURING",
                    "FIND THE EXIT",
                    "WHERES THE EXIT",
                    "GO TO THE EXIT",
                    "FIND THE WALL",
                    "UPON DISCOVERY",
                    "ON A JOURNEY",
                    "SOS WHERE AM I",
                    "SHOW ME AROUND",
                    "I NEED ASSISTANCE",
                    "LABYRINTHIAN",
                    "PATH FINDER",
                    "CARDINAL DIRECTION",
                    "WHAT IS THIS PLACE",
                    "GO IN CIRCLES",
                    "DEAD END",
                    "TURN AROUND"
                };
                var ix = rnd.Next(0, phrases.Count);
                rowPhrase = phrases[ix];
                phrases.RemoveAt(ix);
                colPhrase = phrases[rnd.Next(0, phrases.Count)];
                Debug.LogFormat("<3D Maze #{0}> Row phrase: {1}", moduleId, rowPhrase);
                Debug.LogFormat("<3D Maze #{0}> Column phrase: {1}", moduleId, colPhrase);
            }

            // Calculate the goal position from the edgework
            var firstDigit = serialNumber.Where(ch => ch >= '0' && ch <= '9').First();
            var lastDigit = serialNumber.Where(ch => ch >= '0' && ch <= '9').Last();
            var rowMsg = firstDigit.ToString();
            var numUnlit = 0;
            foreach (string unlit in unlitIndicators)
            {
                // MAZE GAMER
                if (unlit.Intersect(rowPhrase).Any())
                {
                    numUnlit++;
                    rowMsg += " + " + unlit;
                }
            }
            if (numUnlit == 0)
                rowMsg = "no unlit indicators";

            var colMsg = lastDigit.ToString();
            var numLit = 0;
            foreach (string lit in litIndicators)
            {
                // HELP I'M LOST
                if (lit.Intersect(colPhrase).Any())
                {
                    numLit++;
                    colMsg += " + " + lit;
                }
            }
            if (numLit == 0)
                colMsg = "no lit indicators";

            sol_x = (lastDigit + numLit) % WIDTH;
            sol_y = (firstDigit + numUnlit) % WIDTH;

            mapData = mazes[Random.Range(0, 10)];
            end_dir = Random.Range(0, 4);
            var decoy_dir = Random.Range(0, 3);
            if (decoy_dir >= end_dir)
                decoy_dir++;

            Debug.LogFormat("[3D Maze #{0}] Selected map: {1}", moduleId, join("", Enumerable.Range(0, WIDTH * WIDTH).Select(ix => mapData[ix % WIDTH, ix / WIDTH].label).Where("ABCDH".Contains).Distinct().OrderBy(c => c)));
            Debug.LogFormat("[3D Maze #{0}] Column: {1} ({2}), Row: {3} ({4}), Cardinal: {5}", moduleId, sol_x, colMsg, sol_y, rowMsg, new[] { "North", "East", "South", "West" }[end_dir]);

            // Replace the asterisks in the map with the true cardinal direction
            for (var x = 0; x < WIDTH; x++)
                for (var y = 0; y < WIDTH; y++)
                    if (mapData[x, y].label == '*')
                        mapData[x, y].label = dirToChar(end_dir);

            // Add three decoy cardinal directions
            for (int i = 0; i < 3; i++)
            {
                int x = Random.Range(0, WIDTH);
                int y = Random.Range(0, WIDTH);
                while (mapData[x, y].label != ' ')
                {
                    x = Random.Range(0, WIDTH);
                    y = Random.Range(0, WIDTH);
                }
                mapData[x, y].label = dirToChar(decoy_dir);
            }

            pl_x = Random.Range(0, WIDTH);
            pl_y = Random.Range(0, WIDTH);
            pl_dir = Random.Range(0, 4);

            while (!getEdge(sol_x, sol_y, end_dir, 0, 0, end_dir))
            {
                sol_x = bound(sol_x + xMod(1, 0, end_dir), WIDTH);
                sol_y = bound(sol_y + yMod(1, 0, end_dir), WIDTH);
            }

            win_x1 = sol_x;
            win_y1 = sol_y;

            win_x2 = bound(sol_x + xMod(1, 0, end_dir), WIDTH);
            win_y2 = bound(sol_y + yMod(1, 0, end_dir), WIDTH);
        }

        private static Coordinate Neighbor(Coordinate cell, int direction)
        {
            switch (direction)
            {
                case NORTH: return new Coordinate(cell.X, (cell.Y + WIDTH - 1) % WIDTH);
                case EAST: return new Coordinate((cell.X + 1) % WIDTH, cell.Y);
                case SOUTH: return new Coordinate(cell.X, (cell.Y + 1) % WIDTH);
                default: return new Coordinate((cell.X + WIDTH - 1) % WIDTH, cell.Y);
            }
        }

        public bool GetNorthWall(int x, int y)
        {
            return mapData[x, y].n_wall;
        }

        public bool GetWestWall(int x, int y)
        {
            return mapData[x, y].w_wall;
        }

        public char GetLabel(int x, int y)
        {
            return mapData[x, y].label;
        }

        private char dirToChar(int dir)
        {
            switch (dir)
            {
                case NORTH:
                    return 'N';
                case SOUTH:
                    return 'S';
                case EAST:
                    return 'E';
                case WEST:
                    return 'W';
            }
            return '?';
        }
    }
}