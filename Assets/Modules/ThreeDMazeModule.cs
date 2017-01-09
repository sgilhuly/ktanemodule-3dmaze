using UnityEngine;
using System.Collections;

public class ThreeDMazeModule : MonoBehaviour {

	public KMBombInfo BombInfo;
	public KMBombModule BombModule;
	public KMAudio KMAudio;
	public KMSelectable ButtonLeft;
	public KMSelectable ButtonRight;
	public KMSelectable ButtonStraight;

	public MeshRenderer MR_ble;
	public MeshRenderer MR_blw;
	public MeshRenderer MR_bre;
	public MeshRenderer MR_brw;
	public MeshRenderer MR_bw;
	public MeshRenderer MR_fle;
	public MeshRenderer MR_flw;
	public MeshRenderer MR_fre;
	public MeshRenderer MR_frw;
	public MeshRenderer MR_fw;

	public MeshRenderer MR_letter;
	public MeshRenderer MR_letter_far;

	public Material MatA;
	public Material MatB;
	public Material MatC;
	public Material MatD;
	public Material MatH;
	public Material MatE;
	public Material MatN;
	public Material MatS;
	public Material MatW;

	private int CodeLeft = 0;
	private int CodeRight = 1;
	private int CodeStraight = 2;

	private bool isActive = false;
	private bool isComplete = false;

	private Map map;

	protected void Start() {
		Debug.Log("3D Maze start!");
		UpdateDisplay (new MapView ());

		BombModule.OnActivate += OnActivate;
		//OnActivate();

		ButtonLeft.OnInteract += delegate () { HandlePress(CodeLeft); return false; };
		ButtonRight.OnInteract += delegate () { HandlePress(CodeRight); return false; };
		ButtonStraight.OnInteract += delegate () { HandlePress(CodeStraight); return false; };
	}

	protected void OnActivate() {
		isActive = true;

		string serialNum = KMBombInfoExtensions.GetSerialNumber (BombInfo);
		//string serialNum = "0abcd0";

		bool foundDigit = false;
		int firstDigit = 0;
		int lastDigit = 0;
		foreach (char c in serialNum) {
			if (c >= '0' && c <= '9') {
				if (!foundDigit) {
					foundDigit = true;
					firstDigit = c - '0';
				}
				lastDigit = c - '0';
			}
		}

		/*int numUnlit = Random.Range (0, 8);
		int numLit = Random.Range (0, 8);*/

		int numUnlit = 0;
		foreach (string s in KMBombInfoExtensions.GetOffIndicators (BombInfo)) {
			if (isCommonLetter (s, "aegmrz")) { // MAZE GAMER
				numUnlit++;
			}
		}

		int numLit = 0;
		foreach (string s in KMBombInfoExtensions.GetOnIndicators (BombInfo)) {
			if (isCommonLetter (s, "ehilmopst")) { // HELP I'M LOST
				numLit++;
			}
		}

		int row = (firstDigit + numUnlit) % 8;
		int col = (lastDigit + numLit) % 8;

		map = new Map (col, row);
		UpdateDisplay (map.getDefaultMapView ());
	}

	protected bool isCommonLetter(string a, string b) {
		b = b.ToLower ();

		foreach (char c in a.ToLower()) {
			if (c >= 'a' && c <= 'z' && b.Contains (c + "")) {
				return true;
			}
		}

		return false;
	}

	protected bool HandlePress(int button) {
		KMAudio.PlayGameSoundAtTransform (KMSoundOverride.SoundEffect.ButtonPress, this.transform);
		GetComponent<KMSelectable> ().AddInteractionPunch (0.1f);

		if (!isActive) {
			BombModule.HandleStrike ();
		} else if (!isComplete) {
			if (button == CodeLeft) {
				map.turnLeft ();
				UpdateDisplay (map.getDefaultMapView ());
			}

			if (button == CodeRight) {
				map.turnRight ();
				UpdateDisplay (map.getDefaultMapView ());
			}

			if (button == CodeStraight) {
				int result = map.moveForward ();

				if (result == Map.RESULT_OK) {
					UpdateDisplay (map.getDefaultMapView ());
				} else if (result == Map.RESULT_FAILURE) {
					BombModule.HandleStrike ();
					Debug.Log ("3D Maze Strike!");
				} else if (result == Map.RESULT_SUCCESS) {
					BombModule.HandlePass ();
					UpdateDisplay (new MapView ());
					isComplete = true;
					Debug.Log ("3D Maze Win!");
				}
			}
		}

		return false;
	}

	protected void UpdateDisplay(MapView view) {
		MR_ble.enabled = view.ble;
		MR_blw.enabled = view.blw;
		MR_bre.enabled = view.bre;
		MR_brw.enabled = view.brw;
		MR_bw.enabled = view.bw;

		MR_fle.enabled = view.fle;
		MR_flw.enabled = view.flw;
		MR_fre.enabled = view.fre;
		MR_frw.enabled = view.frw;
		MR_fw.enabled = view.fw;

		MR_letter_far.enabled = view.letter_far;
		MR_letter.enabled = true;

		switch (view.letter) {
		case 'A':
			MR_letter.material = MatA;
			break;
		case 'B':
			MR_letter.material = MatB;
			break;
		case 'C':
			MR_letter.material = MatC;
			break;
		case 'D':
			MR_letter.material = MatD;
			break;
		case 'H':
			MR_letter.material = MatH;
			break;
		case 'E':
			MR_letter.material = MatE;
			break;
		case 'N':
			MR_letter.material = MatN;
			break;
		case 'S':
			MR_letter.material = MatS;
			break;
		case 'W':
			MR_letter.material = MatW;
			break;
		default:
			MR_letter.enabled = false;
			break;
		}

		// Perform some wizardry because the game has very bizarre glitches
		MeshFilter mf = MR_letter.gameObject.GetComponent<MeshFilter> ();

		Vector2[] uvs = mf.mesh.uv;
		uvs [0] = new Vector2 (0.0f, 0.0f);
		uvs [1] = new Vector2 (1.0f, 1.0f);
		uvs [2] = new Vector2 (1.0f, 0.0f);
		uvs [3] = new Vector2 (0.0f, 1.0f);
		mf.mesh.uv = uvs;
	}

	/*void Update() {
		if (Input.GetKeyDown ("a")) {
			HandlePress (CodeLeft);
		}
		if (Input.GetKeyDown ("w")) {
			HandlePress (CodeStraight);
		}
		if (Input.GetKeyDown ("d")) {
			HandlePress (CodeRight);
		}
	}*/
}
