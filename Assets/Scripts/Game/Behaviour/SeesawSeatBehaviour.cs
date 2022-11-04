using RTG;
using UnityEngine;

public class SeesawSeatBehaviour : NodeBaseBehaviour
{
    private bool isFull = false;

    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        norScale = true;
    }

    public void SetPos(Vector3 pos)
    {
        transform.localPosition = pos;
    }

    public void Reverse()
    {
        transform.Rotate(0,180,0,Space.World);
    }

    public override void OnRayEnter()
    {
        base.OnRayEnter();
        if (!SeesawManager.Inst.CanUseSeesaw())
        {
            return;
        }
        if(!isFull)
        {
            PortalPlayPanel.Show();
            PortalPlayPanel.Instance.SetIcon(PortalPlayPanel.IconName.Seesaw);
            PortalPlayPanel.Instance.SetTransform(transform);
            PortalPlayPanel.Instance.AddButtonClick(PlayerOnSeesaw, true);
        }
    }

    public override void OnRayExit()
    {
        base.OnRayExit();
        if (!isFull)
        {
            PortalPlayPanel.Hide();
        }
    }

    public override void OnReset()
    {
        base.OnReset();
        var symmetrySeat = gameObject.GetComponent<SymmetrySeat>();
        if (symmetrySeat != null)
        {
            Destroy(symmetrySeat);
        }
    }

    public void PlayerOnSeesaw()
    {
        if (!StateManager.Inst.CheckCanSitOnSeesaw())
        {
            return;
        }
        
        int index = entity.Get<SeesawSeatComponent>().index;

        SeesawManager.Inst.PlayerSendOnSeesaw(GetHashCode(), index == 1);
        PortalPlayPanel.Hide();
    }

    public void SetCurStatu(bool isFull)
    {
        this.isFull = isFull;
        entity.Get<SeesawSeatComponent>().isFull = isFull;
    }
}