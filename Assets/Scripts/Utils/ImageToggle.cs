using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
[RequireComponent(typeof(Button))]
public class ImageToggle : MonoBehaviour
{
    public bool isOn = false;
    public Sprite OnSprite;
    public Sprite NormalSprite;
    private Image bgImage;
    private Button selfButton;

    public UnityEvent<bool> onValueChanged;
    // Start is called before the first frame update
    void Awake()
    {
        selfButton = this.GetComponent<Button>();
        bgImage = this.GetComponent<Image>();
        bgImage.sprite = isOn ? OnSprite : NormalSprite;
        selfButton.onClick.AddListener(() =>
        {
            isOn = !isOn;
            bgImage.sprite = isOn ? OnSprite : NormalSprite;
            onValueChanged?.Invoke(isOn);
        });
    }

    public void SetToggle(bool state)
    {
        isOn = state;
        bgImage.sprite = state ? OnSprite : NormalSprite;
    }

}
