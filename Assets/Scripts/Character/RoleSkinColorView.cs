using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoleSkinColorView : RoleColorView
{
    public GameObject hsvButton;
    public HsvColorView hsvView;
    public Button backButton;

    private void Start()
    {
        GameObject btn = Instantiate(hsvButton, colorParent);
        btn.transform.SetAsFirstSibling();
        btn.GetComponentInChildren<Button>().onClick.AddListener(OnHsvClick);
        backButton.onClick.AddListener(OnBackClick);
    }

    private void OnEnable()
    {
        if (hsvView.gameObject.activeSelf)
        {
            hsvView.gameObject.SetActive(false);
        }
    }

    private void OnHsvClick()
    {
        hsvView.gameObject.SetActive(true);
        hsvView.SetToCurrentTarget();
    }

    private void OnBackClick()
    {
        hsvView.gameObject.SetActive(false);
        SetSelect(hsvView.GetCurrentColor());
    }
}
