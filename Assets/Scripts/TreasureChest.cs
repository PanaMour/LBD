using System.Collections;
using UnityEngine;
using Mirror;

public class TreasureChest : NetworkBehaviour
{
    [SyncVar] public int gridX;
    [SyncVar] public int gridY;

    public Color tileColor = Color.yellow;
    public float verticalOffset = 0.5f;
    public float modelScale = 1.0f;

    private Renderer tileRenderer;

    public override void OnStartClient()
    {
        base.OnStartClient();
        StartCoroutine(SnapToGrid());
    }

    IEnumerator SnapToGrid()
    {
        Transform foundTile = null;
        GameObject gridGen = null;
        float timeout = 5.0f;

        while (foundTile == null && timeout > 0)
        {
            if (gridGen == null)
                gridGen = GameObject.Find("GridGenerator(Clone)") ?? GameObject.Find("GridGenerator");

            if (gridGen != null)
            {
                foreach (Transform child in gridGen.transform)
                {
                    GridStat stat = child.GetComponent<GridStat>();
                    if (stat != null && stat.x == gridX && stat.y == gridY)
                    {
                        foundTile = child;
                        break;
                    }
                }
            }

            if (foundTile == null)
            {
                yield return null;
                timeout -= Time.deltaTime;
            }
        }

        if (foundTile != null)
        {
            transform.SetParent(foundTile, false);

            Vector3 parentWorldScale = foundTile.lossyScale;
            float scaleX = parentWorldScale.x == 0 ? 1 : (1f / parentWorldScale.x) * modelScale;
            float scaleY = parentWorldScale.y == 0 ? 1 : (1f / parentWorldScale.y) * modelScale;
            float scaleZ = parentWorldScale.z == 0 ? 1 : (1f / parentWorldScale.z) * modelScale;

            transform.localScale = new Vector3(scaleX, scaleY, scaleZ);

            float adjustedHeight = verticalOffset / parentWorldScale.y;
            transform.localPosition = new Vector3(0, adjustedHeight, 0);
            transform.localRotation = Quaternion.identity;

            Transform visualPart = foundTile.Find("Quad");

            if (visualPart != null)
            {
                tileRenderer = visualPart.GetComponent<Renderer>();
            }
            else
            {
                foreach (Transform child in foundTile)
                {
                    if (child != this.transform)
                    {
                        Renderer r = child.GetComponent<Renderer>();
                        if (r != null)
                        {
                            tileRenderer = r;
                            break;
                        }
                    }
                }
            }

            if (tileRenderer != null)
            {
                tileRenderer.material.color = tileColor;
            }
            else
            {
                Debug.LogError($"[TreasureChest] Could not find a 'Quad' or any Renderer on Tile ({gridX},{gridY})");
            }
        }
    }

    void Update()
    {
        if (tileRenderer != null)
        {
            if (tileRenderer.material.color != tileColor)
            {
                tileRenderer.material.color = tileColor;
            }
        }
    }
}