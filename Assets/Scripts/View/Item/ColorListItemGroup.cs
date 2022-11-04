using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ColorListItemGroup
{
    public List<ColorListItem> Items { get; private set;}
    public int CurItemIndex { get; private set; }
    private Transform parent;

    public ColorListItemGroup(Transform parent)
    {
        this.parent = parent;
        Items = new List<ColorListItem>();
        CurItemIndex = -1;
    }
    public void Create(GameObject src, int size)
    {
        for (int i = 0; i < size; ++i)
        {
            GameObject newItem = Object.Instantiate(src, parent);
            if (newItem.TryGetComponent(out ColorListItem item))
            {
                Items.Add(item);
                item.SetActive(false);
            }
        }
    }

    public void AddSelectListener(UnityAction<int> onClick)
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            int index = i;
            Items[i].AddSelectListener(() => onClick(index));
        }
    }

    public void AddNextListener(UnityAction<int> onClick)
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            int index = i;
            Items[i].AddNextListener(() => onClick(index));
        }
    }

    public void AddLastListener(UnityAction<int> onClick)
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            int index = i;
            Items[i].AddLastListener(() => onClick(index));
        }
    }

    public void AddDelListener(UnityAction<int> onClick)
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            int index = i;
            Items[i].AddDelListener(() => onClick(index));
        }
    }

    public void DiselectAll()
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].Diselect();
        }
        CurItemIndex = -1;
    }

    public void DisActiveAll()
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            Items[i].SetActive(false);
        }
        CurItemIndex = -1;
    }

    public void RefreshList(List<Color> colors)
    {
        for (int i = 0; i < Items.Count; ++i)
        {
            if(i < colors.Count)
            {
                Items[i].SetActive(true);
                Items[i].SetColor(colors[i]);
            }
            else
            {
                Items[i].SetActive(false);
            }           
        }
    }

    public void SelectOnly(int i)
    {
        DiselectAll();
        if(i >= 0 && i < Items.Count)
        {
            Items[i].Select();
        }      
    }

    public void SelectOnlyWithoutNotify(int i)
    {
        DiselectAll();
        if(i >= 0 && i < Items.Count && Items[i].IsActive)
        {
            CurItemIndex = i;
            bool showLast = i > 0;
            bool showNext = i + 1 < Items.Count && Items[i + 1].IsActive;
            Items[i].OnSelect(showLast, showNext);
        }       
    }

    public void SelectColorWithoutNotify(Color color)
    {
        DiselectAll();
        for (int i = 0; i < Items.Count; ++i)
        {
            if (Items[i].IsActive && Items[i].CurColor == color)
            {
                SelectOnlyWithoutNotify(i);
            }
        }
    }

    public ColorListItem GetCurrentItem()
    {
        if(CurItemIndex >= 0 && CurItemIndex < Items.Count)
        {
            return Items[CurItemIndex];
        }
        return null;
    }
    public void ApplyDefaultSelectListener()
    {
        AddSelectListener(SelectOnlyWithoutNotify);
    }



    /*
        #region  Default Listening



        public void ApplyDefaultNextListener()
        {
            AddNextListener(OnNextClick);
        }

        public void ApplyDefaultLastListener()
        {
            AddLastListener(OnLastClick);
        }

        public void ApplyDefaultDelListener()
        {
            AddDelListener(OnDelClick);
        }

        private void OnSelectClick(int i)
        {
            SelectOnlyWithoutNotify(i);
        }

        private void OnLastClick(int i)
        {
            if(i > 0)
            {
                SwitchColorSelect(i, i - 1);
            }
        }

        private void OnNextClick(int i)
        {
            if (i+1 < Items.Count && Items[i+1].IsActive)
            {
                SwitchColorSelect(i, i + 1);
            }
        }

        private void OnDelClick(int i)
        {
            for(int ind = i; ind < Items.Count; ++ind)
            {
                int nxt = ind + 1;
                if(nxt < Items.Count && Items[nxt].IsActive)
                {
                    Items[ind].SetColor(Items[nxt].CurColor);
                }
                else
                {
                    Items[ind].SetActive(false);
                }
            }
            if (Items[i].IsActive)
            {
                SelectOnly(i);
            }
            else if (i > 0)
            {
                SelectOnly(i - 1);
            }
            else
            {
                DiselectAll();
            }
        }
        #endregion

        private void SwitchColorSelect(int i, int other)
        {
            Color otherColor = Items[other].CurColor;
            Color thisColor = Items[i].CurColor;
            Items[other].SetColor(thisColor);
            Items[i].SetColor(otherColor);
            Items[i].Diselect();
            SelectOnly(other);
        }
    */

}
