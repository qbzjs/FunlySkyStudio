using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;
public enum ViewType
{
    ZoomWholeBody,
    ZoomUpperBody,
    ZoomFoot,
}

/// <summary>
/// Author:LiShuZhan
/// Description:人物形象编辑界面，对人物进行镜头拉近拉远
/// Date: 2021.01.14
/// </summary>
public class RoleMenuController : MonoBehaviour, IDragHandler
{
    public static RoleMenuController Ins;
    public ViewType viewType;
    private Camera roleCamera;
    private RoleController rController;
    private Image shadow;
    public Image zoomshoeshadow;
    public RawImage mRoleImage;
    public Material mMaskMaterial;


    private void Awake()
    {
        Ins = this;
        shadow = GameObject.Find("shadow").GetComponent<Image>();
        roleCamera = GameObject.Find("RoleCamera").GetComponent<Camera>();
        rController = GameObject.Find("CharacterPrefab").GetComponent<RoleController>();
    }
    public void OnDrag(PointerEventData eventData)
    {
        GameObject selectObj = EventSystem.current.currentSelectedGameObject;
        if (selectObj)
        {
            if (selectObj.name == "ClickArea")
            {
                Vector2 move = 0.3f * Input.GetTouch(0).deltaPosition;
                rController.transform.Rotate(Vector3.up, -move.x, Space.World);
            }
        }
    }

    private void ReRoleRote()
    {
        Vector3 roleCurRote = new Vector3(0, -180, 0);
        rController.transform.DORotate(roleCurRote, 0.5f);
    }

    private void CameraZoom()
    {
        switch (viewType)
        {
            case ViewType.ZoomUpperBody:
                ZoomUpperBody();
                break;
            case ViewType.ZoomWholeBody:
                ZoomWholeBody();
                break;
            case ViewType.ZoomFoot:
                ZoomFoot();
                break;
            default:
                break;
        }
        SetShadowType(viewType);
    }

    public void SetCameraZoomImImmediately(ViewType type)
    {
        viewType = type;
        ReRoleRote();
        Vector3 enlargeV3 = new Vector3(roleCamera.transform.position.x, 5.48f, roleCamera.transform.position.z);
        Vector3 zoomoutV3 = new Vector3(roleCamera.transform.position.x, 8.5f, roleCamera.transform.position.z);
        if (viewType == ViewType.ZoomUpperBody)
        {
            roleCamera.orthographicSize = 11f;
            roleCamera.transform.position = enlargeV3;
            roleCamera.transform.eulerAngles = Vector3.zero;
        }
        if (viewType == ViewType.ZoomWholeBody)
        {
            roleCamera.orthographicSize = 6f;
            roleCamera.transform.position = zoomoutV3;
            shadow.gameObject.SetActive(false);
        }
    }

    private void ZoomUpperBody()
    {
        Vector3 enlargeV3 = new Vector3(roleCamera.transform.position.x, 5.48f, roleCamera.transform.position.z);
        Vector3 roation = new Vector3(0, 0, 0);
        Tweener tw = DOTween.To(() => roleCamera.orthographicSize, x => roleCamera.orthographicSize = x, 7.1f, 0.5f);
        roleCamera.transform.DOMove(enlargeV3, 0.5f);
        roleCamera.transform.DORotate(roation, 0.5f);
    }
    private void ZoomWholeBody()
    {
        Vector3 zoomoutV3 = new Vector3(roleCamera.transform.position.x, 8.5f, roleCamera.transform.position.z);
        Vector3 roation = new Vector3(0, 0, 0);
        DOTween.To(() => roleCamera.orthographicSize, x => roleCamera.orthographicSize = x, 6, 0.5f);
        roleCamera.transform.DOMove(zoomoutV3, 0.5f);
        roleCamera.transform.DORotate(roation, 0.5f);
    }
    private void ZoomFoot()
    {
        Vector3 roleCurRote = new Vector3(0, -138, 0);
        rController.transform.DORotate(roleCurRote, 0.5f);
        Vector3 v = new Vector3(roleCamera.transform.position.x, 4.47f, roleCamera.transform.position.z);
        Vector3 r = new Vector3(10, roleCamera.transform.rotation.y, roleCamera.transform.rotation.z);
        Tweener tw = DOTween.To(() => roleCamera.orthographicSize, x => roleCamera.orthographicSize = x, 2.8f, 0.5f);
        roleCamera.transform.DOMove(v, 0.5f);
        roleCamera.transform.DORotate(r, 0.5f);
    }
    public void SetCameraZoom(ViewType type)
    {
        viewType = type;
        ReRoleRote();
        CameraZoom();
        SetZoomMask(type);
    }
    public void SetZoomMask(ViewType type)
    {
        if (type==ViewType.ZoomFoot)
        {
            mRoleImage.material = mMaskMaterial;
        }
        else
        {
            mRoleImage.material = null;
        }
    }
    private void SetShadowType(ViewType type)
    {
        switch (type)
        {
            case ViewType.ZoomFoot:
                shadow.gameObject.SetActive(false);
                zoomshoeshadow.sprite = SpriteAtlasManager.Inst.GetAvatarCommonSprite("zoomshoe_shadow");
                zoomshoeshadow.gameObject.SetActive(true);
                break;
            case ViewType.ZoomUpperBody:
                shadow.gameObject.SetActive(true);
                zoomshoeshadow.gameObject.SetActive(false);
                break;
            case ViewType.ZoomWholeBody:
                shadow.gameObject.SetActive(false);
                zoomshoeshadow.gameObject.SetActive(false);
                break;
        }
    }
    private void OnDestroy()
    {
        Ins = null;
    }
}
