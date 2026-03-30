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

    void Start()
    {

    }

    private void Awake()
    {
        gridArray = new GameObject[columns, rows];
        if (gridPrefab) GenerateGrid();
        else Debug.LogError("Missing gridPrefab!");
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

    bool BlocksDirection(int x, int y, string side)
    {
        if (gridArray[x, y] == null) return true;

        Transform quad = gridArray[x, y].transform.Find("Quad");
        if (quad == null) return false;

        Texture tex = quad.GetComponent<Renderer>().material.mainTexture;
        if (tex == null) return false;

        string textureName = tex.name;

        string idStr = textureName.Replace("labyrinthblock", "");
        int id = -1;
        if (string.IsNullOrEmpty(idStr)) id = 0;
        else int.TryParse(idStr, out id);

        switch (side)
        {
            case "Top":
                if (id == 1 || id == 6 || id == 8 || id == 10 || id == 11 || id == 12 || id == 19 || id == 20 || id == 28 || id == 29 || id == 37) return true;
                break;
            case "Bottom":
                if (id == 3 || id == 5 || id == 9 || id == 10 || id == 11 || id == 12 || id == 13 || id == 23 || id == 24 || id == 27 || id == 30 || id == 40) return true;
                break;
            case "Left":
                if (id == 2 || id == 7 || id == 8 || id == 9 || id == 12 || id == 13 || id == 21 || id == 22 || id == 29 || id == 30 || id == 38) return true;
                break;
            case "Right":
                if (id == 4 || id == 5 || id == 6 || id == 7 || id == 11 || id == 13 || id == 25 || id == 26 || id == 27 || id == 28 || id == 39) return true;
                break;
        }

        return false;
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
        switch (direction)
        {
            case 4: // Attempting to move LEFT (Check x-1)
                if (x - 1 > -1 && gridArray[x - 1, y] && gridArray[x - 1, y].GetComponent<GridStat>().visited == step)
                {
                    // 1. Check Walls
                    if (BlocksDirection(x, y, "Left") || BlocksDirection(x - 1, y, "Right")) return false;

                    // 2. Check Occupancy (THE FIX)
                    // Only check occupancy if we are calculating distances (step == -1) 
                    // or finding the path (step > -1). 
                    if (IsOccupied(x - 1, y)) return false;

                    return true;
                }
                return false;

            case 3: // Attempting to move DOWN (Check y-1)
                if (y - 1 > -1 && gridArray[x, y - 1] && gridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                {
                    if (BlocksDirection(x, y, "Bottom") || BlocksDirection(x, y - 1, "Top")) return false;

                    if (IsOccupied(x, y - 1)) return false;

                    return true;
                }
                return false;

            case 2: // Attempting to move RIGHT (Check x+1)
                if (x + 1 < columns && gridArray[x + 1, y] && gridArray[x + 1, y].GetComponent<GridStat>().visited == step)
                {
                    if (BlocksDirection(x, y, "Right") || BlocksDirection(x + 1, y, "Left")) return false;

                    if (IsOccupied(x + 1, y)) return false;

                    return true;
                }
                return false;

            case 1: // Attempting to move UP (Check y+1)
                if (y + 1 < rows && gridArray[x, y + 1] && gridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                {
                    if (BlocksDirection(x, y, "Up") || BlocksDirection(x, y + 1, "Bottom")) return false;

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

            PlayerManager localPM = Mirror.NetworkClient.connection.identity.GetComponent<PlayerManager>();
            if (labScript.hasAuthority || localPM.IsMyTurn)
            {
                labScript.CmdMoveToTile(targetTile.name);

                if (labScript.card != null)
                {
                    labScript.card.GetComponent<ThisCard>().hasMoved = true;
                }
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

        string opposite = "";
        if (direction == "Right") opposite = "Left";
        if (direction == "Left") opposite = "Right";
        if (direction == "Top") opposite = "Bottom";
        if (direction == "Bottom") opposite = "Top";

        if (BlocksDirection(sourceX, sourceY, direction) || BlocksDirection(targetX, targetY, opposite))
        {
            return;
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
