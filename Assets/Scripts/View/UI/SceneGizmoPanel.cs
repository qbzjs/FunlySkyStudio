using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SceneGizmoPanel : BasePanel<SceneGizmoPanel>
{
    public enum EAxisSign
    {
        Positive = 0,
        Negative
    }
    public enum EAxisName
    {
        X,
        Y,
        Z,
    }
    public enum EAxisType
    {
        None,
        PositiveXAxis,
        NegativeXAxis,
        PositiveYAxis,
        NegativeYAxis,
        PositiveZAxis,
        NegativeZAxis,
    }
    public class AxisDes
    {
        private EAxisSign mSign;
        private EAxisName mIndex;

        public EAxisSign Sign { get { return mSign; } }
        public EAxisName Index { get { return mIndex; } }
        public bool IsPositive { get { return mSign == EAxisSign.Positive; } }
        public bool IsNegative { get { return mSign == EAxisSign.Negative; } }

        public AxisDes(EAxisName axisIndex, EAxisSign axisSign)
        {
            mSign = axisSign;
            mIndex = axisIndex;
        }
    }
    public class SceneGizmoAxis
    {
        AxisDes mAxisDesc;
        public Action<EAxisType, AxisDes> mOnClickCallBack;
        public GameObject mGameObject;
        public Button mBtn;

        public GameObject mSelectedIcon;
        public EAxisType mAxisType;
        public SceneGizmoAxis(GameObject obj, EAxisType axisType, AxisDes gizmoAxisDesc, Action<EAxisType, AxisDes> OnClickCallBack)
        {
            mAxisDesc = gizmoAxisDesc;
            mOnClickCallBack = OnClickCallBack;
            mGameObject = obj;
            mAxisType = axisType;
            Find();
        }

        public void Find()
        {
            mBtn = mGameObject.transform.Find("Button").GetComponent<Button>();
            mSelectedIcon = mGameObject.transform.Find("Button/selected").gameObject;
            mBtn.onClick.AddListener(OnClick);
            SetSelected(false);
        }
        public void OnClick()
        {
            mOnClickCallBack?.Invoke(mAxisType, mAxisDesc);
        }
        public void SetSelected(bool selected)
        {
            mSelectedIcon.SetActive(selected);
        }
    }

    private Dictionary<EAxisType, SceneGizmoAxis> mSceneGizmoAxis = new Dictionary<EAxisType, SceneGizmoAxis>();
    public Vector3[] mAxes3D = new Vector3[3];
    public EAxisType mCurrentAxisType = EAxisType.None;
    public EditModeHandler mEditModeHandler;
    public GameObject mGizmoRoot;
    public Button mSwitchBtn;
    public GameObject mOnObj;
    public GameObject mOffObj;
    private bool mGizmoIsVisible = false;
    public bool GizmoIsVisible
    {
        get
        { return mGizmoIsVisible; }
        set
        {
            mOnObj.SetActive(value);
            mOffObj.SetActive(!value);
            mGizmoRoot.SetActive(value);
            DeselectedCurAxis();
            mGizmoIsVisible = value;
        }
    }
    public override void OnInitByCreate()
    {
        base.OnInitByCreate();
        mSwitchBtn = transform.Find("Panel/BtnGroup/Button").GetComponent<Button>();
        mSwitchBtn.onClick.AddListener(OnClickSwitchBtn);
        mOnObj = transform.Find("Panel/BtnGroup/on").gameObject;
        mOffObj = transform.Find("Panel/BtnGroup/off").gameObject;
        mGizmoRoot = transform.Find("Panel/GizmoGroup").gameObject;
        
        GameObject positiveXAxis = transform.Find("Panel/GizmoGroup/X/PositiveAxis").gameObject;
        GameObject negativeXAxis = transform.Find("Panel/GizmoGroup/X/NegativeAxis").gameObject;
        GameObject positiveYAxis = transform.Find("Panel/GizmoGroup/Y/PositiveAxis").gameObject;
        GameObject negativeYAxis = transform.Find("Panel/GizmoGroup/Y/NegativeAxis").gameObject;
        GameObject positiveZAxis = transform.Find("Panel/GizmoGroup/Z/PositiveAxis").gameObject;
        GameObject negativeZAxis = transform.Find("Panel/GizmoGroup/Z/NegativeAxis").gameObject;


        mSceneGizmoAxis.Add(EAxisType.PositiveXAxis, new SceneGizmoAxis(positiveXAxis, EAxisType.PositiveXAxis, new AxisDes(EAxisName.X, EAxisSign.Positive), OnGizmoHandlePicked));
        mSceneGizmoAxis.Add(EAxisType.NegativeXAxis, new SceneGizmoAxis(negativeXAxis, EAxisType.NegativeXAxis, new AxisDes(EAxisName.X, EAxisSign.Negative), OnGizmoHandlePicked));
        mSceneGizmoAxis.Add(EAxisType.PositiveYAxis, new SceneGizmoAxis(positiveYAxis, EAxisType.PositiveYAxis, new AxisDes(EAxisName.Y, EAxisSign.Positive), OnGizmoHandlePicked));
        mSceneGizmoAxis.Add(EAxisType.NegativeYAxis, new SceneGizmoAxis(negativeYAxis, EAxisType.NegativeYAxis, new AxisDes(EAxisName.Y, EAxisSign.Negative), OnGizmoHandlePicked));
        mSceneGizmoAxis.Add(EAxisType.PositiveZAxis, new SceneGizmoAxis(positiveZAxis, EAxisType.PositiveZAxis, new AxisDes(EAxisName.Z, EAxisSign.Positive), OnGizmoHandlePicked));
        mSceneGizmoAxis.Add(EAxisType.NegativeZAxis, new SceneGizmoAxis(negativeZAxis, EAxisType.NegativeZAxis, new AxisDes(EAxisName.Z, EAxisSign.Negative), OnGizmoHandlePicked));

        UpdateAxis();
        GizmoIsVisible = true;
    }
    private void DeselectedCurAxis()
    {
        SceneGizmoAxis axis = null;
        if (mSceneGizmoAxis.TryGetValue(mCurrentAxisType, out axis))
        {
            axis.SetSelected(false);
            mCurrentAxisType= EAxisType.None;
        }
    }
    private void OnGizmoHandlePicked(EAxisType axisType, AxisDes _axisDesc)
    {
        Quaternion targetRotation = Quaternion.LookRotation(-GetAxis3D(_axisDesc), Vector3.up);
        mEditModeHandler.OnSceneGizmoHandlePicked(targetRotation);
        if (mCurrentAxisType != axisType)
        {
            SceneGizmoAxis axis = null;
            if (mSceneGizmoAxis.TryGetValue(mCurrentAxisType, out axis))
            {
                axis.SetSelected(false);
            }
            mCurrentAxisType = axisType;
            if (mSceneGizmoAxis.TryGetValue(mCurrentAxisType, out axis))
            {
                axis.SetSelected(true);
            }
        }
    }
    public void UpdateAxis()
    {
        Matrix4x4 rotationMtx = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, Vector3.one);
        mAxes3D[0] = GetNormalizedAxis(rotationMtx, 0);
        mAxes3D[1] = GetNormalizedAxis(rotationMtx, 1);
        mAxes3D[2] = GetNormalizedAxis(rotationMtx, 2);
    }
    public static Vector3 GetNormalizedAxis(Matrix4x4 matrix, int axisIndex)
    {
        Vector3 axis = matrix.GetColumn(axisIndex);
        return Vector3.Normalize(axis);
    }
    public Vector3 GetAxis3D(AxisDes axisDesc)
    {
        Vector3 axis = mAxes3D[(int)axisDesc.Index];
        if (axisDesc.IsNegative) axis = -axis;
        return axis;
    }
    public void SetEditModeHandler(EditModeHandler handler)
    {
        mEditModeHandler = handler;
        mEditModeHandler.OnMouseAndKeyboardInput += HandleMouseAndKeyboardInput;
    }
    public void OnClickSwitchBtn()
    {
        GizmoIsVisible = !GizmoIsVisible;
    }
    public void HandleMouseAndKeyboardInput()
    {
        DeselectedCurAxis();
    }
    protected override void OnDestroy()
    {
        mEditModeHandler.OnMouseAndKeyboardInput-= HandleMouseAndKeyboardInput;
        mEditModeHandler = null;
        base.OnDestroy();
    }
}