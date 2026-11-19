using UnityEngine;
using UnityEngine.EventSystems;

// On a tile in the Graveyard inspector: hovering it shows the card in the
// zoom panel, the same way hovering a card on the field does. Only the
// pointer-enter event is handled, so the list can still be scrolled.
public class GraveyardTileHover : MonoBehaviour, IPointerEnterHandler
{
    public GameObject card;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (card == null) return;

        CardZoom monster = card.GetComponent<CardZoom>();
        if (monster != null) { monster.OnHoverEnter(); return; }

        MagicZoom magic = card.GetComponent<MagicZoom>();
        if (magic != null) { magic.OnHoverEnter(); return; }

        ActionZoom action = card.GetComponent<ActionZoom>();
        if (action != null) action.OnHoverEnter();
    }
}
