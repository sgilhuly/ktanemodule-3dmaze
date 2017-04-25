using UnityEngine;
using System.Collections;

public class ThreeDMazeModule : MonoBehaviour
{

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
    private int moduleId;
    private static int moduleIdCounter = 1;

    private Map map;

    protected void Start()
    {
        // In Unity, we assigned all 8 possible letter materials to the MR_letter game object because otherwise they do not show up on Mac.
        // Here we reduce the number of materials back to one.
        MR_letter.materials = new[] { MatA };

        moduleId = moduleIdCounter++;

        UpdateDisplay(new MapView());

        BombModule.OnActivate += OnActivate;

        ButtonLeft.OnInteract += delegate () { HandlePress(CodeLeft); return false; };
        ButtonRight.OnInteract += delegate () { HandlePress(CodeRight); return false; };
        ButtonStraight.OnInteract += delegate () { HandlePress(CodeStraight); return false; };
    }

    protected void OnActivate()
    {
        isActive = true;

        string serialNum = BombInfo.GetSerialNumber();
        bool foundDigit = false;
        int firstDigit = 0;
        int lastDigit = 0;
        foreach (char c in serialNum)
        {
            if (c >= '0' && c <= '9')
            {
                if (!foundDigit)
                {
                    foundDigit = true;
                    firstDigit = c - '0';
                }
                lastDigit = c - '0';
            }
        }

        var rowMsg = firstDigit.ToString();
        int numUnlit = 0;
        foreach (string s in BombInfo.GetOffIndicators())
        {
            // MAZE GAMER
            if (isCommonLetter(s, "aegmrz"))
            {
                numUnlit++;
                rowMsg += " + " + s;
            }
        }
        if (numUnlit == 0)
            rowMsg = "no indicators";

        var colMsg = lastDigit.ToString();
        int numLit = 0;
        foreach (string s in BombInfo.GetOnIndicators())
        {
            // HELP I'M LOST
            if (isCommonLetter(s, "ehilmopst"))
            {
                numLit++;
                colMsg += " + " + s;
            }
        }
        if (numLit == 0)
            colMsg = "no indicators";

        int row = (firstDigit + numUnlit) % 8;
        int col = (lastDigit + numLit) % 8;

        map = new Map(col, row, colMsg, rowMsg, moduleId);
        UpdateDisplay(map.getDefaultMapView());
    }

    protected bool isCommonLetter(string a, string b)
    {
        b = b.ToLower();

        foreach (char c in a.ToLower())
        {
            if (c >= 'a' && c <= 'z' && b.Contains(c + ""))
            {
                return true;
            }
        }

        return false;
    }

    protected bool HandlePress(int button)
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.1f);

        if (!isActive)
        {
            BombModule.HandleStrike();
        }
        else if (!isComplete)
        {
            if (button == CodeLeft)
            {
                map.turnLeft();
                UpdateDisplay(map.getDefaultMapView());
            }

            if (button == CodeRight)
            {
                map.turnRight();
                UpdateDisplay(map.getDefaultMapView());
            }

            if (button == CodeStraight)
            {
                int result = map.moveForward();

                if (result == Map.RESULT_OK)
                {
                    UpdateDisplay(map.getDefaultMapView());
                }
                else if (result == Map.RESULT_FAILURE)
                {
                    Debug.LogFormat("[3D Maze #{0}] You walked into a wrong wall:\n{1}", moduleId, map.GetLog());
                    BombModule.HandleStrike();
                }
                else if (result == Map.RESULT_SUCCESS)
                {
                    BombModule.HandlePass();
                    UpdateDisplay(new MapView());
                    isComplete = true;
                    Debug.LogFormat("[3D Maze #{0}] Module solved.", moduleId);
                }
            }
        }

        return false;
    }

    protected void UpdateDisplay(MapView view)
    {
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

        switch (view.letter)
        {
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
        MeshFilter mf = MR_letter.gameObject.GetComponent<MeshFilter>();

        Vector2[] uvs = mf.mesh.uv;
        uvs[0] = new Vector2(0.0f, 0.0f);
        uvs[1] = new Vector2(1.0f, 1.0f);
        uvs[2] = new Vector2(1.0f, 0.0f);
        uvs[3] = new Vector2(0.0f, 1.0f);
        mf.mesh.uv = uvs;
    }
}
