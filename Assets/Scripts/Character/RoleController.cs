/// <summary>
/// Author:Mingo-LiZongMing
/// Description:控制人物模型的换装
/// </summary>
using System;
using System.Collections.Generic;
using UnityEngine;



public class RoleController : MonoBehaviour
{
    public RuntimeAnimatorController animCtr;
    public RuntimeAnimatorController moveChooseCtr;
    public Animator animCom;
    public Camera matchCamera;
    public Action ShowFrame;
    [HideInInspector]
    public enum PlayerType
    {
        SelfPlayer = 1,
        OtherPlayer = 2,
    }
    public PlayerType curPlayerType = PlayerType.SelfPlayer;
    public enum InitRoleType
    {
        Normal = 1,
        UGCPlayerModelCloth = 2,//ugc人物参照模式（不生成衣服）
        UGCPlayerModelFace = 3,//ugc人物参照模式（不生成脸部）
    }

    //拾取道具拾起点
    private Transform pickPos;
    //拾取道具父节点
    private Transform pickNode;
    //拾取道具父节点
    private Transform foodNode;
    //骨骼头部路径
    const string BONE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 Head";
    //材质路径
    const string MAT_PATH = "Character/Material/";
    //贴图路径
    const string TEX_PATH = "Character/Texture/";
    private GameObject head;
    //脸部装饰
    private GameObject mat_Blusher_L;
    private GameObject mat_Blusher_R;
    private GameObject bone_Blusher_L;
    private GameObject bone_Blusher_R;
    //眉毛
    private GameObject bone_Brow_L;
    private GameObject bone_Brow_R;
    private Material mat_Brow_L;
    private Material mat_Brow_R;
    //眼睛
    private GameObject bone_Eye_L;
    private GameObject bone_Eye_R;
    private GameObject mat_Eye_L;
    private GameObject mat_Eye_R;
    //面部
    private GameObject fbx_Face;
    //面部彩绘
    private Material mat_Pattern;
    private GameObject ugc_face;

    //身体
    private GameObject fbx_Body;
    private Material mat_Body;
    //嘴巴
    private GameObject bone_Mouth;
    private Material mat_Mouth;
    //鼻子
    private GameObject bone_Nose;
    private GameObject child_Nose;
    private GameObject fbx_Nose;
    private Material mat_Nose;
    //衣服
    private GameObject mat_Clothes_01;
    private GameObject mat_Clothes_02;
    private GameObject mat_Clothes_03;
    //头饰
    private GameObject bone_Hat;

    //特效
    private GameObject bone_Effect;

    private GameObject bone_Effect_x;

    //眼镜
    private GameObject bone_Glasses;

    private GameObject mat_Hair;

    private string curHatColor;
    private string curGlassesColor;
    private string curBagColor;

    public GameObject roleFbx;
    public static RoleConfigData CurConfigRoleData = new RoleConfigData();

    Dictionary<string, Transform> BonesDic = new Dictionary<string, Transform>();
    Dictionary<string, GameObject> hatStyleDic = new Dictionary<string, GameObject>();
    Dictionary<string, GameObject> glassesStyleDic = new Dictionary<string, GameObject>();
    Dictionary<int, GameObject> bagStyleDic = new Dictionary<int, GameObject>();
    Dictionary<int, GameObject[]> handStyleDic = new Dictionary<int, GameObject[]>();
    Dictionary<string, GameObject> specialStyleDic = new Dictionary<string, GameObject>();
    Dictionary<int, GameObject> effectStyleDic = new Dictionary<int, GameObject>();

    //shoes
    private GameObject shoe;

    //scraf
    private GameObject accessories;

    //背饰
    private GameObject bone_Bag;
    private GameObject Crossbody;


    //手部装饰
    private GameObject bone_hand_l;
    private GameObject bone_hand_r;

    //手腕装饰
    private GameObject bone_handarm_l;
    private GameObject bone_handarm_r;
    
    //特殊挂饰
    private GameObject bone_special;

    private Transform underWear;
    
    //层级相关
    private Transform nickTF;
    private Transform stateTF;
    private Transform netTF;

    public RoleData customRoleData = new RoleData();

    //骨骼右手路径
    const string RIGHT_HAND_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/prop_r";
    //骨骼左手路径
    const string LEFT_HAND_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand/prop_l";
    const string LEFT_EFFECT_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand/effect_l";



    //骨骼右手拾起取点路径
    const string PICK_HAND_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/prop_r/pickNode";
    //吃东西拾起点
    const string PICK_FOOD_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/prop_r/foodNode";
    //骨骼右手装饰路径
    const string HAND_R_GLOVE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/Bip001 R Hand/glove_r/glove_r_x";
    //骨骼左手装饰路径
    const string HAND_L_GLOVE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/Bip001 L Hand/glove_l/glove_l_x";
    //骨骼右手手腕装饰路径
    const string HAND_R_ARMGLOVE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 R Clavicle/Bip001 R UpperArm/Bip001 R Forearm/arm_r/arm_r_x";
    //骨骼左手手腕装饰路径
    const string HAND_L_ARMGLOVE_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/Bip001 Neck/Bip001 L Clavicle/Bip001 L UpperArm/Bip001 L Forearm/arm_l/arm_l_x";
    
    //背部路径
    const string BACK_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/backNode";

    //背包路径
    const string BAG_PATH = "Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/back";

    //UGC衣服骨骼节点数据
    Dictionary<int, List<string>> clothBoneConfigs = new Dictionary<int, List<string>>()
    {
        { 1, new List<string> { "clothing_1000_tops_b", "clothing_1000_tops_f", "clothing_1000_tops_l", "clothing_1000_tops_r", "clothing_1000_trosers_b", "clothing_1000_trosers_f" }},
        { 2, new List<string> { "clothing_1001_tops_b", "clothing_1001_tops_f", "clothing_1001_tops_l", "clothing_1001_tops_r" }},
        { 3, new List<string> { "clothing_1003_tops_b", "clothing_1003_tops_f", "clothing_1003_tops_l", "clothing_1003_tops_r" }},
        { 4, new List<string> { "clothing_1002_tops_b", "clothing_1002_tops_f", "clothing_1002_tops_l", "clothing_1002_tops_r" }}

    };
    //UGC形象资源已创建列表
    Dictionary<int, List<int>> CreateUgcBoneDic = new Dictionary<int, List<int>>() {
         { (int)UGC_CLOTH_TYPE.CLOTH, new List<int> { } }
    };

    private Action<int> handPropCreated;

    public int playerLayer;
    //特殊的眼睛Id，由于特殊眼睛使用了新的shader，在没有眨眼动画的场景（比如更换头像）需要替换相关眼睛材质，来展示眼睛
    private const int specialEyeId = 18;
    private string[] currentStyles = new string [(int)BundlePart.PartCount];
    public Dictionary<ClassifyType, GameObject> ParticleNodeDic = new Dictionary<ClassifyType, GameObject>();
    private void Awake()
    {
        //Inst = this;
        InitRoleData();
    }

    public void InitRoleData()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        styleOrder.Clear();
        head = this.transform?.Find(BONE_PATH).gameObject;

        mat_Blusher_L = head.transform.Find("blusher_l/body_blusher_l").gameObject;
        mat_Blusher_R = head.transform.Find("blusher_r/body_blusher_r").gameObject;
        bone_Blusher_L = head.transform.Find("blusher_l").gameObject;
        bone_Blusher_R = head.transform.Find("blusher_r").gameObject;

        bone_Brow_L = head.transform.Find("brow_l").gameObject;
        bone_Brow_R = head.transform.Find("brow_r").gameObject;
        var brow_L = head.transform.Find("brow_l/brow_l_01/body_brow_l").gameObject;
        var brow_R = head.transform.Find("brow_r/brow_r_01/body_brow_r").gameObject;
        mat_Brow_L = brow_L.GetComponent<MeshRenderer>().material;
        mat_Brow_R = brow_R.GetComponent<MeshRenderer>().material;

        bone_Eye_L = head.transform.Find("eye_l").gameObject;
        bone_Eye_R = head.transform.Find("eye_r").gameObject;
        mat_Eye_L = head.transform.Find("eye_l/eye_l_01/body_eye_l").gameObject;
        mat_Eye_R = head.transform.Find("eye_r/eye_r_01/body_eye_r").gameObject;

        fbx_Face = this.transform.Find("body_face").gameObject;
        fbx_Body = this.transform.Find("body").gameObject;
        mat_Pattern = fbx_Face.GetComponent<SkinnedMeshRenderer>().material;
        mat_Body = fbx_Body.GetComponent<SkinnedMeshRenderer>().material;

        bone_Mouth = head.transform.Find("mouth").gameObject;
        mat_Mouth = head.transform.Find("mouth/body_mouth").gameObject.GetComponent<MeshRenderer>().material;

        bone_Nose = head.transform.Find("nose").gameObject;
        child_Nose = bone_Nose.transform.Find("nose_x").gameObject;
        fbx_Nose = bone_Nose.transform.Find("nose_x/body_nose").gameObject;
        mat_Nose = fbx_Nose.GetComponent<MeshRenderer>().material;

        mat_Clothes_01 = this.transform.Find("clothing_01").gameObject;
        
        bone_Hat = head.transform.Find("hat").gameObject;


        bone_Glasses = head.transform.Find("glasses").gameObject;


        mat_Hair = this.transform.Find("hair").gameObject;

        shoe = this.transform.Find("shoe").gameObject;

        accessories = this.transform.Find("scarf_01").gameObject;

        bone_Bag = this.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/Bip001 Spine1/back").gameObject;
        Crossbody = this.transform.Find("Crossbody").gameObject;
   

        bone_hand_l = this.transform.Find(HAND_L_GLOVE_PATH).gameObject;
        bone_hand_r = this.transform.Find(HAND_R_GLOVE_PATH).gameObject;

        bone_handarm_l = this.transform.Find(HAND_L_ARMGLOVE_PATH).gameObject;
        bone_handarm_r = this.transform.Find(HAND_R_ARMGLOVE_PATH).gameObject;
        
        bone_special = this.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/waist").gameObject;


        pickNode = transform.Find(PICK_HAND_PATH);
        pickPos = transform.Find("pickPos");

        nickTF = transform.Find("playerInfo/nick");
        stateTF = transform.Find("state");
        netTF = transform.Find("netTip");

        underWear = transform.Find("body_underwear");

        ugc_face = transform.Find(BONE_PATH + "/ugc_face").gameObject;

