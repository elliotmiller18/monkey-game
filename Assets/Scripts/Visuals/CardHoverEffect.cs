using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverOffsetY = 50f; // How much to move the card up

    private RectTransform cardRectTransform;
    private Vector2 originalCardPosition;

    bool frozen;

    void Start()
    {
        // Get the RectTransform of the parent GameObject (the card itself)
        cardRectTransform = transform.parent.GetComponent<RectTransform>();
        originalCardPosition = cardRectTransform.anchoredPosition;
        frozen = false;
    }

    // Move the parent card up
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (frozen) return;
        Raise();
    }

    // Move the parent card back down
    public void OnPointerExit(PointerEventData eventData)
    {
        if (frozen) return;
        Lower();
    }

    void Lower()
    {
        cardRectTransform.anchoredPosition = originalCardPosition;
    }

    void Raise()
    {
        cardRectTransform.anchoredPosition = new Vector2(originalCardPosition.x, originalCardPosition.y + hoverOffsetY);
    }
    
    public void ToggleFrozen()
    {
        frozen = !frozen;
        if (!frozen) Lower();
        else Raise();
    }
}