using UnityEngine;

public class HandLayout : MonoBehaviour
{
    public float cardSpacing = 0.1f;

    void Update()
    {
        if (transform.childCount == 0) return;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform card = transform.GetChild(i);

            float xOffset = (i - (transform.childCount - 1) / 2f) * cardSpacing;

            card.localPosition = new Vector3(xOffset, 0, i * 0f);

            card.localRotation = Quaternion.identity;
        }
    }
}