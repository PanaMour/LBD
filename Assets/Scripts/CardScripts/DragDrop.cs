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
    private bool isDraggable = true;
    private GameObject dropZone;
    private GameObject startParent;
    private Vector3 startPosition;

    public GameObject ConfirmationBox;
    public Button YesButton;
    public Button NoButton;

    private void Start()
    {
        GameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        Canvas = GameObject.Find("Main Canvas");

        if (NetworkClient.connection.identity != null)
        {
            PlayerManager = NetworkClient.connection.identity.GetComponent<PlayerManager>();
        }

        if (!hasAuthority)
        {
            isDraggable = false;
        }
    }

    void Update()
    {
        if (isDragging)
        {
            // 3D Mouse Following: 
            // We project the mouse position into the 3D world at a specific distance (1.2f) from the camera
            Vector3 mousePos = Input.mousePosition;
            mousePos.z = 1.2f;
            transform.position = Camera.main.ScreenToWorldPoint(mousePos);

            // Keep the card upright while dragging
            transform.localRotation = Quaternion.Euler(0, 0, 0);
        }
    }

    public void StartDrag()
    {
        if (!isDraggable) return;

        startParent = transform.parent.gameObject;
        startPosition = transform.position;
        isDragging = true;

        // Temporarily unparent so it doesn't get squished by HandAnchor layout
        transform.SetParent(null);
    }

    public void EndDrag()
    {
        if (!isDraggable) return;
        isDragging = false;

        // --- 3D RAYCAST DETECTION ---
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        bool isOverDropZone = false;
        dropZone = null;

        if (Physics.Raycast(ray, out hit))
        {
            GameObject hitObject = hit.collider.gameObject;

            // Check if the 3D object we hit is one of our sockets
            if (gameObject.GetComponent<ThisCard>() != null)
            {
                if (PlayerManager.PlayerSockets.Contains(hitObject))
                {
                    isOverDropZone = true;
                    dropZone = hitObject;
                }
            }
            else if (gameObject.GetComponent<ThisMagic>() != null)
            {
                if (PlayerManager.PlayerActionSockets.Contains(hitObject))
                {
                    isOverDropZone = true;
                    dropZone = hitObject;
                }
            }
        }

        // --- ORIGINAL LOGIC RE-APPLIED TO 3D ---
        if (isOverDropZone && PlayerManager.IsMyTurn && dropZone.transform.childCount == 0 && (PlayerManager.nomoresummons == false || gameObject.GetComponent<ThisMagic>() != null))
        {
            if (gameObject.GetComponent<ThisCard>() == null)
            {
                FinalizePlacement(dropZone);
            }
            else if (gameObject.GetComponent<ThisCard>().stars <= 4)
            {
                // Confirmation Box Logic (Remains 2D UI)
                GameObject box = Instantiate(ConfirmationBox);
                box.GetComponentInChildren<Text>().text = "Summon " + gameObject.GetComponent<ThisCard>().cardName + "?";
                box.transform.SetParent(Canvas.transform, false);

                YesButton = GameObject.Find("YESButton").GetComponent<Button>();
                NoButton = GameObject.Find("NOButton").GetComponent<Button>();

                YesButton.GetComponentInChildren<Text>().text = "Attack";
                NoButton.GetComponentInChildren<Text>().text = "Defense";

                StartCoroutine(AttackORDefense(dropZone, box));
            }
            else
            {
                ReturnToStart();
            }
        }
        else if (isOverDropZone && PlayerManager.IsMyTurn && dropZone.transform.childCount == 1 && dropZone.transform.GetChild(0).GetComponent<ThisCard>().canBeTributed == true && gameObject.GetComponent<ThisCard>().stars >= 5)
        {
            // Tribute Logic
            GameObject box = Instantiate(ConfirmationBox);
            box.GetComponentInChildren<Text>().text = "Tribute " + dropZone.transform.GetChild(0).GetComponent<ThisCard>().cardName + "?";
            box.transform.SetParent(Canvas.transform, false);

            YesButton = GameObject.Find("YESButton").GetComponent<Button>();
            NoButton = GameObject.Find("NOButton").GetComponent<Button>();

            StartCoroutine(Confirmation(dropZone, box));
        }
        else
        {
            ReturnToStart();
        }
    }

    private void FinalizePlacement(GameObject targetZone)
    {
        int index = FindSocketIndex(targetZone);
        isDraggable = false;
        PlayerManager.PlayCard(gameObject, index);
    }

    private void ReturnToStart()
    {
        transform.SetParent(startParent.transform);
        transform.position = startPosition;
    }

    IEnumerator Confirmation(GameObject dropZone, GameObject box)
    {
        var waitForButton = new WaitForUIButtons(YesButton, NoButton);
        yield return waitForButton.Reset();
        if (waitForButton.PressedButton == YesButton)
        {
            PlayerManager.CmdPlayerDestroyCard(dropZone.transform.GetChild(0).gameObject, 0);
            FinalizePlacement(dropZone);
        }
        else
        {
            ReturnToStart();
        }
        Destroy(box);
    }

    IEnumerator AttackORDefense(GameObject dropZone, GameObject box)
    {
        var waitForButton = new WaitForUIButtons(YesButton, NoButton);
        yield return waitForButton.Reset();

        bool isAttack = (waitForButton.PressedButton == YesButton);

        FinalizePlacement(dropZone);
        PlayerManager.CmdChangeBattlePosition(gameObject, isAttack);

        GridBehavior gb = GameObject.Find("GridGenerator").GetComponent<GridBehavior>();
        gb.ShowSummonZone(gameObject);

        Destroy(box);
    }

    private int FindSocketIndex(GameObject targetZone)
    {
        if (gameObject.GetComponent<ThisCard>() != null)
            return PlayerManager.PlayerSockets.IndexOf(targetZone);
        else
            return PlayerManager.PlayerActionSockets.IndexOf(targetZone);
    }
}