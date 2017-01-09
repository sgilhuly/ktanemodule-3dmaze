using UnityEngine;
using System.Collections;

public class VerifierController : MonoBehaviour {

	public GameObject Cell;
	public int Index;

	void Start () {
	
		Map map = new Map (0, 0, Index);

		GameObject cell;

		for (int x = 0; x < 8; x++) {
			for (int y = 0; y < 8; y++) {
				cell = Instantiate (Cell);
				cell.transform.position = new Vector3 (x, 0, -y);
				cell.GetComponent<VerifierCellController> ().SetValues (map.GetNorthWall (x, y), map.GetWestWall (x, y), map.GetLabel (x, y));
			}
		}
	}
}
