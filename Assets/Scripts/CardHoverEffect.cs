using UnityEngine;
using UnityEngine.EventSystems;

public class CardHoverEffect : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public float hoverOffsetY = 50f; // How much to move the card up
    
    // Reference to the RectTransform of the *actual card* we want to move.
    // This will be the parent of the GameObject this script is attached to (the HoverZone).
    private RectTransform cardRectTransform;
    private Vector2 originalCardPosition;

    void Start() // Use Awake to ensure this runs before Start on other components
    {
        // Get the RectTransform of the parent GameObject (the card itself)
        cardRectTransform = transform.parent.GetComponent<RectTransform>();
        if (cardRectTransform == null)
        {
            Debug.LogError("CardHoverEffect expects to be on a child of the card with a RectTransform.", this);
            enabled = false; // Disable script if setup is wrong
            return;
        }
        originalCardPosition = cardRectTransform.anchoredPosition;
    } 

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Move the parent card up
        cardRectTransform.anchoredPosition = new Vector2(originalCardPosition.x, originalCardPosition.y + hoverOffsetY);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Move the parent card back down
        cardRectTransform.anchoredPosition = originalCardPosition;
    }
}