        InitBoneData();
    }

    public void InitPlayerType(PlayerType playerType)
    {
        curPlayerType = playerType;
    }

    public void InitPlayerLayer()
    {
        #region 公共部分
        //"ShotExclude"层
        GameUtils.ChangeToTargetLayer("ShotExclude", nickTF);
        GameUtils.ChangeToTargetLayer("ShotExclude", stateTF);
        GameUtils.ChangeToTargetLayer("ShotExclude", netTF);
        #endregion
        #region 其他独有
        if (curPlayerType == PlayerType.OtherPlayer)
        {
            //TODO：其他玩家特殊层设置
        }
        #endregion
        #region 自己独有
        else
        {
            //"Head"层
            GameUtils.ChangeToTargetLayer("Head", head.transform);
            GameUtils.ChangeToTargetLayer("Head", fbx_Face.transform);
            GameUtils.ChangeToTargetLayer("Head", mat_Hair.transform);
        }
        #endregion
    }

    void InitBoneData()
    {
        Transform[] transforms = transform.GetComponentsInChildren<Transform>();
        BonesDic.Clear();
        foreach (var transform in transforms)
        {
            BonesDic[transform.name] = transform;
        }
    }

    public Material LoadMaterial(string matPath)
    {
        Material material = ResManager.Inst.LoadCharacterRes<Material>(matPath);  //material的名字
        return material;
    }

    public Texture LoadTexture(string matPath)
    {
        Texture texture = ResManager.Inst.LoadCharacterRes<Texture>(matPath);  //material的名字
        return texture;
    }

    public void SetBlusherColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            mat_Blusher_L.GetComponent<Renderer>().material.color = color;
            mat_Blusher_R.GetComponent<Renderer>().material.color = color;
        }
    }

    public void SetBlusherStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            succ?.Invoke();
            mat_Blusher_L.SetActive(false);
            mat_Blusher_R.SetActive(false);
            return;
        }

        currentStyles[(int)BundlePart.Face] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Face, texName, wrapper =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Face] != texName) return;
            currentStyles[(int)BundlePart.Face] = texName;
            mat_Blusher_L.SetActive(true);
            mat_Blusher_R.SetActive(true);
            Texture tex_L =  wrapper.LoadAsset<Texture>(texName + "_left");
            Texture tex_R = wrapper.LoadAsset<Texture>(texName + "_right");

            mat_Blusher_L.GetComponent<MeshRenderer>().material.mainTexture = tex_L;
            mat_Blusher_R.GetComponent<MeshRenderer>().material.mainTexture = tex_R;
            
            
        }, fail, url);

    }

    public void SetBrowStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        currentStyles[(int)BundlePart.Brow] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Brow, texName, wrapper =>
        {
            succ?.Invoke();
            if(currentStyles[(int)BundlePart.Brow] != texName) return;
            Texture tex_L = wrapper.LoadAsset<Texture>(texName + "_left");
            Texture tex_R = wrapper.LoadAsset<Texture>(texName + "_right");
            if (tex_L == null || tex_R == null)
            {
                return;
            }

            mat_Brow_L.mainTexture = tex_L;
            mat_Brow_R.mainTexture = tex_R;
        }, fail, url);
    }

    public void SetBrowColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            mat_Brow_L.color = color;
            mat_Brow_R.color = color;
        }
    }

    /// <summary>
    ///  设置眼睛瞳孔颜色
    /// </summary>
    public void SetEyePupilColor(string texName, string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            var mpb = new MaterialPropertyBlock();
            mpb.Clear();
            mpb.SetColor("_diffuse_g", color);
            var lRender = mat_Eye_L.GetComponent<MeshRenderer>();
            var rRender = mat_Eye_R.GetComponent<MeshRenderer>();
            lRender.SetPropertyBlock(mpb);
            rRender.SetPropertyBlock(mpb);
        }
    }

    public void SetEyesStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        currentStyles[(int)BundlePart.Eyes] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Eyes, texName, wrapper =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Eyes] != texName) return;
            Texture tex_L = wrapper.LoadAsset<Texture>(texName + "_left");
            Texture tex_R = wrapper.LoadAsset<Texture>(texName + "_right");
            if (tex_L == null || tex_R == null)
            {
                return;
            }
            mat_Eye_L.GetComponent<MeshRenderer>().material.mainTexture = tex_L;
            mat_Eye_R.GetComponent<MeshRenderer>().material.mainTexture = tex_R;
        }, fail, url);
    }


    /// <summary>
    /// 特殊眼睛，需要更换材质
    /// </summary>
    /// <param name="eyeName"></param>
    public void SetSpecialEyesStyle(string eyeName)
    {
        if (RoleMenuView.Ins && RoleMenuView.Ins.roleData.eId == specialEyeId)
        {
            var material_L = LoadMaterial(MAT_PATH + eyeName + "_01_left");
            var material_R = LoadMaterial(MAT_PATH + eyeName + "_01_right");
            if (material_L == null || material_R == null)
            {
                return;
            }
            mat_Eye_L.GetComponent<MeshRenderer>().material = material_L;
            mat_Eye_R.GetComponent<MeshRenderer>().material = material_R;
        }
    }

    public void SetFaceStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        
    }

    public void SetSkinColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            mat_Pattern.color = color;
            mat_Body.color = color;
            mat_Nose.color = color;
        }
    }

    public void SetMouthStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        currentStyles[(int)BundlePart.Mouse] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Mouse, texName, wrapper =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Mouse] != texName) return;
            Texture tex = wrapper.LoadAsset<Texture>(texName);
            if (tex == null)
            {
                return;
            }
            mat_Mouth.mainTexture = tex;
        }, fail, url );
    }

    public void SetNoseStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            succ?.Invoke();
            fbx_Nose.SetActive(false);
            return;
        }
        fbx_Nose.SetActive(true);
        currentStyles[(int)BundlePart.Nose] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Nose, texName, wrapper =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Nose] != texName) return;
            
            Texture tex = wrapper.LoadAsset<Texture>(texName);
            if (tex == null)
            {
                return;
            }
            mat_Nose.mainTexture = tex;
        }, fail, url );
    }

    public void SetNoseChildScale(Vector3 vector)
    {
        child_Nose.transform.localScale = vector;
    }

    public void SetNoseParentScale(Vector3 vector)
    {
        bone_Nose.transform.localScale = vector;
    }

    public void SetNosePos(Vector3 vector)
    {
        bone_Nose.transform.localPosition = vector;
    }
    
    public void SetClothesStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        currentStyles[(int)BundlePart.Clothes] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Clothes, texName, ab =>
        {
            succ?.Invoke();
            if (texName != currentStyles[(int)BundlePart.Clothes]) return;
            Texture tex = ab.LoadAsset<Texture>(texName);
            if (tex == null)
            {
                LoggerUtils.LogError($"not found tex {texName}");
                return;
            }
            
            var sp = texName.Split('_');
            string clothTemplateName = "";
            
            if (sp.Length > 2)
            {
                clothTemplateName = texName.Substring(0, texName.Length - 3);
            }
            else
            {
                clothTemplateName = texName;
            }
            
            mat_Clothes_01.SetActive(true);
            DisableClothesUGCRoot();
            
            GameObject roleCloth = ab.LoadAsset<GameObject>(clothTemplateName).transform.Find(clothTemplateName).gameObject;
            RefreshMaterial(roleCloth.transform);
            if (roleCloth == null)
            {
                LoggerUtils.LogError($"not found roleCloth {clothTemplateName}");
                return;
            }
            
            var fbxMeshCom = roleCloth.GetComponent<SkinnedMeshRenderer>();
            if (fbxMeshCom == null)
            {
                LoggerUtils.LogError($"not found fbxMeshCom in {clothTemplateName}");
                return;
            }
            
            var roleMeshCom = mat_Clothes_01.GetComponent<SkinnedMeshRenderer>();
            if (roleMeshCom == null)
            {
                LoggerUtils.LogError($"not found roleMeshCom in {clothTemplateName}");
                return;
            }

            RefreshGoBones(fbxMeshCom, roleMeshCom);
            RefreshClothesMaterial(roleCloth, tex);

            mat_Clothes_01.ClearChildren();

            CleanScreenShotParticalByType(ClassifyType.outfits);
            var data = RoleConfigDataManager.Inst.CurConfigRoleData.clothes.Find(x => x.texName == texName);
            if (data != null && data.extraType == 1)
            {
                var extraNodeName = string.Format("{0}_{1}", clothTemplateName, "extra");
                var extraNodePrefab = ab.LoadAsset<GameObject>(extraNodeName);
                var extraNode = Instantiate(extraNodePrefab.gameObject, mat_Clothes_01.transform);
                RefreshMaterial(extraNode.transform);

                CreateScreenShotPartical(ClassifyType.outfits, mat_Clothes_01);
            }
        }, fail, url);
    }

    private void CleanScreenShotParticalByType(ClassifyType type)
    {
        if(GameManager.Inst.engineEntry.sceneType != (int)SCENE_TYPE.ROLE_SCENE)
        {
            return;
        }
        if (ParticleNodeDic.ContainsKey(type))
        {
            var partObj = ParticleNodeDic[type];
            Destroy(partObj);
            ParticleNodeDic.Remove(type);
        }
    }

    private void CreateScreenShotPartical(ClassifyType type, GameObject oriParticalObj)
    {
        if (GameManager.Inst.engineEntry.sceneType != (int)SCENE_TYPE.ROLE_SCENE)
        {
            return;
        }
        var particalObj = Instantiate(oriParticalObj, transform.parent);
        ParticleNodeDic[type] = particalObj;
        Destroy(particalObj.GetComponent<SkinnedMeshRenderer>());
        particalObj.transform.localPosition = new Vector3(1000, 1000, 1000);
        particalObj.transform.localScale = new Vector3(10, 10, 10);
        particalObj.transform.eulerAngles = particalObj.transform.eulerAngles + new Vector3(0, -180, 0);
    }

    private void DisableClothesUGCRoot()
    {
        var clothBonelist = CreateUgcBoneDic[(int)UGC_CLOTH_TYPE.CLOTH];
        clothBonelist.ForEach(x =>
        {
            var boneParent = transform.Find("UgcBoneParent" + x).gameObject;
            boneParent.SetActive(false);
        });
        underWear.gameObject.SetActive(false);
    }

    private void RefreshGoBones(SkinnedMeshRenderer fbxMeshCom, SkinnedMeshRenderer roleMeshCom, bool refRoot = false)
    {
        
        List<Transform> clothBones = new List<Transform>();
        if (fbxMeshCom.bones.Length > 0)
        {
            foreach (var roleBone in fbxMeshCom.bones)
            {
                if (BonesDic.ContainsKey(roleBone.name))
                {
                    clothBones.Add(BonesDic[roleBone.name]);
                }
            }
            roleMeshCom.bones = clothBones.ToArray();
            roleMeshCom.sharedMesh = fbxMeshCom.sharedMesh;
            if (refRoot)
            {
                roleMeshCom.rootBone = BonesDic[fbxMeshCom.rootBone.name];
            }
        }
    }

    private void RefreshClothesMaterial(GameObject roleCloth, Texture tex)
    {
        var clothMat = roleCloth.GetComponent<SkinnedMeshRenderer>().sharedMaterial;
        mat_Clothes_01.GetComponent<SkinnedMeshRenderer>().material = clothMat;
        mat_Clothes_01.GetComponent<SkinnedMeshRenderer>().material.mainTexture = tex;
    }
    private void RefreshGoLayer(GameObject go)
    {
        if (curPlayerType == PlayerType.SelfPlayer)
        {
            go.layer = playerLayer;
            foreach (var item in go.GetComponentsInChildren<Transform>())
            {
                item.gameObject.layer = playerLayer;
            }
        }
    }
    
    public void SetUGCClothStyle(ClothStyleData clothData)
    {
        ClothLoadManager.Inst.LoadUGCClothRes(clothData, this);
    }

    

    public void SetShoeStyle(string texName, Action succ = null, Action fail = null, string url = null) 
    {
        var roleMeshCom = shoe.GetComponent<SkinnedMeshRenderer>();
        if (string.IsNullOrEmpty(texName))
        {
            roleMeshCom.enabled = false;
            succ?.Invoke();
            return;
        }

        currentStyles[(int)BundlePart.Shoe] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Shoe, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Shoe] != texName) return;
            Texture texture = ab.LoadAsset<Texture>(texName);
            var shoeData = RoleConfigDataManager.Inst.CurConfigRoleData.shoesStyles.Find(x => x.texName == texName);
            GameObject roleShoe = null;
            if (shoeData!=null)
            {
                roleShoe = ab.LoadAsset<GameObject>(shoeData.modelName);
            }
            if (roleShoe == null)
            {
                LoggerUtils.LogError($"not found shoe {texName}");
                return;
            }
            roleShoe = roleShoe.transform.Find(shoeData.modelName).gameObject;
            
            if (roleShoe==null)
            {
                return;
            }
            var fbxMeshCom = roleShoe.GetComponent<SkinnedMeshRenderer>();
            if (fbxMeshCom==null||roleMeshCom==null)
            {
                return;
            }
            RefreshGoBones(fbxMeshCom, roleMeshCom);
            roleMeshCom.enabled = true;
            var shoeMat = fbxMeshCom.sharedMaterial;
            roleMeshCom.material = fbxMeshCom.sharedMaterial;
            if (texture)
            {
                shoe.SetActive(true);
                roleMeshCom.material.mainTexture = texture;
            }
            
            RefreshMaterial(shoe.transform);
        }, fail, url);
        
        
    }
    

    public void SetHairStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        var roleMeshCom = mat_Hair.GetComponent<SkinnedMeshRenderer>();
        if (string.IsNullOrEmpty(texName))
        {
            //支持光头设置
            roleMeshCom.enabled = false;
            succ?.Invoke();
            return;
        }
        currentStyles[(int)BundlePart.Hair] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Hair, texName, ab =>
        {

            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Hair] != texName) return;
            GameObject roleHair =  ab.LoadAsset<GameObject>(texName);
            if (roleHair == null)
            {
                LoggerUtils.LogError($"not found hair {texName}");
                return;
            }
            roleHair = roleHair.transform.Find(texName).gameObject;
            
            var fbxMeshCom = roleHair.GetComponent<SkinnedMeshRenderer>();
            if (roleHair == null || fbxMeshCom == null || roleMeshCom == null)
            {
                return;
            }
            RefreshGoBones(fbxMeshCom, roleMeshCom);
            roleMeshCom.enabled = true;
            var mat = ab.LoadAsset<Material>("hair"); 
            var curcolor = roleMeshCom.material.GetColor("_Color");
            if (mat == null)
            {
                var defMat = ab.LoadAsset<Material>(texName);
                roleMeshCom.material = defMat;
            }
            else
            {
                roleMeshCom.material = mat;
            }
            RefreshMaterial(roleMeshCom.transform);
            roleMeshCom.material.color = curcolor;
            
            Texture texture = ab.LoadAsset<Texture>(texName + "_normal");
            if (texture)
            {
                roleMeshCom.material.SetTexture("_BumpMap", texture);
            }
            else
            {
                roleMeshCom.material.SetTexture("_BumpMap", null);
            }
            roleMeshCom.material.EnableKeyword("_NORMALMAP");
            
        }, fail, url);
    }

    
    public void SetHairColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            GameObject Hair = transform.Find("hair").gameObject;
            var mat = Hair.GetComponent<Renderer>().material;
            mat.SetColor("_Color", color);
        }
    }

    Transform GetTransform(Transform check, string name)
    {
        Transform forreturn = null;
        foreach (Transform transform in check.GetComponentsInChildren<Transform>())
        {
            if (transform.name == name)
            {
                forreturn = transform;
                return transform;
            }
        }
        return forreturn;
    }

    public void SetEyesPos(Vector3 vector)
    {
        bone_Eye_L.transform.localPosition = vector;
        vector.z = -vector.z;
        bone_Eye_R.transform.localPosition = vector;
    }

    public void SetEyesRot(Vector3 vector)
    {
        Vector3 rot_L = vector;
        Vector3 rot_R = rot_L;
        rot_R.y = -rot_R.y;

        GameObject bone_Rot_L = bone_Eye_L.transform.Find("eye_l_01").gameObject;
        GameObject bone_Rot_R = bone_Eye_R.transform.Find("eye_r_01").gameObject;
        bone_Rot_L.transform.localEulerAngles = rot_L;
        bone_Rot_R.transform.localEulerAngles = rot_R;
    }
    public void SetEyesScale(Vector3 vector)
    {
        bone_Eye_L.transform.localScale = vector;
        bone_Eye_R.transform.localScale = vector;
    }

    public void SetBrowPos(Vector3 vector)
    {
        bone_Brow_L.transform.localPosition = vector;
        vector.z = -vector.z;
        bone_Brow_R.transform.localPosition = vector;
    }

    public void SetHatColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        if (isCus)
        {
            curHatColor = colorHex;
            foreach (var hat in hatStyleDic.Keys)
            {
                var data = RoleConfigDataManager.Inst.CurConfigRoleData.hatStyles.Find(x => x.texName == hat);
                if (data != null)
                {
                    if (data.CantSetColor)
                    {
                        hatStyleDic[hat].GetComponent<Renderer>().material.color = color;
                    }
                }
            }
        }
    }


    public void SetHatStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            foreach (GameObject hatStyle in hatStyleDic.Values)
            {
                hatStyle.SetActive(false);
            }
            succ?.Invoke();
            return;
        }

        currentStyles[(int)BundlePart.Hats] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Hats, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Hats] != texName) return;
            var hatData = RoleConfigDataManager.Inst.CurConfigRoleData.hatStyles.Find(x => x.texName == texName);
            if (hatData == null) return;
            GameObject hatPrefab;
            if (!string.IsNullOrEmpty(hatData.modelName))
            {
                hatPrefab = ab.LoadAsset<GameObject>(hatData.modelName);
            }
            else
            {
                hatPrefab = ab.LoadAsset<GameObject>(hatData.texName);
            }
            
            var hatFbx = hatPrefab.transform.Find(BONE_PATH + "/hat/hat_01_x").gameObject;
            
            foreach (GameObject hatStyle in hatStyleDic.Values)
            {
                hatStyle.SetActive(false);
            }

            if (hatStyleDic.ContainsKey(texName))
            {
                hatStyleDic[texName].SetActive(true);
            }
            else
            {
                GameObject hat = null;
                if (!string.IsNullOrEmpty(hatData.modelName))
                {
                    hat = hatFbx.transform.Find(hatData.modelName).gameObject;
                }
                else
                {
                    hat = hatFbx.transform.Find(texName).gameObject;
                }

                if (hat == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }

                Transform hatParent = bone_Hat.transform.Find("hat_01_x");
                
                hat = Instantiate(hat, hatParent);
                RefreshMaterial(hat.transform);
                RefreshGoLayer(hat);
                
                hatStyleDic.Add(texName, hat);
                
                Texture hatTex = ab.LoadAsset<Texture>(texName);
                do
                {
                    if (hatTex == null)
                    {
                        LoggerUtils.LogError($"SetHatStyle not found tex {texName}");
                        break;
                    }
                    int childcount = hat.transform.childCount;
                    var hatMeshRenderer = hat.GetComponent<MeshRenderer>();

                    if (hatMeshRenderer != null)
                    {
                        hatMeshRenderer.material.mainTexture = hatTex;
                    }
                    else
                    {
                        LoggerUtils.LogError("SetHatStyle not found hatMeshRenderer!");
                    }
                    
                    if (childcount <= 0)
                    {
                        break;
                    }
                    
                    var hatChildMeshRenderer = hat.transform.GetChild(0).gameObject.GetComponent<MeshRenderer>();
                    
                    if (hatChildMeshRenderer == null)
                    {
                        break;
                    }
                    hatChildMeshRenderer.material.mainTexture = hatTex;
                    
                } while (false);

            }

            SetHatColor(curHatColor);
        }, fail, url);
    }

    public void SetHatPos(Vector3 vector)
    {
        bone_Hat.transform.localPosition = vector;
    }

    public void SetHatSca(Vector3 vector)
    {
        bone_Hat.transform.localScale = vector;
    }
    public void SetHatRot(Vector3 rotation)
    {
        var hat = bone_Hat.transform.GetChild(0).transform;
        hat.transform.localEulerAngles = rotation;
    }

    public void SetGlassesColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        curGlassesColor = colorHex;
        if (isCus)
        {
            foreach (var glasses in glassesStyleDic.Keys)
            {
                var data = RoleConfigDataManager.Inst.CurConfigRoleData.glassesStyles.Find(x => x.texName == glasses);
                if (data != null)
                {
                    if (data.CantSetColor)
                    {
                        LoggerUtils.Log("glasses Key = " + glasses);
                        glassesStyleDic[glasses].GetComponent<Renderer>().material.color = color;
                    }
                }
            }
        }
    }
    #region SetEffect
    public void SetEffectStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            foreach (GameObject effectStyle in effectStyleDic.Values)
            {
                effectStyle.SetActive(false);
            }
            if (succ != null)
            {
                succ.Invoke();
            }
            return;
        }

        currentStyles[(int)BundlePart.Effect] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Effect, texName, ab =>
        {
            if (succ != null)
            {
                succ.Invoke();
            }
            var effectData = RoleConfigDataManager.Inst.CurConfigRoleData.effectStyles.Find(x => x.texName == texName);
            if (effectData == null)
            {
                return;
            }
            GameObject effectFbx = GetEffectFbx(texName, ab, effectData);
            if(effectFbx == null)
            {
                return;
            }
            foreach (GameObject effectStyle in effectStyleDic.Values)
            {
                effectStyle.SetActive(false);
            }

            if (effectStyleDic.ContainsKey(effectData.id))
            {
                effectStyleDic[effectData.id].SetActive(true);
            }
            else
            {
                Transform effectParent = effectFbx.transform.parent;
                var childs = transform.GetComponentsInChildren<Transform>();
                GameObject playerEffPar = null;
                for (int i = 0; i < childs.Length; i++)
                {
                    if(childs[i].name == effectParent.name)
                    {
                        playerEffPar = childs[i].gameObject;
                        break;
                    }
                }
                if(playerEffPar == null)
                {
                    LoggerUtils.Log("No parent node found on player");
                    return;
                }
                GameObject effectTmp = effectFbx.transform.Find(effectData.modelName).gameObject;
                if (effectTmp == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }
                SetBoneTrans(effectTmp, effectFbx, effectData, playerEffPar);
            }
        }, fail, url);
    }

    private GameObject GetEffectFbx(string texName,BundleWrapper ab,EffectStyleData effectData)
    {
        if (currentStyles[(int)BundlePart.Effect] != texName)
        {
            return null;
        }

        if (string.IsNullOrEmpty(effectData.modelName))
        {
            return null;
        }
        GameObject effectPrefab = ab.LoadAsset<GameObject>(effectData.modelName);
        var trans = effectPrefab.GetComponentsInChildren<Transform>();
        GameObject effectFbx = null;
        string[] effName = texName.Split('_');
        foreach (var item in trans)
        {
            if (item.name == effName[0])
            {
                effectFbx = item.gameObject;
                break;
            }
        }
        return effectFbx;
    }

    private GameObject GetBoneEffect()
    {
        if(bone_Effect == null)
        {
            bone_Effect = new GameObject("effect");
            bone_Effect.transform.localPosition = Vector3.zero;
            bone_Effect.transform.localScale = Vector3.one;
            bone_Effect.transform.localEulerAngles = Vector3.zero;
        }
        return bone_Effect;
    }

    private GameObject GetBoneEffectX()
    {
        if(bone_Effect_x == null)
        {
            bone_Effect_x = new GameObject("effect_x");
            var boneEff = GetBoneEffect();
            bone_Effect_x.transform.SetParent(boneEff.transform);
            bone_Effect_x.transform.localPosition = Vector3.zero;
            bone_Effect_x.transform.localScale = Vector3.one;
            bone_Effect_x.transform.localEulerAngles = Vector3.zero;
        }
        return bone_Effect_x;
    }

    private void SetBoneTrans(GameObject effectTmp, GameObject effectFbx, EffectStyleData effectData,GameObject playerEffPar)
    {
        var boneEffx = GetBoneEffectX();
        GameObject effect = Instantiate(effectTmp, boneEffx.transform);
        RefreshGoLayer(effect);
        effectStyleDic.Add(effectData.id, effect);
        var boneEff = GetBoneEffect();
        var boneOriPos = boneEff.transform.localPosition;
        var boneOriSca = boneEff.transform.localScale;
        boneEff.transform.SetParent(playerEffPar.transform);
        boneEff.transform.localPosition = boneOriPos;
        boneEff.transform.localScale = boneOriSca;
        boneEff.transform.localEulerAngles = effectFbx.transform.localEulerAngles;
        RefreshMaterial(effect.transform);
    }

    public void SetEffectPos(Vector3 vector)
    {
        var boneEff = GetBoneEffect();
        boneEff.transform.localPosition = vector;
    }

    public void SetEffectSca(Vector3 vector)
    {
        var boneEff = GetBoneEffect();
        boneEff.transform.localScale = vector;
    }

    public void SetEffectRot(Vector3 rotation)
    {
        var boneEffx = GetBoneEffectX();
        boneEffx.transform.localEulerAngles = rotation;
    }
    #endregion
    public void SetGlassesStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            foreach (GameObject glassesStyle in glassesStyleDic.Values)
            {
                glassesStyle.SetActive(false);
            }
            succ?.Invoke();
            return;
        }

        currentStyles[(int)BundlePart.Glasses] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Glasses, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Glasses] != texName) return;
            GameObject fbxGlasses;
            var data = RoleConfigDataManager.Inst.CurConfigRoleData.glassesStyles.Find(x => x.texName == texName);
            if (!string.IsNullOrEmpty(data.modelName))
            {
                fbxGlasses = ab.LoadAsset<GameObject>(data.modelName).transform.Find(BONE_PATH + "/glasses").gameObject;   
            }
            else
            {
                fbxGlasses = ab.LoadAsset<GameObject>(data.texName).transform.Find(BONE_PATH + "/glasses").gameObject;   
            }

            foreach (GameObject glassesStyle in glassesStyleDic.Values)
            {
                glassesStyle.SetActive(false);
            }
            
            GameObject glasses = null;
            if (glassesStyleDic.ContainsKey(texName))
            {
                glassesStyleDic[texName].SetActive(true);
                glasses = glassesStyleDic[texName];
            }
            else
            {          
                if (!string.IsNullOrEmpty(data.modelName))
                {
                    if (glassesStyleDic.ContainsKey(data.modelName))
                    {
                        glassesStyleDic[data.modelName].SetActive(true);
                        glasses = glassesStyleDic[data.modelName];
                    }
                    else
                    {
                        glasses = fbxGlasses.transform.Find(data.modelName).gameObject;
                        glasses = Instantiate(glasses, bone_Glasses.transform);
                        glassesStyleDic.Add(data.modelName, glasses);
                    }              
                }             
                else
                {
                    glasses = fbxGlasses.transform.Find(texName).gameObject;
                    glasses = Instantiate(glasses, bone_Glasses.transform);
                }
                if (glasses == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }
                RefreshGoLayer(glasses);
                if (!glassesStyleDic.ContainsKey(texName))
                {
                    glassesStyleDic.Add(texName, glasses);
                }     
                var glMeshCom = glasses.GetComponentInChildren<SkinnedMeshRenderer>();
                if (glMeshCom != null)
                {
                    List<Transform> transforms = new List<Transform>();
                    transforms.Add(BonesDic["glasses"]);
                    glMeshCom.bones = transforms.ToArray();
                    glMeshCom.rootBone = BonesDic["glasses"];
                }
            }

            RefreshMaterial(glasses.transform);
            
            Texture glassTex = ab.LoadAsset<Texture>(texName);
            if (glassTex != null)
            {
                glasses.GetComponent<MeshRenderer>().material.mainTexture = glassTex;
                
            }
            else
            {
                glasses.GetComponent<MeshRenderer>().material.mainTexture = null;
            }
            
            if (data != null)
            {
                if (data.CantSetColor)
                {
                    SetGlassesColor(curGlassesColor);
                }
            }
        }, fail, url);
        
    }

    
    //shader不在内存直接加载会有问题 这边刷新一下
    private void RefreshMaterial(Transform t)
    {
        var mrs = t.GetComponentsInChildren<Renderer>();
        foreach (var m in mrs)
        {
            var mt = m.sharedMaterial;
            if (mt != null && mt.shader != null)
            {
                var rq = mt.renderQueue;
                mt.shader = Shader.Find(mt.shader.name);
                mt.renderQueue = rq;
            }
        }
    }

    public void SetGlassesPos(Vector3 vector)
    {
        bone_Glasses.transform.localPosition = vector;
    }

    public void SetGlassesSca(Vector3 scale)
    {
        bone_Glasses.transform.localScale = scale;
    }
    public void SetGlassesRot(Vector3 rotation)
    {
        bone_Glasses.transform.localEulerAngles = rotation;
    }
    public void SetAccessoriesStyles(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            accessories.SetActive(false);
            succ?.Invoke();
            return;
        }
        currentStyles[(int)BundlePart.Accessoies] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Accessoies, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Accessoies] != texName) return;
            Texture tex = ab.LoadAsset<Texture>(texName);
            if (tex == null)
            {
                return;
            }

            var accessoriesData =
                RoleConfigDataManager.Inst.CurConfigRoleData.accessoriesStyles.Find(x => x.texName == texName);
            GameObject roleAccessories = null;
            if (accessoriesData != null)
            {
                roleAccessories = ab.LoadAsset<GameObject>(accessoriesData.modelName);
            }

            if (roleAccessories == null)
            {
                LoggerUtils.LogError($"not found accessories {texName}");
                return;
            }
            
            roleAccessories = roleAccessories.transform.Find(accessoriesData.modelName).gameObject;
            
            if (roleAccessories == null)
            {
                return;
            }

            var fbxMeshCom = roleAccessories.GetComponent<SkinnedMeshRenderer>();
            var roleMeshCom = accessories.GetComponent<SkinnedMeshRenderer>();
            if (fbxMeshCom == null || roleMeshCom == null)
            {
                return;
            }
            RefreshGoBones(fbxMeshCom, roleMeshCom);
            roleMeshCom.material = fbxMeshCom.sharedMaterial;
            if (tex)
            {
                accessories.SetActive(true);
                RefreshGoLayer(accessories);
                roleMeshCom.material.mainTexture = tex;
            }
        },fail, url);
    }

    #region 背包
    public void SetBagStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            foreach (GameObject bagStyle in bagStyleDic.Values)
            {
                bagStyle.SetActive(false);
            }
            succ?.Invoke();
            return;
        }
        var data = RoleConfigDataManager.Inst.CurConfigRoleData.bagStyles.Find(x => x.texName == texName);
        currentStyles[(int)BundlePart.Bag] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Bag, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Bag] != texName) return;
            var fbxBag = ab.LoadAsset<GameObject>(data.modelName).transform.Find(BAG_PATH).gameObject;
            foreach (GameObject bagStyle in bagStyleDic.Values)
            {
                bagStyle.SetActive(false);
            }
            GameObject bag = null;
            if (bagStyleDic.ContainsKey(data.id))
            {
                bagStyleDic[data.id].SetActive(true);
                bag = bagStyleDic[data.id];
            }
            else
            {
                bag = fbxBag.transform.Find(data.modelName).gameObject;
                if (bag == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }
                bag = Instantiate(bag, bone_Bag.transform);
                RefreshMaterial(bag.transform);
                RefreshGoLayer(bag);
                bagStyleDic[data.id] = bag;
            }
            Texture bagTex = ab.LoadAsset<Texture>(texName);
            Texture bagNormalTex = ab.LoadAsset<Texture>(texName+ "_normal");
            if (bagTex)
            {
                bag.GetComponent<MeshRenderer>().material.mainTexture = bagTex;
            }
            if (bagNormalTex)
            {
                bag.GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", bagNormalTex);
            }
            else
            {
                bag.GetComponent<MeshRenderer>().material.SetTexture("_BumpMap", null);
            }
            if (data.CantSetColor)
            {
                SetBagColor(curBagColor);
            }
        }, fail, url);


    }
    public void SetCroddBodyStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            Crossbody.SetActive(false);
            succ?.Invoke();
            return;
        }
        var data = RoleConfigDataManager.Inst.CurConfigRoleData.bagStyles.Find(x => x.texName == texName);
        currentStyles[(int)BundlePart.Crossbody] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Crossbody, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Crossbody] != texName) return;
            else
            {
                var crossbodyMod = ab.LoadAsset<GameObject>(data.modelName).transform.Find(data.modelName).gameObject;
                if (crossbodyMod == null)
                {
                    LoggerUtils.Log("model no Find:" + data.modelName);
                    return;
                }
                RefreshMaterial(Crossbody.transform);
                RefreshGoLayer(Crossbody);
                var fbxMeshCom = crossbodyMod.GetComponent<SkinnedMeshRenderer>();
                var roleMeshCom = Crossbody.GetComponent<SkinnedMeshRenderer>();
                if (fbxMeshCom == null || roleMeshCom == null)
                {
                    return;
                }
                RefreshGoBones(fbxMeshCom, roleMeshCom);
                Texture tex = ab.LoadAsset<Texture>(texName);
                if (tex)
                {
                    roleMeshCom.material.mainTexture = tex;
                }
                Crossbody.SetActive(true);
            }
        }, fail, url);
    }

    public void SetBagColor(string colorHex)
    {
        Color color;
        bool isCus = ColorUtility.TryParseHtmlString(colorHex, out color);
        curBagColor = colorHex;
        if (isCus)
        {
            foreach (var bagId in bagStyleDic.Keys)
            {
                var data = RoleConfigDataManager.Inst.CurConfigRoleData.bagStyles.Find(x => x.id == bagId);
                if (data != null)
                {
                    if (data.CantSetColor)
                    {
                        bagStyleDic[bagId].GetComponent<Renderer>().material.color = color;
                    }
                }
            }
        }
    }
    public void SetBagPos(Vector3 vector)
    {
        bone_Bag.transform.localPosition = vector;
    }
    public void SetBagSca(Vector3 scale)
    {
        bone_Bag.transform.localScale = scale;
    }
    public void SetBagRot(Vector3 rotation)
    {
        bone_Bag.transform.localEulerAngles = rotation;
    }
    #endregion
    #region 面部彩绘
    /// <summary>
    /// 设置ugc面部彩绘与pgc面部彩绘的互斥关系
    /// </summary>
    /// <param name="isUgc">当前设置的类型是否为ugc</param>
    public void SetPatternMutual()
    {
        SetPatternStyle("patterns_00");//将PGC面部彩绘置空
        ugc_face.SetActive(true);
    }
    public void SetPatternStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        string texN = string.IsNullOrEmpty(texName) ? "patterns_00" : texName;
        ugc_face.SetActive(false);
        currentStyles[(int)BundlePart.Pattern] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Pattern, texN, wrapper =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Pattern] != texName) return;
            Texture tex = wrapper.LoadAsset<Texture>(texN);
            if (tex == null)
            {
                return;
            }
            mat_Pattern.SetTexture("_patterns_tex", tex);
        }, fail, url );
    }
    public void SetUgcPatternStyle(PatternStyleData patternStyleData, Action onSuccess = null, Action onFail = null)
    {
        LoadUgcResData loadUgcResData = new LoadUgcResData()
        {
            clothesUrl = patternStyleData.patternUrl,
            templateId = patternStyleData.templateId,
        };
        ClothLoadManager.Inst.LoadUGCPatternsRes(loadUgcResData, this.gameObject, () => { SetPatternMutual(); onSuccess?.Invoke(); }, onFail);
    }
    private Material GetPatternMat(bool isPgc)
    {
        return isPgc ? mat_Pattern : ugc_face.GetComponent<MeshRenderer>().material;
    }
    public void SetPatternPos(Vector3 vector, bool isPGC)
    {
        var mat = GetPatternMat(isPGC);
        if (mat)
        {
            GetPatternMat(isPGC)?.SetTextureOffset("_patterns_tex", vector);
        }
    }
    public void SetPatternScale(Vector3 vector, bool isPGC)
    {
        var mat = GetPatternMat(isPGC);
        if (mat)
        {
            GetPatternMat(isPGC)?.SetFloat("_patterms_size", vector.x);
        }
    }
    #endregion

    public void SetHandStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        foreach (var key in handStyleDic.Keys)
        {
            var lHandGO = handStyleDic[key][0];
            var rHandGO = handStyleDic[key][1];
            if (lHandGO) lHandGO.SetActive(false);
            if (rHandGO) rHandGO.SetActive(false);
        }
        if (string.IsNullOrEmpty(texName))
        {
            succ?.Invoke();
            return;
        }
        var data = RoleConfigDataManager.Inst.CurConfigRoleData.handStyles.Find(x => x.texName == texName);
        currentStyles[(int)BundlePart.Hand] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Hand, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Hand] != texName) return;
            string[] strs = texName.Split('_');
            string handKey = strs[0] + "_" + strs[1];
            Texture handTex = ab.LoadAsset<Texture>(texName);
            var handFbx = ab.LoadAsset<GameObject>(handKey);
            if (!handStyleDic.ContainsKey(data.id))
            {
                string path_l = "";
                string path_r = "";
                switch (data.handBipType)
                {
                    case (int)HandBipType.Arm:
                        path_l = HAND_L_ARMGLOVE_PATH + "/" + handKey + "_l";
                        path_r = HAND_R_ARMGLOVE_PATH + "/" + handKey + "_r";
                        break;
                    case (int)HandBipType.Glove:
                        path_l = HAND_L_GLOVE_PATH + "/" + handKey + "_l";
                        path_r = HAND_R_GLOVE_PATH + "/" + handKey + "_r";
                        break;
                }
                var handTF_l = handFbx.transform.Find(path_l);
                var handTF_r = handFbx.transform.Find(path_r);
                if (handTF_l == null && handTF_r == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }

                handStyleDic.Add(data.id, new GameObject[2]);
                if (handTF_l)
                {
                    var parent_l = GetHandTrs(data.handBipType, 0);
                    //var handL = Instantiate(handTF_l.gameObject, parent_l);
                    var handL = InstantiateHand(handTF_l.gameObject, parent_l);
                    RefreshMaterial(handL.transform);
                    if (curPlayerType == PlayerType.SelfPlayer)
                    {
                        GameUtils.ChangeToTargetLayer(LayerMask.LayerToName(playerLayer), handL.transform);
                    }
                    handStyleDic[data.id][0] = handL;
                }
                if (handTF_r)
                {
                    var parent_r = GetHandTrs(data.handBipType, 1);
                    //var handR = Instantiate(handTF_r.gameObject, parent_r);
                    var handR = InstantiateHand(handTF_r.gameObject, parent_r);
                    RefreshMaterial(handR.transform);
                    if (curPlayerType == PlayerType.SelfPlayer)
                    {
                        GameUtils.ChangeToTargetLayer(LayerMask.LayerToName(playerLayer), handR.transform);
                    }
                    handStyleDic[data.id][1] = handR;
                }
                
            }
            var lHand = handStyleDic[data.id][0];
            if (lHand)
            {
                lHand.SetActive(true);
                lHand.GetComponentInChildren<MeshRenderer>().material.mainTexture = handTex;
            }
            var rHand = handStyleDic[data.id][1];
            if (rHand)
            {
                rHand.SetActive(true);
                rHand.GetComponentInChildren<MeshRenderer>().material.mainTexture = handTex;
            }
            if(handPropCreated != null)
            {
                handPropCreated.Invoke(data.id);
                handPropCreated = null;
            }            
        },fail, url);
        
        

    }

    
    /// <summary>
    /// 获取手部装饰节点
    /// </summary>
    /// <param name="handBipType">节点类型</param>
    /// <param name="type">左右区分（0-左，1-右）</param>
    /// <returns></returns>
    public Transform GetHandTrs(int handBipType, int type)
    {
        switch (handBipType)
        {
            case (int)HandBipType.Arm:
                return type == 0 ? bone_handarm_l.transform : bone_handarm_r.transform;
            case (int)HandBipType.Glove:
                return type == 0 ? bone_hand_l.transform : bone_hand_r.transform;
            default:
                return null;
        }
    }
    
    
    public void SetHandScale(Vector3 vector, int handBipType)
    {
        var transformL = GetHandTrs(handBipType, 0);
        var transformR = GetHandTrs(handBipType, 1);
        transformL.localScale = vector;
        transformR.localScale = vector;
    }
    
    public void SetHandRot(Vector3 rotation, int handBipType)
    {
        //手部两个挂点默认位置不一样，需要做兼容
        // var GloveDefRot = new Vector3(0, 0, 0);
        // var ArmDefRot = new Vector3(0, -90, 0);
        // if (rotation == GloveDefRot && handBipType == (int)HandBipType.Arm)
        // {
        //     rotation = ArmDefRot;
        // }
        var transformL = GetHandTrs(handBipType, 0);
        var transformR = GetHandTrs(handBipType, 1);
        transformL.localEulerAngles = rotation;
        // rotation.x = -rotation.x;
        // rotation.y = -rotation.y;
        transformR.localEulerAngles = rotation;
    }
    public void SetHandPos(Vector3 pos, int handBipType)
    {
        var transformL = GetHandTrs(handBipType, 0);
        var transformR = GetHandTrs(handBipType, 1);
        transformL.localPosition = pos;
        transformR.localPosition = pos;
    }

    public void SetHandLeftRight(int handId, HandLRType type)
    {
        if (!(type == HandLRType.Left || type == HandLRType.Right)) return;
        var data = RoleConfigDataManager.Inst.GetHandStyleDataById(handId);
        if (data == null || string.IsNullOrEmpty(data.modelName)) return;

        if (handStyleDic.TryGetValue(data.id, out GameObject[] gos))
        {
            Transform parentL = GetHandTrs(data.handBipType, 0);
            Transform parentR = GetHandTrs(data.handBipType, 1);
            GameObject left = gos[0];
            GameObject right = gos[1];
            GameObject changedHand = null;
            if(type == HandLRType.Left && right != null)
            {
                right.transform.SetParent(parentL);
                changedHand = right;
            }
            else if (type == HandLRType.Right && left != null)
            {
                left.transform.SetParent(parentR);
                changedHand = left;
            }
            if (changedHand != null)
            {
                
                changedHand.transform.localPosition = Vector3.zero;
                if((type == HandLRType.Left && data.leftRightType == (int)HandLRType.Right)
                    || (type == HandLRType.Right && data.leftRightType == (int)HandLRType.Left))
                {
                    changedHand.transform.localRotation = Quaternion.Euler(new Vector3(0f, 0f, 180f));
                }
                else
                {
                    changedHand.transform.localRotation = Quaternion.identity;
                }
                gos[0] = right;
                gos[1] = left;
            }
        }
    }


    public void SwitchHand(int handId)
    {
        var data = RoleConfigDataManager.Inst.GetHandStyleDataById(handId);
        if (data == null || string.IsNullOrEmpty(data.modelName)) return;
        
        if(handStyleDic.TryGetValue(data.id, out GameObject[] gos))
        {   
            GameObject left = gos[0];
            GameObject right = gos[1];
            if(left != null)
            {
                SetHandLeftRight(handId, HandLRType.Right);
            }
            else if(right != null)
            {
                SetHandLeftRight(handId, HandLRType.Left);
            }
        }
    }

    public void SetSpecialStyle(string texName, Action succ = null, Action fail = null, string url = null)
    {
        if (string.IsNullOrEmpty(texName))
        {
            foreach (GameObject specialStyle in specialStyleDic.Values)
            {
                specialStyle.SetActive(false);
            }
            succ?.Invoke();
            return;
        }
        var data = RoleConfigDataManager.Inst.CurConfigRoleData.specialStyles.Find(x => x.texName == texName);
        foreach (GameObject specialStyle in specialStyleDic.Values)
        {
            specialStyle.SetActive(false);
        }

        currentStyles[(int)BundlePart.Special] = texName;
        BundleMgr.Inst.LoadBundle(BundlePart.Special, texName, ab =>
        {
            succ?.Invoke();
            if (currentStyles[(int)BundlePart.Special] != texName) return;
            GameObject fbxSpecial;
            if (!string.IsNullOrEmpty(data.modelName))
            {
                fbxSpecial = ab.LoadAsset<GameObject>(data.modelName);
            }
            else
            {
                fbxSpecial = ab.LoadAsset<GameObject>(data.texName);
            }
            
            var special = fbxSpecial.transform.Find("Bip001/Bip001 Pelvis/Bip001 Spine/waist/waist_x/waist_01").gameObject;
            
            if (specialStyleDic.ContainsKey(texName))
            {
                specialStyleDic[texName].SetActive(true);
            }
            else
            {
                Transform specialParent = bone_special.transform.Find("waist_x");
                var sp = Instantiate(special, specialParent);
                if (sp == null)
                {
                    LoggerUtils.Log("model no Find");
                    return;
                }
                specialStyleDic.Add(texName, sp);
                if (curPlayerType == PlayerType.SelfPlayer)
                {
                    sp.layer = playerLayer;
                }

                Texture specialTex = ab.LoadAsset<Texture>(texName);
                if (specialTex)
                {
                    sp.GetComponent<MeshRenderer>().material.mainTexture = specialTex;
                }
            }
            
        }, fail, url);
        
    }

    public void SetSpecialPos(Vector3 vector)
    {
        bone_special.transform.localPosition = vector;
    }
    public void SetSpecialRot(Vector3 rotation)
    {
        var transform = bone_special.transform.GetChild(0).transform;
        transform.localEulerAngles = rotation;
    }

    public void SetSpecialScale(Vector3 vector)
    {
        bone_special.transform.localScale = vector;
    }

    public void SetFacialDefaultPos()
    {
        RoleData defaultRoleData = RoleConfigDataManager.Inst.GetDefaultRoleData();
        SetNoseStyle("");
        SetEyesPos(defaultRoleData.eP);
        SetEyesRot(defaultRoleData.eR);
        SetEyesScale(defaultRoleData.eS);
        SetBrowPos(defaultRoleData.bP);
        SetBrowRot(defaultRoleData.bR);
        SetBrowSca(defaultRoleData.bS);
        SetMouthPos(defaultRoleData.mP);
        SetMouthRot(defaultRoleData.mR);
        SetMouthSca(defaultRoleData.mS);
    }

    public void SetCustomDefaultPos()
    {
        SetEyesPos(customRoleData.eP);
        SetEyesRot(customRoleData.eR);
        SetEyesScale(customRoleData.eS);
        SetBrowPos(customRoleData.bP);
        SetBrowRot(customRoleData.bR);
        SetBrowSca(customRoleData.bS);
        SetMouthPos(customRoleData.mP);
        SetMouthRot(customRoleData.mR);
        SetMouthSca(customRoleData.mS);
        var noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(customRoleData.nId);
        if (noseData!=null)
        {
            SetNoseStyle(noseData.texName);
        }
        ResetFaceMaterials();   
    }

    public void ResetFaceMaterials()
    {
        if(head == null)
        {
            return;
        }

        head.transform.Find("mouth/body_mouth").gameObject.GetComponent<MeshRenderer>().material = mat_Mouth;
        head.transform.Find("brow_l/brow_l_01/body_brow_l").gameObject.GetComponent<MeshRenderer>().material = mat_Brow_L;
        head.transform.Find("brow_r/brow_r_01/body_brow_r").gameObject.GetComponent<MeshRenderer>().material = mat_Brow_R;
        fbx_Nose.GetComponent<MeshRenderer>().material = mat_Nose;
    }

    public void SetBrowRot(Vector3 vector)
    {
        Vector3 rot_L = vector;
        Vector3 rot_R = rot_L;
        rot_R.y = -rot_R.y;

        GameObject bone_Rot_L = bone_Brow_L.transform.Find("brow_l_01").gameObject;
        GameObject bone_Rot_R = bone_Brow_R.transform.Find("brow_r_01").gameObject;
        bone_Rot_L.transform.localEulerAngles = rot_L;
        bone_Rot_R.transform.localEulerAngles = rot_R;
    }
    public void SetBrowSca(Vector3 scale)
    {
        bone_Brow_L.transform.localScale = scale;
        bone_Brow_R.transform.localScale = scale;
    }

    public void SetMouthPos(Vector3 vector)
    {
        bone_Mouth.transform.localPosition = vector;
    }
    public void SetMouthRot(Vector3 rotation)
    {
        bone_Mouth.transform.localEulerAngles = rotation;
    }
    public void SetMouthSca(Vector3 scale)
    {
        bone_Mouth.transform.localScale = scale;
    }
    public void SetBlushPos(Vector3 pos)
    {
        bone_Blusher_L.transform.localPosition = pos;
        pos.z = -pos.z;
        bone_Blusher_R.transform.localPosition = pos;
    }
    public void SetBlushSca(Vector3 scale)
    {
        bone_Blusher_L.transform.localScale = scale;
        bone_Blusher_R.transform.localScale = scale;
    }
    public void InitRoleByData(RoleData roleData)
    {
        InitRoleByData(roleData,InitRoleType.Normal);
    }
    public void InitRoleByData(RoleData roleData,InitRoleType initRoleType)
    {
        LoggerUtils.Log("InitRoleByData");
        if (BonesDic.Count <= 0)
        {
            InitRoleData();
        }

        var isLegal = RoleDataVerify.CheckRoleDataIsLegal(roleData);
        if (!isLegal)
        {
            roleData = RoleDataVerify.DefRoleData;
            LoggerUtils.LogError("RoleData is Error ==> Uid = " + GameManager.Inst.ugcUserInfo.uid + " |RoleData = " + GameManager.Inst.ugcUserInfo.imageJson + " |Time = " + GameUtils.GetTimeStamp());
        }
        customRoleData = roleData;
        var defRoleConfigData = RoleConfigDataManager.Inst.defRoleConfigData;

        EyeStyleData eyeStyleData = RoleConfigDataManager.Inst.GetEyeStyleDataById(roleData.eId);

        BrowStyleData browStyleData = RoleConfigDataManager.Inst.GetBrowStyleDataById(roleData.bId);

        MouseStyleData mouseStyleData = RoleConfigDataManager.Inst.GetMouseStyleDataById(roleData.mId);

        var hairData = RoleConfigDataManager.Inst.GetHairDataById(roleData.hId);
        if (hairData == null)
        {
            hairData = RoleConfigDataManager.Inst.GetHairDataById(defRoleConfigData.hId);
        }
        if (hairData != null)
        {
            SetHairStyle(hairData.texName);
        }
        // else
        // {
        //     LoggerUtils.Log("hairData == null, hId:" + roleData.hId);
        // }

        SetHairColor(roleData.hCr);

        SetSkinColor(roleData.sCr);
        if (eyeStyleData == null)
        {
            eyeStyleData = RoleConfigDataManager.Inst.GetEyeStyleDataById(defRoleConfigData.eId);
        }

        SetEyesStyle(eyeStyleData.texName);
        
        if (roleData.eId == specialEyeId)
        {
            SetEyePupilColor(eyeStyleData.texName, roleData.eCr);
        }
        SetEyesPos(roleData.eP);
        SetEyesRot(roleData.eR);
        SetEyesScale(roleData.eS);
        if (browStyleData == null)
        {
            browStyleData = RoleConfigDataManager.Inst.GetBrowStyleDataById(defRoleConfigData.bId);
        }

        SetBrowStyle(browStyleData.texName);
        SetBrowColor(roleData.bCr);
        SetBrowPos(roleData.bP);
        SetBrowRot(roleData.bR);
        SetBrowSca(roleData.bS);


        var noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(roleData.nId);
        if (noseData == null)
        {
            noseData = RoleConfigDataManager.Inst.GetNoseStyleDataById(defRoleConfigData.nId);
        }
        if (noseData != null) {
            SetNoseStyle(noseData.texName);
            SetNoseChildScale(roleData.nCS);
            SetNoseParentScale(roleData.nPS);
            SetNosePos(roleData.nP);
        }

        if (mouseStyleData == null)
        {
            mouseStyleData = RoleConfigDataManager.Inst.GetMouseStyleDataById(defRoleConfigData.mId);
        }

        SetMouthStyle(mouseStyleData.texName);
        SetMouthPos(roleData.mP);
        SetMouthRot(roleData.mR);
        SetMouthSca(roleData.mS);

        var blusherData = RoleConfigDataManager.Inst.GetBlusherStyleDataById(roleData.bluId);
        if (blusherData == null)
        {
            blusherData = RoleConfigDataManager.Inst.GetBlusherStyleDataById(defRoleConfigData.bId);
        }
        if (blusherData != null)
        {
            SetBlusherStyle(blusherData.texName);
            SetBlusherColor(roleData.bluCr);
            SetBlushPos(roleData.bluP);
            SetBlushSca(roleData.bluS);
        }

        if (initRoleType!=InitRoleType.UGCPlayerModelCloth)
        {
            var clothesData = RoleConfigDataManager.Inst.GetClothesById(roleData.cloId);
            if (clothesData == null)
            {
                clothesData = RoleConfigDataManager.Inst.GetClothesById(defRoleConfigData.cloId);
            }
            if (clothesData.IsPGC())
            {
                SetClothesStyle(clothesData.texName);
            }
            else
            {
                clothesData.clothesJson = roleData.clothesJson;
                clothesData.clothesUrl = roleData.clothesUrl;
                clothesData.clothMapId = roleData.clothMapId;
                SetUGCClothStyle(clothesData);
            }
        }
        if (initRoleType != InitRoleType.UGCPlayerModelFace)
        {
            var patternData = RoleConfigDataManager.Inst.GetPatternStylesDataById(roleData.fpId);
            if (patternData == null)
            {
                patternData = RoleConfigDataManager.Inst.GetPatternStylesDataById(defRoleConfigData.fpId);
            }
            if (patternData.IsPGC())
            {
                SetPatternStyle(patternData.texName);
            }
            else
            {
                patternData.patternJson = roleData.ugcFPData.ugcJson;
                patternData.patternUrl = roleData.ugcFPData.ugcUrl;
                patternData.patternMapId = roleData.ugcFPData.ugcMapId;
                SetUgcPatternStyle(patternData);
            }
            SetPatternPos(roleData.fpP, patternData.IsPGC());
            SetPatternScale(roleData.fpS, patternData.IsPGC());
        }

        var glassesData = RoleConfigDataManager.Inst.GetGlassesStyleDataById(roleData.glId);
        if (glassesData == null)
        {
            glassesData = RoleConfigDataManager.Inst.GetGlassesStyleDataById(defRoleConfigData.glId);
        }
        if(glassesData != null) {
            SetGlassesStyle(glassesData.texName);
            SetGlassesColor(roleData.glCr);
            SetGlassesPos(roleData.glP);
            SetGlassesRot(roleData.glR);
            SetGlassesSca(roleData.glS);
        }

        var assessoriesData= RoleConfigDataManager.Inst.GetAccessoriesStylesDataById(roleData.acId);
        if (assessoriesData == null)
        {
            assessoriesData = RoleConfigDataManager.Inst.GetAccessoriesStylesDataById(defRoleConfigData.acId);
        }
        if (assessoriesData != null)
        {
            SetAccessoriesStyles(assessoriesData.texName);
        }

        var hatData = RoleConfigDataManager.Inst.GetHatStyleDataById(roleData.hatId);
        if (hatData == null)
        {
            hatData = RoleConfigDataManager.Inst.GetHatStyleDataById(defRoleConfigData.hatId);
        }
        if(hatData != null)
        {
            SetHatStyle(hatData.texName);
            SetHatColor(roleData.hatCr);
            SetHatPos(roleData.hatP);
            SetHatRot(roleData.hatR);
            SetHatSca(roleData.hatS);
        }

        var shoesData = RoleConfigDataManager.Inst.GetShoeStylesDataById(roleData.shoeId);
        if (shoesData == null)
        {
            shoesData = RoleConfigDataManager.Inst.GetShoeStylesDataById(defRoleConfigData.shoeId);
        }
        if (shoesData != null)
        {
            SetShoeStyle(shoesData.texName);
        }
        var bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(roleData.bagId);
        if (bagData == null)
        {
            bagData = RoleConfigDataManager.Inst.GetBagStylesDataById(defRoleConfigData.bagId);
        }
        if (bagData != null)
        {
            SetBagStyle(bagData.texName);
            SetBagColor(roleData.bagCr);
            SetBagPos(roleData.bagP);
            SetBagRot(roleData.bagR);
            SetBagSca(roleData.bagS);
        }
        var CrossBodyData = RoleConfigDataManager.Inst.GetBagStylesDataById(roleData.cbId);
        if (CrossBodyData == null)
        {
            CrossBodyData = RoleConfigDataManager.Inst.GetBagStylesDataById(defRoleConfigData.cbId);
        }
        if (CrossBodyData != null)
        {
            SetCroddBodyStyle(CrossBodyData.texName);
        }
      
            
        var specialData = RoleConfigDataManager.Inst.GetSpecialStylesDataById(roleData.saId);
        if (specialData == null)
        {
            specialData = RoleConfigDataManager.Inst.GetSpecialStylesDataById(defRoleConfigData.saId);
        }
        if (specialData != null)
        {
            SetSpecialStyle(specialData.texName);
            SetSpecialPos(roleData.saP);
            SetSpecialRot(roleData.saR);
            SetSpecialScale(roleData.saS);
        }

        var handData = RoleConfigDataManager.Inst.GetHandStyleDataById(roleData.hdId);
        if (handData == null)
        {
            handData = RoleConfigDataManager.Inst.GetHandStyleDataById(defRoleConfigData.hdId);
        }
        if (handData != null)
        {
            SetHandCreateListener((id)=>SetHandLeftRight(id, GetTargetHandOnLoad(roleData.hdLR, handData.leftRightType)));
            SetHandStyle(handData.texName);
            SetHandScale(roleData.hdS, handData.handBipType);
            SetHandPos(roleData.hdP, handData.handBipType);
            SetHandRot(roleData.hdR, handData.handBipType);
        }

        var effectData = RoleConfigDataManager.Inst.GetEffectStyleDataById(roleData.effId);
        if (effectData == null)
        {
            effectData = RoleConfigDataManager.Inst.GetEffectStyleDataById(defRoleConfigData.effId);
        }
        if (effectData != null)
        {
            SetEffectStyle(effectData.texName);
            SetEffectPos(roleData.effP);
            SetEffectSca(roleData.effS);
            SetEffectRot(roleData.effR);
        }

        OpenAnim();
    }


    public void ChangeRoleImage(RoleData roleData)
    {
        //场景内替换形象，需要还原runtimeAnimatorController
        var rAnimCon = animCom.runtimeAnimatorController;
        InitRoleByData(roleData);
        animCom.runtimeAnimatorController = rAnimCon;
    }

    public void ChangeAnimCtr(bool isChooseMove)
    {
        var eId = RoleMenuView.Ins.roleData.eId;
        string eyeName = GetAnimName(eId);
        if (isChooseMove)
        {
            animCom.runtimeAnimatorController = moveChooseCtr;
            SetEyesStyle(eyeName);
            SetSpecialEyesStyle(eyeName);
        }
        else
        {
            animCom.runtimeAnimatorController = animCtr;
            animCom.Play(eyeName, 1, 0f);
        }

        if (eId == specialEyeId)
        {
            SetEyePupilColor(eyeName, RoleMenuView.Ins.roleData.eCr);
        }
    }
    private void OpenAnim()
    {
        animCom.enabled = true;
        animCom.runtimeAnimatorController = animCtr;
        //animCom.Play(GetAnimName(customRoleData.eId), 1, 0f); 
    }
    public void StartEyeAnimation(int id)
    {
        string animName = GetAnimName(id);
        if (animCom)
        {
            animCom.Play(animName, 1, 0f);
        }

    }

    public Transform GetBandNode(int type)
    {

        switch (type)
        {
            case (int)BodyNode.RightHand:
                return transform.Find(RIGHT_HAND_PATH);
            case (int)BodyNode.LeftHand:
                return transform.Find(LEFT_HAND_PATH);
            case (int)BodyNode.PickNode:
                if (pickNode == null) {
                    pickNode = transform.Find(PICK_HAND_PATH);
                }
                return pickNode;
            case (int)BodyNode.PickPos:
                if(pickPos == null)
                {
                    pickPos = transform.Find("pickPos");
                }
                return pickPos;
            case (int)BodyNode.FoodNode:
                if(foodNode == null)
                {
                    foodNode = transform.Find(PICK_FOOD_PATH);
                }
                return foodNode;
            case (int)BodyNode.BackNode:
                return transform.Find(BACK_PATH);
            case (int)BodyNode.LEffectNode:
                return transform.Find(LEFT_EFFECT_PATH);
            case (int)BodyNode.BackDeckNode:
                return transform.Find(BAG_PATH);
        }
        return null;
    }

    public string GetAnimName(int id)
    {
        return "eye_" + id;
    }

    public void OnEmoPlay()
    {
        SetPickPropActive(false);
        SetEffectToolActive(false);
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps
        && curPlayerType == PlayerType.SelfPlayer)
        {
            PlayerBaseControl.Inst.playerAnim.gameObject.SetActive(false);
        }
    }

    public void OnEmoEnd()
    {
        SetPickPropActive(true);
        SetEffectToolActive(true);
        if (PlayerBaseControl.Inst && !PlayerBaseControl.Inst.isTps && curPlayerType == PlayerType.SelfPlayer)
        {
            bool isVisible = !PlayerBaseControl.Inst.isFlying && (PlayerControlManager.Inst.isPickedProp
                            || (PlayerDriveControl.Inst && PlayerDriveControl.Inst.steeringWheel));
            PlayerBaseControl.Inst.playerAnim.gameObject.SetActive(isVisible);
        }
    }

    private void SetEffectToolActive(bool isActive)
    {
        Transform childNode = GetBandNode((int)BodyNode.LEffectNode);
        if (childNode != null)
        {
            childNode.gameObject.SetActive(isActive);
        }
    }

    private void SetPickPropActive(bool isActive)
    {
        Transform childNode = GetBandNode((int)BodyNode.PickNode);
        if(childNode != null)
        {
            childNode.gameObject.SetActive(isActive);
        }
    }

    public GameObject InitUgcClothBone(int templateId)
    {
        if (!clothBoneConfigs.ContainsKey(templateId)) return null;
        if (mat_Clothes_01 == null)
        {
            LoggerUtils.LogError("InitUgcClothBone=>mat_Clothes_01=null:" + templateId);
            return null;
        }
        mat_Clothes_01.SetActive(false);
        if (underWear == null)
        {
            LoggerUtils.LogError("InitUgcClothBone=>underWear=null:" + templateId);
            return null;
        }
        underWear.gameObject.SetActive(true);
        var clothBonelist = CreateUgcBoneDic[(int)UGC_CLOTH_TYPE.CLOTH];
        clothBonelist.ForEach(x =>
        {
            var boneParent = transform.Find("UgcBoneParent" + x).gameObject;
            if (boneParent == null)
            {
                LoggerUtils.LogError("InitUgcClothBone=>boneParent=null:" + x);
            }
            boneParent.SetActive(false);
        });
        if (clothBonelist.Contains(templateId))
        {
            var boneParent = transform.Find("UgcBoneParent" + templateId).gameObject;
            if (boneParent == null)
            {
                LoggerUtils.LogError("InitUgcClothBone=>boneParent=null:" + templateId);
            }
            boneParent.SetActive(true);
            return boneParent;
        }

        GameObject ugcBoneParent = new GameObject("UgcBoneParent" + templateId);
        ugcBoneParent.transform.SetParent(transform);
        
        var boneList = clothBoneConfigs[templateId];
        
        foreach(var boneName in boneList)
        {
            GameObject oUgcCloth = roleFbx.transform.Find(boneName).gameObject;
            if (oUgcCloth == null)
            {
                LoggerUtils.LogError("InitUgcClothBone=>boneParent=null:" + boneName);
            }
            GameObject nUgcCloth = Instantiate(oUgcCloth, ugcBoneParent.transform);
            nUgcCloth.layer = playerLayer;
            if (curPlayerType == PlayerType.OtherPlayer)
            {
                nUgcCloth.layer = LayerMask.NameToLayer("OtherPlayer");
            }
            nUgcCloth.name = boneName;
            var oMeshCom = oUgcCloth.GetComponent<SkinnedMeshRenderer>();
            if (oMeshCom == null)
            {
                LoggerUtils.LogError($"InitUgcClothBone=>oMeshCom=null templateID:{templateId}");
                return null;
            }
            var nMeshCom = nUgcCloth.GetComponent<SkinnedMeshRenderer>();
            if (nMeshCom == null)
            {
                LoggerUtils.LogError($"InitUgcClothBone=>nMeshCom=null templateID:{templateId}");
                return null;
            }
            List<Transform> nUgcClothBones = new List<Transform>();
            if (oMeshCom.bones.Length > 0)
            {
                foreach (var oBone in oMeshCom.bones)
                {
                    if (BonesDic.ContainsKey(oBone.name))
                    {
                        nUgcClothBones.Add(BonesDic[oBone.name]);
                    }
                }
                nMeshCom.bones = nUgcClothBones.ToArray();
                nMeshCom.sharedMesh = oMeshCom.sharedMesh;
                if (BonesDic.ContainsKey(oMeshCom.rootBone.name))
                {
                    nMeshCom.rootBone = BonesDic[oMeshCom.rootBone.name];
                }
            }
            var oUgcClothMat = oMeshCom.sharedMaterial;
            nMeshCom.material = oUgcClothMat;
        }
        
        clothBonelist.Add(templateId);
        CreateUgcBoneDic[(int)UGC_CLOTH_TYPE.CLOTH] = clothBonelist;
        return ugcBoneParent;
    }


    private static Dictionary<int, long> styleOrder = new Dictionary<int, long>();

    public static long QueryWearTime(BundlePart part, string name)
    {
        var k = GetKey(part, name);
        if (styleOrder.ContainsKey(k)) return styleOrder[k];
        return 0;
    }
    
    
    //TODO: 现在一个部件一个换装接口不好维护  这边先包到一个接口  后面有空可以改成走一套流程
    public void SetStyle(BundlePart part, string texName, Action succ = null, Action fail = null, string url = null)
    {
        var k = GetKey(part, texName);
        succ += () =>
        {
            if (styleOrder.ContainsKey(k))
            {
                styleOrder[k] = GetTimeStamp();
            }
            else
            {
                styleOrder.Add(k, GetTimeStamp());
            }
        };
        switch (part)
        {
            case BundlePart.Clothes:
                SetClothesStyle(texName, succ, fail, url);
                break;
            case BundlePart.Hats:
                SetHatStyle(texName, succ, fail, url);
                break;
            case BundlePart.Glasses:
                SetGlassesStyle(texName, succ, fail, url);
                break;
            case BundlePart.Bag:
                SetBagStyle(texName, succ, fail, url);
                break;
            case BundlePart.Crossbody:
                SetCroddBodyStyle(texName, succ, fail, url);
                break;
            case BundlePart.Hand:
                SetHandStyle(texName, succ, fail, url);
                break;
            case BundlePart.Special:
                SetSpecialStyle(texName, succ, fail, url);
                break;
            case BundlePart.Shoe:
                SetShoeStyle(texName, succ, fail, url);
                break;
            case BundlePart.Hair:
                SetHairStyle(texName, succ, fail, url);
                break;
            case BundlePart.Accessoies:
                SetAccessoriesStyles(texName, succ, fail, url);
                break;
            case BundlePart.Eyes:
                SetEyesStyle(texName, succ, fail, url);
                break;
            case BundlePart.Brow:
                SetBrowStyle(texName, succ, fail, url);
                break;
            case BundlePart.Nose:
                SetNoseStyle(texName, succ, fail, url);
                break;
            case BundlePart.Mouse:
                SetMouthStyle(texName, succ, fail, url);
                break;
            case BundlePart.Face:
                SetBlusherStyle(texName, succ, fail, url);
                break;
            case BundlePart.Pattern:
                SetPatternStyle(texName, succ, fail, url);
                break;
            case BundlePart.Effect:
                SetEffectStyle(texName, succ, fail, url);
                break;
        }
        

    }

    public static long GetTimeStamp()
    {
        TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return Convert.ToInt64(ts.TotalMilliseconds);
    }


    
    private static int GetKey(BundlePart part, string texName)
    {
        return (part + texName).GetHashCode();
    }
    
    //手部道具创建后回调
    public void SetHandCreateListener(Action<int> action)
    {
        handPropCreated = action;
    }

    //添加手部道具时额外绑定一个父节点
    public GameObject InstantiateHand(GameObject src, Transform parent)
    {
        GameObject tempParent = new GameObject(src.name);
        tempParent.transform.SetParent(parent);

        tempParent.transform.localPosition = Vector3.zero;
        tempParent.transform.localRotation = Quaternion.identity;
        tempParent.transform.localScale = Vector3.one;

        Instantiate(src, tempParent.transform);

        return tempParent;
    }

    //换手信息若读取到0则使用默认值
    private HandLRType GetTargetHandOnLoad(int hdLR, int defaultLR)
    {
        if (hdLR <= (int)HandLRType.Default)
        {
            return (HandLRType)defaultLR;
        }
        return (HandLRType)hdLR;
    }

    private void OnDisable() {
        SetCustomDefaultPos();
    }

    public List<GameObject> GetAvatarObj()
    {
        List<GameObject> avatarObjs = new List<GameObject>();
        foreach (var item in bagStyleDic)
        {
            if(item.Value != null)
            avatarObjs.Add(item.Value);
        }
        foreach (var item in handStyleDic)
        {
            avatarObjs.AddRange(item.Value);
        }
        return avatarObjs;
    }

    public RoleCtrData GetHand(HandStyleData data,GameObject handFbx)
    {
        string path_l = "";
        string path_r = "";
        switch (data.handBipType)
        {
            case (int)HandBipType.Arm:
                path_l = HAND_L_ARMGLOVE_PATH + "/" + data.modelName + "_l";
                path_r = HAND_R_ARMGLOVE_PATH + "/" + data.modelName + "_r";
                break;
            case (int)HandBipType.Glove:
                path_l = HAND_L_GLOVE_PATH + "/" + data.modelName + "_l";
                path_r = HAND_R_GLOVE_PATH + "/" + data.modelName + "_r";
                break;
        }
        var handTF_l = handFbx.transform.Find(path_l);
        var handTF_r = handFbx.transform.Find(path_r);
        RoleCtrData rdata = new RoleCtrData();
        rdata.curSword = handTF_l == null ? handTF_r : handTF_l;
        rdata.typeTF = handTF_l == null ? 1 : 0;
        return rdata;
    }

    public GameObject GetBag(BagStyleData data,GameObject bagFbx)
    {
        if (!string.IsNullOrEmpty(data.modelName))
        {
            var bag = bagFbx.transform.Find(data.modelName).gameObject;
            bag = GameObject.Instantiate(bag, bone_Bag.transform);
            return bag;
        }
        return null;
    }

    public void SetBoneActive(bool isActive)
    {
        if (bone_hand_l != null)
        {
            bone_hand_l.gameObject.SetActive(isActive);
        }
        if(bone_hand_r != null)
        {
            bone_hand_r.gameObject.SetActive(isActive);
        }
        if (bone_handarm_l != null)
        {
            bone_handarm_l.gameObject.SetActive(isActive);
        }
        if (bone_handarm_r != null)
        {
            bone_handarm_r.gameObject.SetActive(isActive);
        }
        if (bone_Bag != null)
        {
            bone_Bag.gameObject.SetActive(isActive);
        }
    }
}
