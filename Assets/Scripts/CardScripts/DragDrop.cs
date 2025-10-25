using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using UnityEngine.UI;

public class DragDrop : NetworkBehaviour
{
    public GameManager GameManager;
    public GameObject Canvas;
    public PlayerManager PlayerManager;

    private bool isDragging = false;
    private bool isOverDropZone = false;
    private bool isDraggable = true;
    private GameObject dropZone;
    private GameObject startParent;
    private Vector2 startPosition;

    public GameObject ConfirmationBox;
    public Button YesButton;
    public Button NoButton;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Canvas = GameObject.Find("Main Canvas");
        NetworkIdentity networkIdentity = NetworkClient.connection.identity;
        PlayerManager = networkIdentity.GetComponent<PlayerManager>();

        if (!hasAuthority)
        {
            isDraggable = false;
        }
    }
    void Update()
    {
        if (isDragging)
        {
            transform.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);
            transform.SetParent(Canvas.transform, true);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        /*if (collision.gameObject == PlayerManager.PlayerSockets[PlayerManager.CardsPlayed])
        {
            isOverDropZone = true;
            dropZone = collision.gameObject;
        }*/


        // checks all the PlayerSockets for a collision
        for (int i = 0; i < 4; i++)
        {
            if (collision.gameObject == PlayerManager.PlayerSockets[i] && gameObject.GetComponent<ThisCard>() != null)
            {
                isOverDropZone = true;
                dropZone = collision.gameObject;                
            }
            else if(collision.gameObject == PlayerManager.PlayerActionSockets[i] && gameObject.GetComponent<ThisMagic>() != null)
            {
                isOverDropZone = true;
                dropZone = collision.gameObject;
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isOverDropZone = false;
        dropZone = null;
    }

    public void StartDrag()
    {
        if (!isDraggable) return;
        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        isDragging = true;
    }

    IEnumerator Confirmation(GameObject dropZone,GameObject box)
    {
        var waitForButton = new WaitForUIButtons(YesButton,NoButton);
        yield return waitForButton.Reset();
        if(waitForButton.PressedButton == YesButton)
        {
            PlayerManager.CmdPlayerDestroyCard(dropZone.transform.GetChild(0).gameObject, 0);
            transform.SetParent(dropZone.transform, false);
            int index = FindSocketIndex(dropZone);
            isDraggable = false;
            PlayerManager.PlayCard(gameObject, index);
            gameObject.GetComponent<ThisCard>().confirmationfinished = true;
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(startParent.transform, true);
        }
        Destroy(box);
    }

    IEnumerator AttackORDefense(GameObject dropZone, GameObject box)
    {
        var waitForButton = new WaitForUIButtons(YesButton, NoButton);
        yield return waitForButton.Reset();
        if (waitForButton.PressedButton == YesButton)
        {
            transform.Rotate(0, 0, 0);
            transform.SetParent(dropZone.transform, false);
            int index = FindSocketIndex(dropZone);
            isDraggable = false;
            PlayerManager.PlayCard(gameObject, index);
            PlayerManager.CmdChangeBattlePosition(gameObject, true);
            //gameObject.GetComponent<ThisCard>().attackmode = true;
            GameObject LabObject = Instantiate(gameObject.GetComponent<ThisCard>().LabyrinthObject, new Vector3(0, 0, 0), Quaternion.identity);
            LabObject.transform.GetComponent<Image>().sprite = gameObject.GetComponent<ThisCard>().thisSprite;
            Debug.Log(transform);
            if(PlayerManager.CardsPlayed%2 == 0)
            {
                LabObject.transform.GetComponent<LabyrinthObject>().card = transform.gameObject;
                LabObject.transform.SetParent(transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(4,0)"));
                LabObject.transform.position = transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(4,0)").position;
            }
            else
            {
                LabObject.transform.GetComponent<LabyrinthObject>().card = transform.gameObject;
                LabObject.transform.SetParent(transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(6,0)"));
                LabObject.transform.position = transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(6,0)").position;
            }
            gameObject.GetComponent<ThisCard>().confirmationfinished = true;
        }
        else
        {
            transform.Rotate(0, 0, 90);
            transform.SetParent(dropZone.transform, false);
            int index = FindSocketIndex(dropZone);
            isDraggable = false;
            PlayerManager.PlayCard(gameObject, index);
            PlayerManager.CmdChangeBattlePosition(gameObject, false);
            //gameObject.GetComponent<ThisCard>().attackmode = false;
            GameObject LabObject = Instantiate(gameObject.GetComponent<ThisCard>().LabyrinthObject, new Vector3(0, 0, 0), Quaternion.identity);
            LabObject.transform.GetComponent<Image>().sprite = gameObject.GetComponent<ThisCard>().thisSprite;
            if (PlayerManager.CardsPlayed % 2 == 0)
            {
                LabObject.transform.GetComponent<LabyrinthObject>().card = transform.gameObject;
                LabObject.transform.SetParent(transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(4,0)"));
                LabObject.transform.position = transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(4,0)").position;
            }
            else
            {
                LabObject.transform.GetComponent<LabyrinthObject>().card = transform.gameObject;
                LabObject.transform.SetParent(transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(6,0)"));
                LabObject.transform.position = transform.parent.parent.parent.Find("GridGenerator").Find("GridObject(6,0)").position;
            }
            gameObject.GetComponent<ThisCard>().confirmationfinished = true;
            /*transform.position = startPosition;
            transform.SetParent(startParent.transform, true);*/
        }
        Destroy(box);
    }

    public void EndDrag()
    {
        if (!isDraggable) return;
        isDragging = false;

        if (isOverDropZone && PlayerManager.IsMyTurn && dropZone.transform.childCount == 0 && (PlayerManager.nomoresummons == false || gameObject.GetComponent<ThisMagic>() != null))
        {
            if(gameObject.GetComponent<ThisCard>() == null)
            {
                transform.SetParent(dropZone.transform, false);
                int index = FindSocketIndex(dropZone);
                isDraggable = false;
                PlayerManager.PlayCard(gameObject, index);
            }else if (gameObject.GetComponent<ThisCard>().stars <= 4)
            {
                GameObject box = Instantiate(ConfirmationBox);
                NetworkServer.Spawn(box, connectionToClient);
                box.GetComponentInChildren<Text>().text = "Do you want to summon " + gameObject.GetComponent<ThisCard>().cardName + "?";
                box.transform.SetParent(Canvas.transform);
                YesButton = GameObject.Find("YESButton").GetComponent<Button>();
                YesButton.GetComponentInChildren<Text>().text = "Attack";
                NoButton = GameObject.Find("NOButton").GetComponent<Button>();
                NoButton.GetComponentInChildren<Text>().text = "Defense";
                StartCoroutine(AttackORDefense(dropZone, box));
                /*gameObject.GetComponent<ThisCard>().attackmode = true;
                transform.SetParent(dropZone.transform, false);
                int index = FindSocketIndex(dropZone);
                isDraggable = false;
                PlayerManager.PlayCard(gameObject, index);*/
            }
            else
            {
                transform.position = startPosition;
                transform.SetParent(startParent.transform, true);
            }
        }
        else if (isOverDropZone && PlayerManager.IsMyTurn && dropZone.transform.childCount == 1 && dropZone.transform.GetChild(0).GetComponent<ThisCard>().canBeTributed == true && gameObject.GetComponent<ThisCard>().stars >= 5 && gameObject.GetComponent<ThisCard>().stars <= 6 && (PlayerManager.nomoresummons == false || gameObject.GetComponent<ThisMagic>() != null))
        {
            GameObject box = Instantiate(ConfirmationBox);
            NetworkServer.Spawn(box, connectionToClient);
            box.GetComponentInChildren<Text>().text = "Do you want to tribute " + dropZone.transform.GetChild(0).GetComponent<ThisCard>().cardName + " to summon " + gameObject.GetComponent<ThisCard>().cardName + "?";
            box.transform.SetParent(Canvas.transform);
            YesButton = GameObject.Find("YESButton").GetComponent<Button>();
            NoButton = GameObject.Find("NOButton").GetComponent<Button>();
            StartCoroutine(Confirmation(dropZone,box));
        }
        else
        {
            transform.position = startPosition;
            transform.SetParent(startParent.transform, true);
        }
    }

    private int FindSocketIndex(GameObject dropZone)
    {
        int i = 0;
        if (gameObject.GetComponent<ThisCard>() != null)
        {
            for (i = 0; i < PlayerManager.PlayerSockets.Count; i++)
            {
                if (PlayerManager.PlayerSockets[i] == dropZone)
                    return i;
            }
        }
        else if(gameObject.GetComponent<ThisMagic>() != null)
        {
            for (i = 0; i < PlayerManager.PlayerActionSockets.Count; i++)
            {
                if (PlayerManager.PlayerActionSockets[i] == dropZone)
                    return i;
            }
        }
        return i;
    }
}
