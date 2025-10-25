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
    [SerializeField]
    Transform[] wayPoints;
    int currentWayPoint = 0;
    [SerializeField]
    float moveSpeed = 5f;

    void Start()
    {

    }

    private void Awake()
    {
        gridArray = new GameObject[columns, rows];
        wayPoints = new Transform[columns * rows];
        if (gridPrefab)
        {
            GenerateGrid();
            //objectToMove.transform.SetParent(gridArray[startX, startY].transform);
        }
        else print("missing gridprefab, please assign.");
    }

    // Update is called once per frame
    void Update()
    {
        //
        
        if (findDistance && objectToMove != null)
        {
            SetDistance();
            SetPath();
            Movement();
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
                wayPoints[k] = obj.transform;
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

    bool TestDirection(int x, int y, int step, int direction)
    {
        // int direction tells which case to use 1 is up, 2 is right, 3 is down, 4 is left
        switch(direction)
        {
            case 4:
                if (x -1 >-1 && gridArray[x -1, y ] && gridArray[x-1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 3:
                if (y - 1 > -1 && gridArray[x, y - 1] && gridArray[x, y - 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 2:
                if (x + 1 < columns && gridArray[x+1, y ] && gridArray[x+1, y].GetComponent<GridStat>().visited == step)
                    return true;
                else
                    return false;
            case 1:
                if (y + 1 < rows && gridArray[x, y + 1] && gridArray[x, y + 1].GetComponent<GridStat>().visited == step)
                    return true;
                else
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
    void Movement()
    {
        /*if (objectToMove.transform.position == path[path.Count - 1].transform.position)
        {
            path.Reverse();
            foreach (GameObject pos in path)
            {
                Vector3 _dir = (pos.transform.position - objectToMove.transform.position);
                *//*Debug.Log(objectToMove.transform.position);
                Debug.Log(objectToMove.transform.position + _dir);*//*
                objectToMove.GetComponent<Rigidbody>().MovePosition(objectToMove.transform.position + _dir * moveSpeed * Time.deltaTime);
            }
        }*/
        if (objectToMove != null)
        {
            if (Vector3.Distance(objectToMove.transform.position, wayPoints[currentWayPoint].position) < .25f)
            {
                currentWayPoint += 1;
                currentWayPoint = currentWayPoint % wayPoints.Length;
                Debug.Log("current way points: " + currentWayPoint);
            }
            Vector3 _dir = (wayPoints[currentWayPoint].position - objectToMove.transform.position).normalized;
            objectToMove.GetComponent<Rigidbody>().MovePosition(objectToMove.transform.position + _dir * moveSpeed * Time.deltaTime);
        }
    }

    public void FindDistanceTrue(int ENDX,int ENDY)
    {
        if (objectToMove != null)
        {
            //fix spaces error
            endX = ENDX;
            endY = ENDY;
            findDistance = true;
            objectToMove.GetComponent<LabyrinthObject>().card.GetComponent<ThisCard>().hasMoved = true;
        }
    }

    public void ShowPossiblePaths(GameObject labyrinthObject)
    {
        objectToMove = labyrinthObject;
        startX = objectToMove.transform.parent.GetComponent<GridStat>().x;
        startY = objectToMove.transform.parent.GetComponent<GridStat>().y;
        spaces = labyrinthObject.GetComponent<LabyrinthObject>().card.GetComponent<ThisCard>().stars;
    }
}
