using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GridBehavior : MonoBehaviour
{
    public bool findDistance = false;
    public int rows = 16;
    public int columns = 11;
    public int scale = 1;
    public GameObject gridPrefab;
    public Vector3 leftBottomLocation = new Vector3(0, 0, 0);
    public GameObject[,] gridArray;
    public int startX = 0;
    public int startY = 0;
    public int endX = 2;
    public int endY = 2;
    public List<GameObject> path = new List<GameObject>();
    public GameObject objectToMove;
    public int spaces;
    public GameObject cardWaitingToSpawn;

    // Generated in Start, not Awake: Mirror's editor-only OnPostProcessScene runs
    // after Awake when entering play mode, and would flag these runtime-created
    // tiles as scene objects with no valid sceneId.
    void Start()
    {
        gridArray = new GameObject[columns, rows];
        if (gridPrefab) GenerateGrid();
        else Debug.LogError("Missing gridPrefab!");

        gridReady = true;

        // Every game gets its own random interior. The server rolls it here;
        // a joining client built its grid from the scene's authored reference
        // layout and asks the server for the real one once its local player
        // spawns (PlayerManager.CmdRequestMazeLayout), which may land before
        // or after this Start -- hence pendingLayout.
        if (Mirror.NetworkServer.active)
        {
            ApplyMazeLayout(RegenerateServerLayout());
        }
        else if (pendingLayout != null)
        {
            int[] layout = pendingLayout;
            pendingLayout = null;
            ApplyMazeLayout(layout);
        }
    }

    void Update()
    {
        if (findDistance && objectToMove != null)
        {
            SetDistance();
            SetPath();

            objectToMove.transform.SetParent(gridArray[endX, endY].transform);

            objectToMove.transform.localPosition = new Vector3(0, 0.5f, 0);

            startX = objectToMove.transform.parent.GetComponent<GridStat>().x;
            startY = objectToMove.transform.parent.GetComponent<GridStat>().y;
            findDistance = false;
            objectToMove = null;
        }
    }

    void GenerateGrid()
    {
        Transform refContainer = transform.Find("GridContainer");
        if (refContainer == null)
        {
            foreach (Transform child in transform)
            {
                if (child.name.Contains("GridContainer"))
                {
                    refContainer = child;
                    break;
                }
            }
        }

        if (refContainer != null) refContainer.gameObject.SetActive(false);

        if (gridPrefab == null)
        {
            Debug.LogError("Missing gridPrefab!");
            return;
        }

        gridArray = new GameObject[columns, rows];
        float spacing = 1f;

        bool isClient = Mirror.NetworkClient.active && !Mirror.NetworkServer.active;

        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                Vector3 pos;
                Quaternion rot;

                if (isClient)
                {
                    float physX = (columns - 1 - i) * spacing;
                    float physZ = (rows - 1 - j) * spacing;

                    pos = new Vector3(leftBottomLocation.x + physX, 0, leftBottomLocation.z + physZ);
                    rot = Quaternion.Euler(0, 180, 0);
                }
                else
                {
                    pos = new Vector3(leftBottomLocation.x + (i * spacing), 0, leftBottomLocation.z + (j * spacing));
                    rot = Quaternion.identity;
                }

                GameObject obj = Instantiate(gridPrefab, pos, rot);
                obj.transform.SetParent(this.transform);

                obj.name = "GridObject(" + i + "," + j + ")";

                GridStat stat = obj.GetComponent<GridStat>();
                if (stat != null)
                {
                    stat.x = i;
                    stat.y = j;
                }
                if (refContainer != null)
                {
                    Transform refTile = refContainer.transform.Find(obj.name);
                    if (refTile != null)
                    {
                        Texture texToCopy = null;
                        if (refTile.GetComponentInChildren<Renderer>() != null)
                            texToCopy = refTile.GetComponentInChildren<Renderer>().material.mainTexture;
                        else if (refTile.GetComponent<UnityEngine.UI.Image>() != null)
                            texToCopy = refTile.GetComponent<UnityEngine.UI.Image>().sprite.texture;

                        if (texToCopy != null)
                        {
                            Transform realQuad = obj.transform.Find("Quad");
                            if (realQuad != null)
                                realQuad.GetComponent<Renderer>().material.mainTexture = texToCopy;

                            string texName = texToCopy.name;
                            string idStr = texName.Replace("labyrinthblock", "");
                            int bID = 0;
                            if (!string.IsNullOrEmpty(idStr)) int.TryParse(idStr, out bID);

                            if (stat != null) stat.blockID = bID;
                        }
                    }
                }

                gridArray[i, j] = obj;
            }
        }
        if (refContainer != null)
        {
            Destroy(refContainer.gameObject);
        }
    }
    void SetDistance()
    {
        InitialSetUp();
        int x = startX;
        int y = startY;
        int[] testArray = new int[rows * columns];
        for (int step = 1; step < rows *columns; step++)
        {
            foreach (GameObject obj in gridArray)
            {
                if (obj&&obj.GetComponent<GridStat>().visited == step - 1)
                    TestFourDirections(obj.GetComponent<GridStat>().x, obj.GetComponent<GridStat>().y, step);
            }
        }
    }
    void SetPath()
    {
        int step;
        int x = endX;
        int y = endY;
        List<GameObject> tempList = new List<GameObject>();
        path.Clear();
        if(gridArray[endX,endY]&&gridArray[endX,endY].GetComponent<GridStat>().visited > 0)
        {
            path.Add(gridArray[x, y]);
            step = gridArray[x, y].GetComponent<GridStat>().visited - 1;
            Debug.Log("STEP IS " + step + 1);
            Debug.Log("SPACE IS " + spaces);
            if (step > spaces - 1)
            {
                print("Can't reach the desired location"+spaces+step);
                return;
            }

        }
        else
        {
            print("Can't reach the desired location");
            return;
        }
        for(int i = step; step > -1; step--)
        {
            if (TestDirection(x, y, step, 1))
                tempList.Add(gridArray[x, y + 1]);
            if (TestDirection(x, y, step, 2))
                tempList.Add(gridArray[x+1, y]);
            if (TestDirection(x, y, step, 3))
                tempList.Add(gridArray[x, y - 1]);
            if (TestDirection(x, y, step, 4))
                tempList.Add(gridArray[x-1, y]);
            GameObject tempObj = FindClosest(gridArray[endX, endY].transform, tempList);
            path.Add(tempObj);
            //tempObj.transform.GetComponent<Image>().sprite = Resources.Load<Sprite>("steppedblock");
            x = tempObj.GetComponent<GridStat>().x;
            y = tempObj.GetComponent<GridStat>().y;
            tempList.Clear();
        }
    }
    void InitialSetUp()
    {
        foreach(GameObject obj in gridArray)
        {
            obj.GetComponent<GridStat>().visited = -1;
        }
        gridArray[startX, startY].GetComponent<GridStat>().visited = 0;
    }

    // Which "labyrinthblockN" floor-texture IDs paint a wall on which side of
    // the tile. This is the single source of truth for the maze's actual
    // walkable structure -- there is no separate wall geometry, the picture
    // on the floor IS the collision data (see BlocksDirection below and
    // GenerateInteriorMazeLayout, which both key off these same sets so they
    // can never disagree with each other).
    static readonly HashSet<int> TopWallIds = new HashSet<int> { 1, 6, 8, 10, 11, 12, 19, 20, 28, 29, 37 };
    static readonly HashSet<int> BottomWallIds = new HashSet<int> { 3, 5, 9, 10, 11, 12, 13, 23, 24, 27, 30, 40 };
    static readonly HashSet<int> LeftWallIds = new HashSet<int> { 2, 7, 8, 9, 12, 13, 21, 22, 29, 30, 38 };
    static readonly HashSet<int> RightWallIds = new HashSet<int> { 4, 5, 6, 7, 11, 13, 25, 26, 27, 28, 39 };

    // Every "labyrinthblockN" id that exists as a Resources texture (0..45)
    // but isn't in any of the four sets above is a plain open floor tile --
    // several exist purely for visual variety among otherwise-identical
    // open cells.
    const int MaxBlockId = 45;

    int GetBlockId(int x, int y)
    {
        if (gridArray[x, y] == null) return -1;

        Transform quad = gridArray[x, y].transform.Find("Quad");
        if (quad == null) return -1;

        Texture tex = quad.GetComponent<Renderer>().material.mainTexture;
        if (tex == null) return -1;

        string idStr = tex.name.Replace("labyrinthblock", "");
        int id = -1;
        if (string.IsNullOrEmpty(idStr)) id = 0;
        else int.TryParse(idStr, out id);
        return id;
    }

    bool IsWallOnSide(int id, string side)
    {
        switch (side)
        {
            case "Top": return TopWallIds.Contains(id);
            case "Bottom": return BottomWallIds.Contains(id);
            case "Left": return LeftWallIds.Contains(id);
            case "Right": return RightWallIds.Contains(id);
        }
        return false;
    }

    bool BlocksDirection(int x, int y, string side)
    {
        if (gridArray[x, y] == null) return true;
        int id = GetBlockId(x, y);
        if (id < 0) return false;
        return IsWallOnSide(id, side);
    }

    // Which corners of each "labyrinthblockN" texture are painted black, in
    // the order top-left, top-right, bottom-left, bottom-right (decoded from
    // the PNGs). A corner is black either because one of the tile's own walls
    // runs through it, or as a "dot" continuing a neighbouring tile's wall
    // that ends at that corner -- the dots are what make walls look joined up.
    static readonly string[] BlockCorners =
    {
        "0000", "1100", "1010", "0011", "0101", "0111", "1101", "1111", "1110", "1011", // 0-9
        "1111", "1111", "1111", "1111", "1111", "1000", "0100", "0001", "0010", "1110", // 10-19
        "1101", "1110", "1011", "1011", "0111", "1101", "0111", "1111", "1111", "1111", // 20-29
        "1111", "1100", "1010", "1001", "0110", "0101", "0011", "1111", "1111", "1111", // 30-39
        "1111", "1110", "1101", "1011", "0111", "1111",                                 // 40-45
    };

    static Dictionary<(bool, bool, bool, bool, string), int> signatureToId;

    static Dictionary<(bool, bool, bool, bool, string), int> SignatureToId
    {
        get
        {
            if (signatureToId == null)
            {
                signatureToId = new Dictionary<(bool, bool, bool, bool, string), int>();
                for (int id = 0; id <= MaxBlockId; id++)
                {
                    // 14 walls nothing for movement (BlocksDirection) but is
                    // missing from Intercept's open-tile list
                    // (LabyrinthObject.CheckIfNextToWallOrInBase), so the two
                    // systems disagree about it -- keep it out of generated
                    // mazes so every generated cell reads the same to both.
                    if (id == 14) continue;

                    signatureToId[(TopWallIds.Contains(id), BottomWallIds.Contains(id), LeftWallIds.Contains(id), RightWallIds.Contains(id), BlockCorners[id])] = id;
                }
            }
            return signatureToId;
        }
    }

    // Walls plus which corners must be black. Returns -1 if no texture
    // matches -- the only gap is walled everywhere except the bottom (id 14,
    // excluded above); see the correction pass in GenerateInteriorMazeLayout.
    int PickBlockIdForSignature(bool wallTop, bool wallBottom, bool wallLeft, bool wallRight, bool cTL, bool cTR, bool cBL, bool cBR)
    {
        string corners = (cTL ? "1" : "0") + (cTR ? "1" : "0") + (cBL ? "1" : "0") + (cBR ? "1" : "0");
        return SignatureToId.TryGetValue((wallTop, wallBottom, wallLeft, wallRight, corners), out int id) ? id : -1;
    }

    void SetOpenSide(bool[,] top, bool[,] bottom, bool[,] left, bool[,] right, int x, int y, string side, bool value)
    {
        switch (side)
        {
            case "Top": top[x, y] = value; break;
            case "Bottom": bottom[x, y] = value; break;
            case "Left": left[x, y] = value; break;
            case "Right": right[x, y] = value; break;
        }
    }

    // The server's authoritative copy of the current interior layout (see
    // GenerateInteriorMazeLayout for the format). Tiles are plain local
    // Instantiate()s on every machine rather than network-spawned objects, so
    // nothing replicates them automatically -- the server decides a layout
    // and PlayerManager ships it to clients (RpcApplyMazeLayout for everyone
    // on Magical Labyrinth, TargetApplyMazeLayout for a client that joins
    // after the host already rolled the starting maze).
    int[] serverLayout;
    int[] pendingLayout;
    bool gridReady;

    public int[] CurrentServerLayout => serverLayout;

    public int[] RegenerateServerLayout()
    {
        serverLayout = GenerateInteriorMazeLayout();
        return serverLayout;
    }

    // Applies a layout from GenerateInteriorMazeLayout to this machine's own
    // tiles: swaps each tile's floor texture (which is what BlocksDirection
    // reads) and its blockID (which Intercept's wall check reads). Monsters
    // are parented to the tile GameObjects themselves, which are never
    // replaced, so they stay exactly where they are.
    public void ApplyMazeLayout(int[] layout)
    {
        if (layout == null || layout.Length != columns * rows) return;

        if (!gridReady)
        {
            pendingLayout = layout;
            return;
        }

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                int id = layout[x * rows + y];
                if (id < 0 || gridArray[x, y] == null) continue;

                Texture2D tex = Resources.Load<Texture2D>(id == 0 ? "labyrinthblock" : "labyrinthblock" + id);
                if (tex == null)
                {
                    Debug.LogError($"ApplyMazeLayout: missing Resources texture for block id {id}");
                    continue;
                }

                Transform quad = gridArray[x, y].transform.Find("Quad");
                if (quad != null) quad.GetComponent<Renderer>().material.mainTexture = tex;

                GridStat stat = gridArray[x, y].GetComponent<GridStat>();
                if (stat != null) stat.blockID = id;
            }
        }
    }

    // Builds a fresh random maze for the interior (rows 1-14; the two Card
    // Base rows, 0 and 15, keep their authored layout so summoning/tribute
    // zones are never accidentally walled off). Returns a flat array indexed
    // [x * rows + y] holding the block id to paint on each tile, with -1 for
    // tiles to leave unchanged. Pure: it doesn't touch any tile itself --
    // only the server should call it, then ApplyMazeLayout on every machine.
    public int[] GenerateInteriorMazeLayout()
    {
        int minY = 1, maxY = rows - 2; // interior rows; 0 and rows-1 are Card Base, untouched
        int midY = rows / 2 - 1;       // last row of the bottom player's half
        bool[,] visited = new bool[columns, rows];
        bool[,] openTop = new bool[columns, rows];
        bool[,] openBottom = new bool[columns, rows];
        bool[,] openLeft = new bool[columns, rows];
        bool[,] openRight = new bool[columns, rows];

        // Fairness: the board is 180-degree rotationally symmetric, so each
        // player faces exactly the same maze from their own Card Base (the
        // authored base rows are already symmetric this way). Every opening
        // is applied together with its rotated twin.
        System.Action<int, int, string> openEdge = (x, y, side) =>
        {
            int nx = x, ny = y;
            string back;
            switch (side)
            {
                case "Top": ny = y + 1; back = "Bottom"; break;
                case "Bottom": ny = y - 1; back = "Top"; break;
                case "Left": nx = x - 1; back = "Right"; break;
                default: nx = x + 1; back = "Left"; break;
            }
            SetOpenSide(openTop, openBottom, openLeft, openRight, x, y, side, true);
            SetOpenSide(openTop, openBottom, openLeft, openRight, nx, ny, back, true);
            SetOpenSide(openTop, openBottom, openLeft, openRight, columns - 1 - x, rows - 1 - y, back, true);
            SetOpenSide(openTop, openBottom, openLeft, openRight, columns - 1 - nx, rows - 1 - ny, side, true);
        };

        // Randomized recursive backtracker over the bottom half only -- a
        // spanning tree, so every cell of that half is reachable. openEdge
        // copies each carve into the top half, making it a spanning tree too.
        var stack = new List<(int x, int y)>();
        int startX = UnityEngine.Random.Range(0, columns);
        int startY = UnityEngine.Random.Range(minY, midY + 1);
        visited[startX, startY] = true;
        stack.Add((startX, startY));

        while (stack.Count > 0)
        {
            (int cx, int cy) = stack[stack.Count - 1];

            var candidates = new List<(int nx, int ny, string side)>();
            if (cy + 1 <= midY && !visited[cx, cy + 1]) candidates.Add((cx, cy + 1, "Top"));
            if (cy - 1 >= minY && !visited[cx, cy - 1]) candidates.Add((cx, cy - 1, "Bottom"));
            if (cx - 1 >= 0 && !visited[cx - 1, cy]) candidates.Add((cx - 1, cy, "Left"));
            if (cx + 1 < columns && !visited[cx + 1, cy]) candidates.Add((cx + 1, cy, "Right"));

            if (candidates.Count == 0)
            {
                stack.RemoveAt(stack.Count - 1);
                continue;
            }

            (int nx, int ny, string side) = candidates[UnityEngine.Random.Range(0, candidates.Count)];
            openEdge(cx, cy, side);
            visited[nx, ny] = true;
            stack.Add((nx, ny));
        }

        // Join the two halves across the middle (the twin crossing is added
        // automatically, so there are one or two symmetric passages).
        openEdge(UnityEngine.Random.Range(0, columns), midY, "Top");

        // Loosen the perfect maze: a per-game share of the remaining walls is
        // knocked out (creating loops), plus a few small open rooms, so boards
        // range from twisty to fairly open. Only the bottom half's edges are
        // rolled; their twins cover the top half.
        float openness = UnityEngine.Random.Range(0.12f, 0.32f);
        for (int x = 0; x < columns; x++)
        {
            for (int y = minY; y <= midY; y++)
            {
                if (!openTop[x, y] && UnityEngine.Random.value < openness) openEdge(x, y, "Top");
                if (x + 1 < columns && !openRight[x, y] && UnityEngine.Random.value < openness) openEdge(x, y, "Right");
            }
        }

        int rooms = UnityEngine.Random.Range(1, 3);
        for (int r = 0; r < rooms; r++)
        {
            int w = UnityEngine.Random.Range(2, 4);
            int h = UnityEngine.Random.Range(2, 4);
            int rx = UnityEngine.Random.Range(0, columns - w + 1);
            int ry = UnityEngine.Random.Range(minY + 1, maxY - h + 1);
            for (int x = rx; x < rx + w; x++)
            {
                for (int y = ry; y < ry + h; y++)
                {
                    if (x + 1 < rx + w) openEdge(x, y, "Right");
                    if (y + 1 < ry + h) openEdge(x, y, "Top");
                }
            }
        }

        // The rows touching each Card Base mirror its openings exactly, so
        // every wall along the base edge is drawn on both sides (as in the
        // authored board) and every base entrance leads into the maze.
        for (int x = 0; x < columns; x++)
        {
            openBottom[x, minY] = !IsWallOnSide(GetBlockId(x, 0), "Top");
            openTop[x, maxY] = !IsWallOnSide(GetBlockId(x, rows - 1), "Bottom");
        }

        // labyrinthblock's texture set has no tile walled on every side
        // except the bottom (a dead end that only opens downward) -- if the
        // carving above produced one, open one more random side so it
        // becomes a representable (still fully connected) cell instead of
        // silently picking a texture that would show the wrong walls.
        for (int x = 0; x < columns; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                bool onlyOpenIsBottom = openBottom[x, y] && !openTop[x, y] && !openLeft[x, y] && !openRight[x, y];
                if (!onlyOpenIsBottom) continue;

                var extra = new List<string>();
                if (y + 1 <= maxY) extra.Add("Top");
                if (x - 1 >= 0) extra.Add("Left");
                if (x + 1 < columns) extra.Add("Right");
                if (extra.Count == 0) continue; // not reachable inside an 11-wide interior

                openEdge(x, y, extra[UnityEngine.Random.Range(0, extra.Count)]);
            }
        }

        // Card Base rows keep their walls; read them from the current tiles so
        // the corner-dot pass below sees the whole board.
        foreach (int baseY in new[] { 0, rows - 1 })
        {
            for (int x = 0; x < columns; x++)
            {
                int id = GetBlockId(x, baseY);
                openTop[x, baseY] = !IsWallOnSide(id, "Top");
                openBottom[x, baseY] = !IsWallOnSide(id, "Bottom");
                openLeft[x, baseY] = !IsWallOnSide(id, "Left");
                openRight[x, baseY] = !IsWallOnSide(id, "Right");
            }
        }

        // A wall segment exists if either tile sharing it draws it.
        System.Func<int, int, string, bool> wallAt = (x, y, side) =>
        {
            if (x < 0 || y < 0 || x >= columns || y >= rows) return false;
            switch (side)
            {
                case "Top": return !openTop[x, y] || (y + 1 < rows && !openBottom[x, y + 1]);
                case "Bottom": return !openBottom[x, y] || (y - 1 >= 0 && !openTop[x, y - 1]);
                case "Left": return !openLeft[x, y] || (x - 1 >= 0 && !openRight[x - 1, y]);
                case "Right": return !openRight[x, y] || (x + 1 < columns && !openLeft[x + 1, y]);
            }
            return false;
        };

        int[] layout = new int[columns * rows];
        for (int i = 0; i < layout.Length; i++) layout[i] = -1;

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                bool wallTop = !openTop[x, y];
                bool wallBottom = !openBottom[x, y];
                bool wallLeft = !openLeft[x, y];
                bool wallRight = !openRight[x, y];

                // A corner is black if this tile's own wall covers it, or if
                // any neighbouring wall ends at that grid point.
                bool sTop = wallAt(x, y, "Top"), sBottom = wallAt(x, y, "Bottom");
                bool sLeft = wallAt(x, y, "Left"), sRight = wallAt(x, y, "Right");
                bool cTL = sTop || sLeft || wallAt(x - 1, y, "Top") || wallAt(x, y + 1, "Left");
                bool cTR = sTop || sRight || wallAt(x + 1, y, "Top") || wallAt(x, y + 1, "Right");
                bool cBL = sBottom || sLeft || wallAt(x - 1, y, "Bottom") || wallAt(x, y - 1, "Left");
                bool cBR = sBottom || sRight || wallAt(x + 1, y, "Bottom") || wallAt(x, y - 1, "Right");

                int id = PickBlockIdForSignature(wallTop, wallBottom, wallLeft, wallRight, cTL, cTR, cBL, cBR);
                if (id < 0)
                {
                    if (y >= minY && y <= maxY)
                        Debug.LogError($"GenerateInteriorMazeLayout: no texture for T{wallTop} B{wallBottom} L{wallLeft} R{wallRight} at ({x},{y}) -- leaving that tile unchanged.");
                    continue;
                }

                layout[x * rows + y] = id;
            }
        }

        return layout;
    }
    public void ShowSummonZone(GameObject card)
    {
        cardWaitingToSpawn = card;
        HighlightRange(false);

        int row = Mirror.NetworkServer.active ? 0 : 15;

        for (int col = 2; col <= 8; col++)
        {
            if (gridArray[col, row] != null)
            {
                gridArray[col, row].GetComponent<LabyrinthTile>().GlowBlock();
            }
        }
    }

    public void OnTileClicked(int x, int y)
    {
        if (cardWaitingToSpawn != null)
        {
            int validRow = Mirror.NetworkServer.active ? 0 : 15;
            if (y == validRow && x >= 2 && x <= 8)
            {
                int cardId = cardWaitingToSpawn.GetComponent<ThisCard>().thisId;
                NetworkIdentity ni = cardWaitingToSpawn.GetComponent<NetworkIdentity>();
                string tileName = gridArray[x, y].name;
                PlayerManager pm = Mirror.NetworkClient.connection.identity.GetComponent<PlayerManager>();
                pm.CmdSpawnMonster(cardId, tileName, ni);
                pm.CompleteSummonSequence();

                HighlightRange(false);
                cardWaitingToSpawn = null;
            }
            else
            {
                Debug.Log("Invalid Spawn Tile! Pick a green tile.");
            }
        }
        else if (objectToMove != null)
        {
            GridStat clickedStat = gridArray[x, y].GetComponent<GridStat>();

            if (clickedStat.visited > 0 && clickedStat.visited <= spaces)
            {
                FindDistanceTrue(x, y);
            }
            else if (clickedStat.visited == 999)
            {
                LabyrinthObject attacker = objectToMove.GetComponent<LabyrinthObject>();
                LabyrinthObject defender = gridArray[x, y].GetComponentInChildren<LabyrinthObject>();

                if (attacker != null && defender != null)
                {
                    attacker.CmdAttackMonster(defender.gameObject);

                    HighlightRange(false);
                    objectToMove = null;
                }
            }
            else if (clickedStat.visited == 888)
            {
                LabyrinthObject attacker = objectToMove.GetComponent<LabyrinthObject>();
                attacker.CmdDirectAttack();

                HighlightRange(false);
                objectToMove = null;
            }
        }
    }
    bool TestDirection(int x, int y, int step, int direction)
    {
        bool hasWallwalk = false;
        if (objectToMove != null)
        {
            LabyrinthObject labScript = objectToMove.GetComponent<LabyrinthObject>();
            if (labScript != null && labScript.card != null)
            {
                ThisCard cardData = labScript.card.GetComponent<ThisCard>();
                if (cardData != null && (cardData.cardProperty == Property.Wallwalk || cardData.grantedWallwalk))
                {
                    hasWallwalk = true;
                }
            }
        }

        switch (direction)
        {
            case 4: // Attempting to move LEFT (Check x-1)
                if (x - 1 > -1 && gridArray[x - 1, y] && gridArray[x - 1, y].GetComponent<GridStat>().visited == step)
                {
                    if (!hasWallwalk)
                    {
                        if (BlocksDirection(x, y, "Left") || BlocksDirection(x - 1, y, "Right")) return false;
                    }

                    if (IsOccupied(x - 1, y)) return false;
                    return true;
                }
                return false;

            case 3: // Attempting to move DOWN (Check y-1)
                if (y - 1 > -1 && gridArray[x, y - 1] && gridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                {
                    if (!hasWallwalk)
                    {
                        if (BlocksDirection(x, y, "Bottom") || BlocksDirection(x, y - 1, "Top")) return false;
                    }

                    if (IsOccupied(x, y - 1)) return false;
                    return true;
                }
                return false;

            case 2: // Attempting to move RIGHT (Check x+1)
                if (x + 1 < columns && gridArray[x + 1, y] && gridArray[x + 1, y].GetComponent<GridStat>().visited == step)
                {
                    if (!hasWallwalk)
                    {
                        if (BlocksDirection(x, y, "Right") || BlocksDirection(x + 1, y, "Left")) return false;
                    }

                    if (IsOccupied(x + 1, y)) return false;
                    return true;
                }
                return false;

            case 1: // Attempting to move UP (Check y+1)
                if (y + 1 < rows && gridArray[x, y + 1] && gridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                {
                    if (!hasWallwalk)
                    {
                        if (BlocksDirection(x, y, "Up") || BlocksDirection(x, y + 1, "Bottom")) return false;
                    }
                    if (IsOccupied(x, y + 1)) return false;
                    return true;
                }
                return false;
        }
        return false;
    }
    void TestFourDirections(int x,int y, int step)
    {
        if (TestDirection(x, y, -1, 1))
            SetVisited(x, y + 1, step);
        if (TestDirection(x, y, -1, 2))
            SetVisited(x + 1, y, step);
        if (TestDirection(x, y, -1, 3))
            SetVisited(x, y - 1, step);
        if (TestDirection(x, y, -1, 4))
            SetVisited(x - 1, y, step);
    }
     void SetVisited (int x, int y, int step)
    {
        if (gridArray[x, y])
            gridArray[x, y].GetComponent<GridStat>().visited = step;
    }
    GameObject FindClosest(Transform targetLocation, List<GameObject> list)
    {
        float currentDistance = scale * rows * columns;
        int indexNumber = 0;
        for (int i = 0; i < list.Count; i++)
        {
            if (Vector3.Distance(targetLocation.position, list[i].transform.position) < currentDistance)
            {
                currentDistance = Vector3.Distance(targetLocation.position, list[i].transform.position);
                indexNumber = i;
            }
        }
        return list[indexNumber];
    }


    /*  Up,Down,Right,Left: 0, 15, 16, 17 ,18, 31, 32, 33, 34, 35, 36, 41, 42, 43, 44, 45, 46
     *  Up,Down,Right: 2, 21, 22, 38
     *  Up,Down,Left: 4, 25, 26, 39
     *  Up,Right,Left: 3, 23, 24, 40
     *  Down,Right,Left: 1, 19, 20, 37
     *  Up,Down: 7, 
     *  Up,Right: 9, 30
     *  Up,Left: 5, 27
     *  Down,Right: 8, 29
     *  Down,Left: 6, 28
     *  Right,Left: 10, 
     *  Up: 13, 
     *  Down: 14, 
     *  Right: 12, 
     *  Left: 11, 
     */

    public void FindDistanceTrue(int ENDX, int ENDY)
    {
        if (objectToMove == null) return;

        GameObject targetTile = gridArray[ENDX, ENDY];
        int dist = targetTile.GetComponent<GridStat>().visited;

        if (dist > 0 && dist <= spaces)
        {
            LabyrinthObject labScript = objectToMove.GetComponent<LabyrinthObject>();
            PlayerManager myPM = NetworkClient.connection.identity.GetComponent<PlayerManager>();

            if (labScript.hasAuthority)
            {
                labScript.CmdMoveToTile(targetTile.name);
            }
            else if (myPM != null && myPM.IsMyTurn)
            {
                myPM.CmdForceMoveMonster(objectToMove, targetTile.name);
            }

            if (labScript.card != null)
            {
                labScript.card.GetComponent<ThisCard>().hasMoved = true;
            }

            HighlightRange(false);
            objectToMove = null;
        }
    }

    public void HighlightRange(bool shouldShow)
    {
        SetDistance();

        foreach (GameObject obj in gridArray)
        {
            if (obj == null) continue;

            GridStat stat = obj.GetComponent<GridStat>();
            LabyrinthTile tile = obj.GetComponent<LabyrinthTile>();

            if (shouldShow)
            {
                if (stat.visited > 0 && stat.visited <= spaces)
                {
                    tile.GlowBlock();
                }
                else
                {
                    tile.StopGlowBlock();
                }
            }
            else
            {
                tile.StopGlowBlock();
            }
        }
    }

    public void ShowPossiblePaths(GameObject labyrinthObject, int forcedRange = -1)
    {
        HighlightRange(false);
        objectToMove = labyrinthObject;

        GridStat currentStat = objectToMove.transform.parent.GetComponent<GridStat>();
        startX = currentStat.x;
        startY = currentStat.y;

        LabyrinthObject labScript = labyrinthObject.GetComponent<LabyrinthObject>();

        if (forcedRange != -1)
        {
            spaces = forcedRange;
        }
        else
        {
            spaces = labScript.moveRange;
        }

        HighlightRange(true);
        CheckAttackableNeighbors(startX, startY);
    }

    void CheckAttackableNeighbors(int x, int y)
    {
        CheckForEnemy(x, y, x + 1, y, "Right");
        CheckForEnemy(x, y, x - 1, y, "Left");
        CheckForEnemy(x, y, x, y + 1, "Top");
        CheckForEnemy(x, y, x, y - 1, "Bottom");
    }

    void CheckForEnemy(int sourceX, int sourceY, int targetX, int targetY, string direction)
    {
        if (targetX < 0 || targetX >= columns || targetY < 0 || targetY >= rows) return;

        if (objectToMove == null) return;

        string opposite = "";
        if (direction == "Right") opposite = "Left";
        if (direction == "Left") opposite = "Right";
        if (direction == "Top") opposite = "Bottom";
        if (direction == "Bottom") opposite = "Top";

        LabyrinthObject attacker = objectToMove.GetComponent<LabyrinthObject>();
        ThisCard attackerCard = attacker.card.GetComponent<ThisCard>();

        if (attackerCard.cardProperty != Property.Wallwalk && !attackerCard.grantedWallwalk)
        {
            if (BlocksDirection(sourceX, sourceY, direction) || BlocksDirection(targetX, targetY, opposite))
            {
                return;
            }
        }

        GameObject tile = gridArray[targetX, targetY];
        if (tile == null) return;

        LabyrinthObject enemyMonster = tile.GetComponentInChildren<LabyrinthObject>();

        if (enemyMonster != null)
        {
            // Monster vs Monster
            if (enemyMonster.hasAuthority != objectToMove.GetComponent<LabyrinthObject>().hasAuthority)
            {
                tile.GetComponent<LabyrinthTile>().RedGlowBlock();
                tile.GetComponent<GridStat>().visited = 999;
            }
        }
        else
        {
            // Direct Attack Check
            int targetRow = Mirror.NetworkServer.active ? 15 : 0;

            if (targetY == targetRow)
            {
                // Only allow Center Columns (3-9, which is Index 2-8)
                if (targetX >= 2 && targetX <= 8)
                {
                    tile.GetComponent<LabyrinthTile>().RedGlowBlock();
                    tile.GetComponent<GridStat>().visited = 888;
                }
            }
        }
    }

    bool IsOccupied(int x, int y)
    {
        if (gridArray[x, y] == null) return true;
        LabyrinthObject monster = gridArray[x, y].GetComponentInChildren<LabyrinthObject>();

        if (monster != null)
        {
            if (monster.gameObject != objectToMove)
            {
                return true;
            }
        }
        return false;
    }

    public bool HighlightAttackOptions(LabyrinthObject attacker)
    {
        bool foundTarget = false;

        GridStat currentStat = attacker.GetComponentInParent<GridStat>();
        if (currentStat == null) return false;

        int x = currentStat.x;
        int y = currentStat.y;

        // Check 4 directions with Wall Checks
        CheckAndHighlight(x, y, x + 1, y, "Right", attacker, ref foundTarget);
        CheckAndHighlight(x, y, x - 1, y, "Left", attacker, ref foundTarget);
        CheckAndHighlight(x, y, x, y + 1, "Top", attacker, ref foundTarget);
        CheckAndHighlight(x, y, x, y - 1, "Bottom", attacker, ref foundTarget);

        return foundTarget;
    }

    void CheckAndHighlight(int sourceX, int sourceY, int targetX, int targetY, string direction, LabyrinthObject attacker, ref bool foundTarget)
    {
        if (targetX >= 0 && targetX < columns && targetY >= 0 && targetY < rows)
        {
            string opposite = "";
            if (direction == "Right") opposite = "Left";
            if (direction == "Left") opposite = "Right";
            if (direction == "Top") opposite = "Bottom";
            if (direction == "Bottom") opposite = "Top";

            if (BlocksDirection(sourceX, sourceY, direction) || BlocksDirection(targetX, targetY, opposite))
            {
                return; // Blocked by wall
            }

            GameObject tile = gridArray[targetX, targetY];
            if (tile == null) return;

            LabyrinthObject targetMonster = tile.GetComponentInChildren<LabyrinthObject>();

            if (targetMonster != null)
            {
                if (targetMonster.hasAuthority != attacker.hasAuthority)
                {
                    Transform quad = tile.transform.Find("Quad");
                    if (quad != null)
                    {
                        quad.GetComponent<Renderer>().material.color = Color.red;
                    }
                    foundTarget = true;
                }
            }
            else
            {
                int baseRow = attacker.isClientOnly ? 0 : 15;

                int targetRow = Mirror.NetworkServer.active ? 15 : 0;

                if (targetY == targetRow)
                {
                    if (targetX >= 2 && targetX <= 8)
                    {
                        Transform quad = tile.transform.Find("Quad");
                        if (quad != null) quad.GetComponent<Renderer>().material.color = Color.red;
                        foundTarget = true;
                    }
                }
            }
        }
    }

    void CheckAndHighlight(int checkX, int checkY, LabyrinthObject attacker, ref bool foundTarget)
    {
        if (checkX >= 0 && checkX < columns && checkY >= 0 && checkY < rows)
        {
            GameObject tile = gridArray[checkX, checkY];
            if (tile == null) return;

            LabyrinthObject targetMonster = tile.GetComponentInChildren<LabyrinthObject>();

            if (targetMonster != null)
            {
                if (targetMonster.hasAuthority != attacker.hasAuthority)
                {
                    Transform quad = tile.transform.Find("Quad");
                    if (quad != null)
                    {
                        quad.GetComponent<Renderer>().material.color = Color.red;
                    }
                    foundTarget = true;
                }
            }
        }
    }

    public void ResetTileColors()
    {
        foreach (GameObject tile in gridArray)
        {
            if (tile != null)
            {
                Transform quad = tile.transform.Find("Quad");
                if (quad != null)
                {
                    quad.GetComponent<Renderer>().material.color = Color.white;
                }
            }
        }
    }
}
