using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Tab : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TabGroup tabGroup;
    [SerializeField]
    private GameObject focus; // 탭 선택 시 보여줄 컨텐츠

    public int idx;
    
    public void Init(TabGroup tabGroup, int idx)
    {
        this.tabGroup = tabGroup;
        this.idx = idx;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        tabGroup.OnTabSelected(this);
    }

    public void ClickItem()
    {
        tabGroup.OnTabSelected(this);
    }

    public void Select()
    {
        if (focus != null)
            focus.SetActive(true);
    }

    public void Deselect()
    {
        if (focus != null)
            focus.SetActive(false);
    }
}
