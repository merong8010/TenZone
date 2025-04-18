using System.Collections.Generic;
using System;
using UnityEngine;

public class TabGroup : MonoBehaviour
{
    private Tab[] tabButtons;

    private Tab selectedTab;

    public int selectedIdx;

    private Action<int> callback;

    public void Init(int idx = 0, Action<int> callback = null)
    {
        this.callback = callback;
        tabButtons = GetComponentsInChildren<Tab>();
        for(int i = 0; i < tabButtons.Length; i++)
        {
            if (i == idx)
            {
                selectedTab = tabButtons[i];
                tabButtons[i].Select();
            }
            else tabButtons[i].Deselect();
            tabButtons[i].Init(this, i);
        }
        selectedIdx = idx;
        //OnTabSelected(tabButtons[idx]);
    }

    public void OnTabSelected(Tab button)
    {
        if (selectedTab != null)
            selectedTab.Deselect();

        selectedTab = button;
        selectedTab.Select();
        selectedIdx = selectedTab.idx;

        callback?.Invoke(selectedIdx);
    }
}
