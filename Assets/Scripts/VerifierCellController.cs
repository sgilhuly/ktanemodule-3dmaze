using UnityEngine;
using System.Collections;

public class VerifierCellController : MonoBehaviour {

	public GameObject North;
	public GameObject West;
	public GameObject Letter;

	public Material MatA;
	public Material MatB;
	public Material MatC;
	public Material MatD;
	public Material MatE;
	public Material MatH;
	public Material MatN;
	public Material MatS;
	public Material MatW;

	public void SetValues(bool n, bool w, char c) {
		North.SetActive (n);
		West.SetActive (w);
		Letter.SetActive (true);

		MeshRenderer mr = Letter.GetComponent<MeshRenderer> ();

		switch (c) {
		case 'A':
			mr.material = MatA;
			break;
		case 'B':
			mr.material = MatB;
			break;
		case 'C':
			mr.material = MatC;
			break;
		case 'D':
			mr.material = MatD;
			break;
		case 'E':
			mr.material = MatE;
			break;
		case 'H':
			mr.material = MatH;
			break;
		case 'N':
			mr.material = MatN;
			break;
		case 'S':
			mr.material = MatS;
			break;
		case 'W':
			mr.material = MatW;
			break;
		default:
			Letter.SetActive (false);
			break;
		}
	}
}
