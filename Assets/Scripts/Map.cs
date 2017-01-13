using UnityEngine;
using System.Collections;

class MapNode {
	public char label;
	public bool n_wall;
	public bool w_wall;

	public MapNode(char label, bool n_wall, bool w_wall) {
		this.label = label;
		this.n_wall = n_wall;
		this.w_wall = w_wall;
	}
}

public class Map {

	public const int WIDTH = 8;

	// Add 1 to go clockwise, subtract 1 to go ccw
	public const int NORTH = 0;
	public const int EAST = 1;
	public const int SOUTH = 2;
	public const int WEST = 3;

	public const int RESULT_OK = 0;
	public const int RESULT_SUCCESS = 1;
	public const int RESULT_FAILURE = 2;

	private MapNode[,] mapData = new MapNode[WIDTH, WIDTH];

	private int win_x1;
	private int win_x2;
	private int win_y1;
	private int win_y2;

	private int pl_x;
	private int pl_y;
	private int pl_dir;

	public void turnLeft() {
		pl_dir = bound (pl_dir - 1, 4);
	}

	public void turnRight() {
		pl_dir = bound (pl_dir + 1, 4);
	}

	public int moveForward() {
		if (!getEdge (pl_x, pl_y, pl_dir, 0, 0, pl_dir)) {
			pl_x = bound (pl_x + xMod (1, 0, pl_dir), WIDTH);
			pl_y = bound (pl_y + yMod (1, 0, pl_dir), WIDTH);

			//Debug.Log ("Player: R: " + pl_y + " C: " + pl_x + " D: " + pl_dir);

			return RESULT_OK;

		} else {
			int new_x = bound (pl_x + xMod (1, 0, pl_dir), WIDTH);
			int new_y = bound (pl_y + yMod (1, 0, pl_dir), WIDTH);

			if ((pl_x == win_x1 && pl_y == win_y1 && new_x == win_x2 && new_y == win_y2) ||
			    (pl_x == win_x2 && pl_y == win_y2 && new_x == win_x1 && new_y == win_y1)) {

				return RESULT_SUCCESS;
			} else {
				return RESULT_FAILURE;
			}
		}
	}

	public MapView getDefaultMapView() {
		return getMapView (pl_x, pl_y, pl_dir);
	}

	public MapView getMapView(int x, int y, int dir) {
		MapView view = new MapView ();

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

	private int xMod(int ahead, int right, int dir) {
		switch (dir) {
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

	private int yMod(int ahead, int right, int dir) {
		switch (dir) {
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

	private int bound(int a, int b) {
		a = a % b;
		if (a < 0) {
			a += b;
		}

		return a;
	}

	private bool getEdge(int x, int y, int dir, int ahead, int right, int facing) {
		dir = bound (dir, 4);

		x += xMod (ahead, right, facing);
		y += yMod (ahead, right, facing);

		if (dir == NORTH || dir == SOUTH) {
			if (dir == SOUTH) {
				y++;
			}

			return mapData [bound (x, WIDTH), bound (y, WIDTH)].n_wall;
		}

		if (dir == EAST || dir == WEST) {
			if (dir == EAST) {
				x++;
			}

			return mapData [bound (x, WIDTH), bound (y, WIDTH)].w_wall;
		}

		return false;
	}

	private char getLetter(int x, int y) {
		return mapData [bound (x, WIDTH), bound (y, WIDTH)].label;
	}

	public Map(int end_x, int end_y, int mapIndex) {

		string[] labels;
		string[] n_walls;
		string[] w_walls;

		Debug.Log ("3D Maze selected map " + mapIndex);
		switch (mapIndex) {

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

		int end_dir = Random.Range (0, 4);
		int decoy_dir = Random.Range (0, 3);
		if (decoy_dir >= end_dir) {
			decoy_dir++;
		}

		Debug.Log ("End: R: " + end_y + " C: " + end_x + " D: " + end_dir);

		for (int y = 0; y < WIDTH; y++) {
			for (int x = 0; x < WIDTH; x++) {
				char label = labels [y] [x];
				bool n_wall;
				bool w_wall;

				if (n_walls [y] [x] == '0') {
					n_wall = false;
				} else {
					n_wall = true;
				}

				if (w_walls [y] [x] == '0') {
					w_wall = false;
				} else {
					w_wall = true;
				}

				if (label == '*') {
					label = dirToChar (end_dir);
				}

				mapData [x, y] = new MapNode (label, n_wall, w_wall);
			}
		}

		// Add three decoys
		for (int i = 0; i < 3; i++) {
			int x = Random.Range (0, WIDTH);
			int y = Random.Range (0, WIDTH);
			while (mapData [x, y].label != ' ') {
				x = Random.Range (0, WIDTH);
				y = Random.Range (0, WIDTH);
			}
			mapData [x, y].label = dirToChar (decoy_dir);
		}

		pl_x = Random.Range (0, WIDTH);
		pl_y = Random.Range (0, WIDTH);
		pl_dir = Random.Range (0, 4);

		Debug.Log ("Player: R: " + pl_y + " C: " + pl_x + " D: " + pl_dir);

		while (!getEdge (end_x, end_y, end_dir, 0, 0, end_dir)) {
			end_x = bound (end_x + xMod (1, 0, end_dir), WIDTH);
			end_y = bound (end_y + yMod (1, 0, end_dir), WIDTH);
		}

		win_x1 = end_x;
		win_y1 = end_y;

		win_x2 = bound (end_x + xMod (1, 0, end_dir), WIDTH);
		win_y2 = bound (end_y + yMod (1, 0, end_dir), WIDTH);
	}

	public Map(int end_x, int end_y) : this(end_x, end_y, Random.Range (0, 10)) {
	}

	public bool GetNorthWall(int x, int y) {
		return mapData [x, y].n_wall;
	}

	public bool GetWestWall(int x, int y) {
		return mapData [x, y].w_wall;
	}

	public char GetLabel(int x, int y) {
		return mapData [x, y].label;
	}

	private char dirToChar(int dir) {
		switch (dir) {
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
