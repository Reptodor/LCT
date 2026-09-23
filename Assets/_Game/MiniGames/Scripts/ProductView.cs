using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ProductView : MonoBehaviour, IPointerClickHandler
{
    public Image icon;
    public Text nameText;
    public Text priceText;
    public Image checkmark;
    public Image background;
    
    private ProductData data;
    private ShopManager shopManager;
    
    public void Setup(ProductData productData, ShopManager manager)
    {
        data = productData;
        shopManager = manager;
        
        icon.sprite = productData.icon;
        nameText.text = productData.productName;
        priceText.text = productData.price + " ₽";
        
        if (checkmark != null)
            checkmark.gameObject.SetActive(false);
    }
    
    public void OnPointerClick(PointerEventData eventData)
    {
        shopManager.SelectProduct(data, this);
    }
    
public bool IsBought()
{
    if (checkmark != null)
        return checkmark.gameObject.activeSelf;
    return false;
}

    public void MarkAsBought()
    {
        if (checkmark != null)
            checkmark.gameObject.SetActive(true);
    }
    
    public ProductData GetData() => data;
}