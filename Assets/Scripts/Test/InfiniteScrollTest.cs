using UnityEngine;
using UnityEngine.UI;

public class InfiniteScrollTest : MonoBehaviour
{
    public ScrollRect ScrollRect;
    public RectTransform viewPortTransform;
    public RectTransform contentPanelTransform;
    public HorizontalLayoutGroup HLG;
    public RectTransform[] ItemList;

    private Vector2 OldVelocity;
    private bool isUpdated;

    void Start()
    {
        int itemsToAdd = Mathf.CeilToInt(viewPortTransform.rect.width / (ItemList[0].rect.width + HLG.spacing));
        for (int i = 0; i < itemsToAdd; i++)
        {
            RectTransform RT = Instantiate(ItemList[i % ItemList.Length], contentPanelTransform);
            RT.SetAsLastSibling();
        }

        for (int i = 0; i < itemsToAdd; i++)
        {
            int num = ItemList.Length - i - 1;
            while (num < 0)
            {
                num += ItemList.Length;
            }

            RectTransform RT = Instantiate(ItemList[num], contentPanelTransform);
            RT.SetAsFirstSibling();
        }

        contentPanelTransform.localPosition = new Vector3(0 - (ItemList[0].rect.width + HLG.spacing) * itemsToAdd,
            contentPanelTransform.localPosition.y, contentPanelTransform.localPosition.z);
    }

    void Update()
    {
        if (isUpdated)
        {
            isUpdated = false;
            ScrollRect.velocity = OldVelocity;
        }

        if (contentPanelTransform.localPosition.x > 0)
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = ScrollRect.velocity;
            contentPanelTransform.localPosition -=
                new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
            isUpdated = true;
        }

        if (contentPanelTransform.localPosition.x < 0 - (ItemList.Length * (ItemList[0].rect.width + HLG.spacing)))
        {
            Canvas.ForceUpdateCanvases();
            OldVelocity = ScrollRect.velocity;
            contentPanelTransform.localPosition +=
                new Vector3(ItemList.Length * (ItemList[0].rect.width + HLG.spacing), 0, 0);
            isUpdated = true;
        }
    }
}