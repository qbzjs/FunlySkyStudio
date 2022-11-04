using UnityEngine;

public class VIPAreaBehaviour : NodeBaseBehaviour
{
    private Transform sign;
    private Transform signPlay;
    private float len = 5.628659f;
    private float thickness = 0.5f;
    private BoxCollider topC;
    private Vector3 topCenterSrc;
    private Vector3 topSizeSrc;
    private BoxCollider bottomC;
    private Vector3 bottomCenterSrc;
    private BoxCollider faceC;
    private Vector3 faceCenterSrc;
    private Vector3 faceSizeSrc;
    private BoxCollider backC;
    private Vector3 backCenterSrc;
    private BoxCollider leftC;
    private Vector3 leftCenterSrc;
    private Vector3 leftSizeSrc;
    private BoxCollider rightC;
    private Vector3 rightCenterSrc;
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        sign= transform.Find("Sign");
        signPlay= transform.Find("SignPlay");
        topC = GameUtils.FindChildByName(transform, "vipAreaBox_top").GetComponent<BoxCollider>();
        bottomC = GameUtils.FindChildByName(transform, "vipAreaBox_bottom").GetComponent<BoxCollider>();
        faceC = GameUtils.FindChildByName(transform, "vipAreaBox_face").GetComponent<BoxCollider>();
        backC = GameUtils.FindChildByName(transform, "vipAreaBox_back").GetComponent<BoxCollider>();
        leftC = GameUtils.FindChildByName(transform, "vipAreaBox_left").GetComponent<BoxCollider>();
        rightC = GameUtils.FindChildByName(transform, "vipAreaBox_right").GetComponent<BoxCollider>();
        float topOffset = len - thickness/2;
        topCenterSrc = new Vector3(0,topOffset,0);
        topC.center = topCenterSrc;
        topSizeSrc = new Vector3(len,thickness,len);
        topC.size = topSizeSrc;
        float bottomOffset = - thickness/2;
        bottomCenterSrc = new Vector3(0,bottomOffset,0);
        bottomC.center = bottomCenterSrc;
        bottomC.size = topSizeSrc;
        float otherOffset = (len - thickness)/2;
        float yOffset = thickness/2;
        faceCenterSrc = new Vector3(0,otherOffset + yOffset,-otherOffset);
        faceC.center = faceCenterSrc;
        faceSizeSrc = new Vector3(len,len,thickness);
        faceC.size = faceSizeSrc;
        backCenterSrc = new Vector3(0,otherOffset + yOffset,otherOffset);
        backC.center = backCenterSrc;
        backC.size = faceSizeSrc;
        leftCenterSrc = new Vector3(-otherOffset,otherOffset + yOffset,0);
        leftC.center = leftCenterSrc;
        leftSizeSrc = new Vector3(thickness,len,len);
        leftC.size = leftSizeSrc;
        rightCenterSrc = new Vector3(otherOffset,otherOffset + yOffset,0);
        rightC.center = rightCenterSrc;
        rightC.size = leftSizeSrc;
    }

    public void AdjustColliderPosAndSize()
    {
        float factorX = transform.localScale.x * transform.parent.localScale.x;
        float factorY = transform.localScale.y * transform.parent.localScale.y;
        float factorZ = transform.localScale.z * transform.parent.localScale.z;
        float wantThick = thickness;
        float realThickY = thickness * factorY;
        float toSetThickY = thickness / factorY;
        topC.size = new Vector3(topSizeSrc.x,toSetThickY,topSizeSrc.z);
        bottomC.size = topC.size;
        
        float topAdjustY = (topCenterSrc.y * factorY + (realThickY - wantThick) / 2) / factorY;
        topC.center = new Vector3(topCenterSrc.x,topAdjustY,topCenterSrc.z);

        float bottomAdjustY = (bottomCenterSrc.y * factorY + (realThickY - wantThick) / 2) / factorY;
        bottomC.center = new Vector3(bottomCenterSrc.x,bottomAdjustY,bottomCenterSrc.z);
        
        float realThickX = thickness * factorX;
        float toSetThickX = thickness / factorX;
        leftC.size = new Vector3(toSetThickX,leftSizeSrc.y,leftSizeSrc.z);
        rightC.size = leftC.size;
        
        float leftAdjustX = (leftCenterSrc.x * factorX - (realThickX - wantThick) / 2) / factorX;
        leftC.center = new Vector3(leftAdjustX,leftCenterSrc.y,leftCenterSrc.z);
        
        float rightAdjustX = (rightCenterSrc.x * factorX + (realThickX - wantThick) / 2) / factorX;
        rightC.center = new Vector3(rightAdjustX,rightCenterSrc.y,rightCenterSrc.z);
        
        float realThickZ = thickness * factorZ;
        float toSetThickZ = thickness / factorZ;
        faceC.size = new Vector3(faceSizeSrc.x,faceSizeSrc.y,toSetThickZ);
        backC.size = faceC.size;
        
        float faceAdjustZ = (faceCenterSrc.z * factorZ - (realThickZ - wantThick) / 2) / factorZ;
        faceC.center = new Vector3(faceCenterSrc.x,faceCenterSrc.y,faceAdjustZ);
        
        float backAdjustZ = (backCenterSrc.z * factorZ + (realThickZ - wantThick) / 2) / factorZ;
        backC.center = new Vector3(backCenterSrc.x,backCenterSrc.y,backAdjustZ);
    }

    public void SwitchPlayMode()
    {
        sign.gameObject.SetActive(false);
        signPlay.gameObject.SetActive(true);
    }

    public void SwitchEditMode()
    {
        sign.gameObject.SetActive(true);
        signPlay.gameObject.SetActive(false);
    }

    public void SwitchFaceColliderStatus(bool status)
    {
        faceC.enabled = status;
    }

    public void SwitchAllColliderStatus(bool status)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            colliders[i].enabled = status;
        }
    }

    public void SwitchColliderLayer(string layer)
    {
        Collider[] colliders = GetComponentsInChildren<Collider>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i].gameObject.name == "vipAreaBox_bottom")
            {
                continue;
            }
            colliders[i].gameObject.layer = LayerMask.NameToLayer(layer);
        }
    }

    public bool CheckPlayerInArea(Vector3 pos)
    {
        BoxCollider bCollider = gameObject.GetComponent<BoxCollider>();
        if (bCollider == null)
        {
            bCollider = gameObject.AddComponent<BoxCollider>();
            bCollider.isTrigger = true;
            var cNode = transform;
            Vector3 postion = cNode.position;
            Quaternion rotation = cNode.rotation;
            Vector3 scale = cNode.localScale;
            cNode.position = Vector3.zero;
            cNode.rotation = Quaternion.Euler(Vector3.zero);
            cNode.localScale = Vector3.one;
            Vector3 center = Vector3.zero;
            Renderer[] renders = cNode.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer child in renders)
            {
                center += child.bounds.center;
            }
            if (renders.Length != 0)
            {
                center /= renders.Length;
            }
            Bounds bounds = new Bounds(center, Vector3.zero);
            foreach (Renderer child in renders)
            {
                bounds.Encapsulate(child.bounds);
            }

            bCollider.size = new Vector3(bounds.size.x / cNode.lossyScale.x, bounds.size.y / cNode.lossyScale.y, bounds.size.z/ cNode.lossyScale.z);
            bCollider.center = cNode.InverseTransformPoint(bounds.center);
            cNode.position = postion;
            cNode.rotation = rotation;
            cNode.localScale = scale;
        }
        bool inArea = GameUtils.Contains(bCollider, pos);
        return inArea;
    }

    public void DeleteContainCollider()
    {
        BoxCollider containCllider = gameObject.GetComponent<BoxCollider>();
        if (containCllider != null)
        {
            Destroy(containCllider);
        }
    }
}