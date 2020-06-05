using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using KModkit;
using ThreeDMaze;
using UnityEngine;

public class ThreeDMazeModule : MonoBehaviour
{
    public KMBombInfo BombInfo;
    public KMBombModule BombModule;
    public KMAudio KMAudio;
    public KMSelectable ButtonLeft;
    public KMSelectable ButtonRight;
    public KMSelectable ButtonStraight;
    public KMRuleSeedable RuleSeedable;

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

    private const int CodeLeft = 0;
    private const int CodeRight = 1;
    private const int CodeStraight = 2;

    private bool isActive = false;
    private bool isComplete = false;    // for Souvenir
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
        map = new Map(BombInfo.GetSerialNumber(), BombInfo.GetOnIndicators().ToArray(), BombInfo.GetOffIndicators().ToArray(), moduleId, RuleSeedable.GetRNG());
        UpdateDisplay(map.getDefaultMapView());
    }

    private bool HandlePress(int button)
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

    private void UpdateDisplay(MapView view)
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

    private KMSelectable[] ShortenDirection(string direction)
    {
        switch (direction)
        {
            case "l":
            case "left":
                return new[] { ButtonLeft };
            case "r":
            case "right":
                return new[] { ButtonRight };
            case "f":
            case "forward":
                return new[] { ButtonStraight };
            case "u":
            case "u-turn":
            case "uturn":
            case "turnaround":
            case "turn-around":
                return new[] { ButtonRight, ButtonRight };
            default:
                return new KMSelectable[] { null };
        }
    }

    private void TwitchHandleForcedSolve()
    {
        KMAudio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, this.transform);
        GetComponent<KMSelectable>().AddInteractionPunch(0.1f);
        BombModule.HandlePass();
        UpdateDisplay(new MapView());
        isComplete = true;
        Debug.LogFormat("[3D Maze #{0}] Module forcibly solved.", moduleId);
    }

#pragma warning disable 414 // Silence the compiler warning about an unused field (it’s used by TP via Reflection)
    private string TwitchHelpMessage = "Make a series of moves using !{0} move f f r f l f u. Walk a littler slower using !{0} walk r f u f f.  (Movements are l = Left, r = Right, f = Forward, and u = U-Turn.)";
#pragma warning restore 414

    private IEnumerator ProcessTwitchCommand(string inputCommand)
    {
        List<string> commands = inputCommand.ToLowerInvariant().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).ToList();

        if (commands.Count == 0) yield break;

        bool moving = false;

        if (ShortenDirection(commands[0])[0] == null)
        {
            switch (commands[0])
            {
                case "move":
                case "m":
                    moving = true;
                    goto case "walk";
                case "walk":
                case "w":
                    commands.RemoveAt(0);
                    break;
                default:
                    yield return "sendtochaterror valid commands are m = \'Move\' or w = \'Walk\'. Valid movements are l = \'Left\', r = \'Right\', f = \'Forward\', or u = \'U-turn\'.";
                    yield break;
            }
        }

        List<KMSelectable> moves = commands.SelectMany(ShortenDirection).ToList();
        if (!moves.Any())
        {
            yield return "sendtochaterror Please tell me a set of moves to walk me into the correct wall.";
            yield break;
        }
        if (moves.Any(m => m == null))
        {
            string invalidMove = commands.FirstOrDefault(x => ShortenDirection(x)[0] == null);
            if (!string.IsNullOrEmpty(invalidMove))
                yield return string.Format("sendtochaterror I don't know how to move in the direction of {0}.", invalidMove);
            yield break;
        }
        yield return null;

        if (moves.Count > (moving ? 64 : 16)) yield return "elevator music";

        float moveDelay = moving ? 0.1f : 0.4f;
        foreach (KMSelectable move in moves)
        {
            move.OnInteract();
            yield return "trycancel";
            yield return new WaitForSeconds(moveDelay);
        }
    }
}
