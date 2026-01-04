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

    void Start()
    {
        if (Mirror.NetworkClient.active && !Mirror.NetworkServer.active)
        {
            transform.localRotation = Quaternion.Euler(0, 0, 180);
            RectTransform rect = GetComponent<RectTransform>();
            float targetX = leftBottomLocation.x - 40;
            rect.anchoredPosition = new Vector2(targetX, rect.anchoredPosition.y);
        }
    }

    private void Awake()
    {
        gridArray = new GameObject[columns, rows];
        if (gridPrefab) GenerateGrid();
        else Debug.LogError("Missing gridPrefab!");
    }

    // Update is called once per frame
    void Update()
    {
        if (findDistance && objectToMove != null)
        {
            SetDistance();
            SetPath();
            objectToMove.transform.SetParent(gridArray[endX, endY].transform);
            objectToMove.transform.position = objectToMove.transform.parent.position;
            startX = objectToMove.transform.parent.GetComponent<GridStat>().x;
            startY = objectToMove.transform.parent.GetComponent<GridStat>().y;
            findDistance = false;
            objectToMove = null;
        }
        //Debug.Log(startX + " " + startY);
    }

    void GenerateGrid()
    {
        int k = 0;
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                GameObject obj = Instantiate(gridPrefab, new Vector3(leftBottomLocation.x + scale * i*50, leftBottomLocation.y + scale * j * 50, leftBottomLocation.z + scale * j * 50),Quaternion.identity);
                obj.transform.SetParent(gameObject.transform);
                obj.GetComponent<GridStat>().x = i;
                obj.GetComponent<GridStat>().y = j;
                obj.name = "GridObject(" + i.ToString() + "," + j.ToString() + ")";
                obj.GetComponent<Image>().sprite = transform.Find("GridContainer").Find(obj.name).gameObject.GetComponent<Image>().sprite;
                Debug.Log(obj.GetComponent<Image>().sprite);
                gridArray[i, j] = obj;
                k++;
            }
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

        Sprite s = gridArray[x, y].GetComponent<Image>().sprite;
        if (s == null) return false;

        string spriteName = s.name;
        string idStr = spriteName.Replace("labyrinthblock", "");
        int id = -1;
        if (string.IsNullOrEmpty(idStr)) id = 0; // The base "labyrinthblock"
        else int.TryParse(idStr, out id);

        switch (side)
        {
            case "Top":
                // 1, 6, 8, 10, 11, 12, 19, 20, 28, 29, 37
                if (id == 1 || id == 6 || id == 8 || id == 10 || id == 11 || id == 12 || id == 19 || id == 20 || id == 28 || id == 29 || id == 37) return true;
                break;
            case "Bottom":
                // 3, 5, 9, 10, 11, 12, 13, 23, 24, 27, 30, 40
                if (id == 3 || id == 5 || id == 9 || id == 10 || id == 11 || id == 12 || id == 13 || id == 23 || id == 24 || id == 27 || id == 30 || id == 40) return true;
                break;
            case "Left":
                // 2, 7, 8, 9, 12, 13, 21, 22, 29, 30, 38
                if (id == 2 || id == 7 || id == 8 || id == 9 || id == 12 || id == 13 || id == 21 || id == 22 || id == 29 || id == 30 || id == 38) return true;
                break;
            case "Right":
                // 4, 5, 6, 7, 11, 13, 25, 26, 27, 28, 39
                if (id == 4 || id == 5 || id == 6 || id == 7 || id == 11 || id == 13 || id == 25 || id == 26 || id == 27 || id == 28 || id == 39) return true;
                break;
        }

        return false;
    }

    bool TestDirection(int x, int y, int step, int direction)
    {
        // direction: 1 is up, 2 is right, 3 is down, 4 is left
        switch (direction)
        {
            case 4: // Attempting to move LEFT
                if (x - 1 > -1 && gridArray[x - 1, y] && gridArray[x - 1, y].GetComponent<GridStat>().visited == step)
                {
                    // Current tile cannot block Left AND Next tile cannot block Right
                    if (!BlocksDirection(x, y, "Left") && !BlocksDirection(x - 1, y, "Right"))
                        return true;
                }
                return false;

            case 3: // Attempting to move DOWN
                if (y - 1 > -1 && gridArray[x, y - 1] && gridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                {
                    // Current tile cannot block Bottom AND Next tile cannot block Top
                    if (!BlocksDirection(x, y, "Bottom") && !BlocksDirection(x, y - 1, "Top"))
                        return true;
                }
                return false;

            case 2: // Attempting to move RIGHT
                if (x + 1 < columns && gridArray[x + 1, y] && gridArray[x + 1, y].GetComponent<GridStat>().visited == step)
                {
                    // Current tile cannot block Right AND Next tile cannot block Left
                    if (!BlocksDirection(x, y, "Right") && !BlocksDirection(x + 1, y, "Left"))
                        return true;
                }
                return false;

            case 1: // Attempting to move UP
                if (y + 1 < rows && gridArray[x, y + 1] && gridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                {
                    // Current tile cannot block Top AND Next tile cannot block Bottom
                    if (!BlocksDirection(x, y, "Up") && !BlocksDirection(x, y + 1, "Bottom"))
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
            // 1. Get the script
            LabyrinthObject labScript = objectToMove.GetComponent<LabyrinthObject>();

            // 2. ONLY the owner sends the command to move
            if (labScript.hasAuthority)
            {
                labScript.CmdMoveToTile(targetTile.name);

                // Handle local logic (turn usage)
                if (labScript.card != null)
                    labScript.card.GetComponent<ThisCard>().hasMoved = true;
            }

            HighlightRange(false);
            objectToMove = null;
        }
        else
        {
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
                // If tile is reachable within monster's 'stars'
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

    public void ShowPossiblePaths(GameObject labyrinthObject)
    {
        HighlightRange(false);
        objectToMove = labyrinthObject;

        startX = objectToMove.transform.parent.GetComponent<GridStat>().x;
        startY = objectToMove.transform.parent.GetComponent<GridStat>().y;

        LabyrinthObject labScript = labyrinthObject.GetComponent<LabyrinthObject>();

        spaces = labScript.moveRange;

        HighlightRange(true);
    }
}
