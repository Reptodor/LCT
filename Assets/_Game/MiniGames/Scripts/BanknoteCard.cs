using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BanknoteCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int value;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPos;
    private Transform startParent;
    private bool isInDropZone = false;
    
    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }
    
    void Start()
    {
        startPos = rectTransform.anchoredPosition;
        startParent = transform.parent;
        isInDropZone = (transform.parent == ShopManager.Instance.dropZone);
    }
    
    public void OnBeginDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = false;
        canvasGroup.alpha = 0.7f;
        transform.SetParent(transform.root);
        transform.SetAsLastSibling();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / GetComponentInParent<Canvas>().scaleFactor;
    }
    
    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
        canvasGroup.alpha = 1f;
        
        GameObject target = eventData.pointerCurrentRaycast.gameObject;
        bool overDrop = target != null && target.transform.IsChildOf(ShopManager.Instance.dropZone);
        
        if (isInDropZone && overDrop)
        {
            transform.SetParent(ShopManager.Instance.dropZone);
            return;
        }
        
        if (isInDropZone && !overDrop)
        {
            ReturnToStart();
            return;
        }
        
        if (!isInDropZone && overDrop)
        {
            ShopManager.Instance.OnBanknoteDropped(this);
            //isInDropZone = true;
            isInDropZone = (transform.parent == ShopManager.Instance.dropZone);
        }
        else if (!overDrop)
        {
            ReturnToStart();
        }
    }
    
    public void ReturnToStart()
    {
        transform.SetParent(startParent);
        rectTransform.anchoredPosition = startPos;
        ShopManager.Instance.RemoveBanknoteFromDrop(this);
        isInDropZone = false;
    }
}