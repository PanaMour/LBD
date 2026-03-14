using UnityEngine;
using UnityEngine.EventSystems;

public class CardHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float scaleMultiplier = 1.5f;
    public float liftAmount = 0.05f;
    public float forwardAmount = -0.05f;
    public float smoothSpeed = 15f;

    private Vector3 defaultScale = new Vector3(0.001f, 0.001f, 0.001f);
    private bool isHovering = false;

    void OnMouseEnter() { StartHover(); }
    void OnMouseExit() { StopHover(); }

    public void OnPointerEnter(PointerEventData eventData) { StartHover(); }
    public void OnPointerExit(PointerEventData eventData) { StopHover(); }

    void StartHover()
    {
        if (IsInHand())
        {
            isHovering = true;
        }
    }

    void StopHover()
    {
        isHovering = false;
    }

    void Update()
    {
        if (!IsInHand()) return;

        Vector3 targetScale = isHovering ? defaultScale * scaleMultiplier : defaultScale;
        float targetY = isHovering ? liftAmount : 0f;

        float targetZ = isHovering ? forwardAmount : 0f;

        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * smoothSpeed);

        Vector3 currentPos = transform.localPosition;
        Vector3 targetPos = new Vector3(currentPos.x, targetY, targetZ);

        transform.localPosition = Vector3.Lerp(currentPos, targetPos, Time.deltaTime * smoothSpeed);
    }

    bool IsInHand()
    {
        return transform.parent != null && transform.parent.name == "Hand_Anchor";
    }
}