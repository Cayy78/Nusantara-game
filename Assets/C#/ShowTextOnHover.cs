using UnityEngine;
using UnityEngine.EventSystems;

public class ShowTextOnHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public GameObject targetTextObject;

    void Start()
    {
        if (targetTextObject != null)
            targetTextObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (targetTextObject != null)
            targetTextObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (targetTextObject != null)
            targetTextObject.SetActive(false);
    }
}
