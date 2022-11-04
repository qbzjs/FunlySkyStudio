using System.Collections.Generic;

/// <summary>
/// 注册UIPanel的方法，由工具自动生成
/// ！！！！！！！！！请勿手动编辑！！！！！！！！！！！！
/// 
/// DO NOT EDIT IT!!!!
/// </summary>
public class UIPanelRegFunc : CInstance<UIPanelRegFunc>
{
    public void InitUIPanelFuncs(Dictionary<string, UIControlManager.UIPanelFuncs> panelFuncs)
    {
        panelFuncs.Add("AttackingPanel", new UIControlManager.UIPanelFuncs(AttackingPanel.Show, AttackingPanel.Hide));
        panelFuncs.Add("AttackWeaponCtrlPanel", new UIControlManager.UIPanelFuncs(AttackWeaponCtrlPanel.Show, AttackWeaponCtrlPanel.Hide));
        panelFuncs.Add("AttackWeaponPanel", new UIControlManager.UIPanelFuncs(AttackWeaponPanel.Show, AttackWeaponPanel.Hide));
        panelFuncs.Add("AudioLoadingPanel", new UIControlManager.UIPanelFuncs(AudioLoadingPanel.Show, AudioLoadingPanel.Hide));
        panelFuncs.Add("BaseMaterialPanel", new UIControlManager.UIPanelFuncs(BaseMaterialPanel.Show, BaseMaterialPanel.Hide));
        panelFuncs.Add("BasePrimitivePanel", new UIControlManager.UIPanelFuncs(BasePrimitivePanel.Show, BasePrimitivePanel.Hide));
        panelFuncs.Add("BGEnrMusicPanel", new UIControlManager.UIPanelFuncs(BGEnrMusicPanel.Show, BGEnrMusicPanel.Hide));
        panelFuncs.Add("BGMusicPanel", new UIControlManager.UIPanelFuncs(BGMusicPanel.Show, BGMusicPanel.Hide));
        panelFuncs.Add("BlackPanel", new UIControlManager.UIPanelFuncs(BlackPanel.Show, BlackPanel.Hide));
        panelFuncs.Add("BloodPropPanel", new UIControlManager.UIPanelFuncs(BloodPropPanel.Show, BloodPropPanel.Hide));
        panelFuncs.Add("BulletAnchorsPanel", new UIControlManager.UIPanelFuncs(BulletAnchorsPanel.Show, BulletAnchorsPanel.Hide));
        panelFuncs.Add("CameraModePanel", new UIControlManager.UIPanelFuncs(CameraModePanel.Show, CameraModePanel.Hide));
        panelFuncs.Add("CatchPanel", new UIControlManager.UIPanelFuncs(CatchPanel.Show, CatchPanel.Hide));
        panelFuncs.Add("CharacterPopupPanel", new UIControlManager.UIPanelFuncs(CharacterPopupPanel.Show, CharacterPopupPanel.Hide));
        panelFuncs.Add("CharacterTipDialogPanel", new UIControlManager.UIPanelFuncs(CharacterTipDialogPanel.Show, CharacterTipDialogPanel.Hide));
        panelFuncs.Add("CharacterTipPanel", new UIControlManager.UIPanelFuncs(CharacterTipPanel.Show, CharacterTipPanel.Hide));
        panelFuncs.Add("ChoseProfilePanel", new UIControlManager.UIPanelFuncs(ChoseProfilePanel.Show, ChoseProfilePanel.Hide));
        panelFuncs.Add("ComfirmPanel", new UIControlManager.UIPanelFuncs(ComfirmPanel.Show, ComfirmPanel.Hide));
        panelFuncs.Add("CoverPanel", new UIControlManager.UIPanelFuncs(CoverPanel.Show, CoverPanel.Hide));
        panelFuncs.Add("DirLightPanel", new UIControlManager.UIPanelFuncs(DirLightPanel.Show, DirLightPanel.Hide));
        panelFuncs.Add("DisplayBoardPanel", new UIControlManager.UIPanelFuncs(DisplayBoardPanel.Show, DisplayBoardPanel.Hide));
        panelFuncs.Add("DTextPanel", new UIControlManager.UIPanelFuncs(DTextPanel.Show, DTextPanel.Hide));
        panelFuncs.Add("EmoMenuPanel", new UIControlManager.UIPanelFuncs(EmoMenuPanel.Show, EmoMenuPanel.Hide));
        panelFuncs.Add("FlyPermisionPanel", new UIControlManager.UIPanelFuncs(FlyPermisionPanel.Show, FlyPermisionPanel.Hide));
        panelFuncs.Add("ForceExitPanel", new UIControlManager.UIPanelFuncs(ForceExitPanel.Show, ForceExitPanel.Hide));
        panelFuncs.Add("FPSPlayerHpPanel", new UIControlManager.UIPanelFuncs(FPSPlayerHpPanel.Show, FPSPlayerHpPanel.Hide));
        panelFuncs.Add("GameEditModePanel", new UIControlManager.UIPanelFuncs(GameEditModePanel.Show, GameEditModePanel.Hide));
        panelFuncs.Add("GlobalEyePanel", new UIControlManager.UIPanelFuncs(GlobalEyePanel.Show, GlobalEyePanel.Hide));
        panelFuncs.Add("GlobalSettingPanel", new UIControlManager.UIPanelFuncs(GlobalSettingPanel.Show, GlobalSettingPanel.Hide));
        panelFuncs.Add("LeaderBoardPanel", new UIControlManager.UIPanelFuncs(LeaderBoardPanel.Show, LeaderBoardPanel.Hide));
        panelFuncs.Add("ModelHandlePanel", new UIControlManager.UIPanelFuncs(ModelHandlePanel.Show, ModelHandlePanel.Hide));
        panelFuncs.Add("ModelPropertyPanel", new UIControlManager.UIPanelFuncs(ModelPropertyPanel.Show, ModelPropertyPanel.Hide));
        panelFuncs.Add("MusicBoardPanel", new UIControlManager.UIPanelFuncs(MusicBoardPanel.Show, MusicBoardPanel.Hide));
        panelFuncs.Add("NetTipsPanel", new UIControlManager.UIPanelFuncs(NetTipsPanel.Show, NetTipsPanel.Hide));
        panelFuncs.Add("NewDTextPanel", new UIControlManager.UIPanelFuncs(NewDTextPanel.Show, NewDTextPanel.Hide));
        panelFuncs.Add("NoNetworkPanel", new UIControlManager.UIPanelFuncs(NoNetworkPanel.Show, NoNetworkPanel.Hide));
        panelFuncs.Add("PackPanel", new UIControlManager.UIPanelFuncs(PackPanel.Show, PackPanel.Hide));
        panelFuncs.Add("PermissionPanel", new UIControlManager.UIPanelFuncs(PermissionPanel.Show, PermissionPanel.Hide));
        panelFuncs.Add("PickablityAnchorsPanel", new UIControlManager.UIPanelFuncs(PickablityAnchorsPanel.Show, PickablityAnchorsPanel.Hide));
        panelFuncs.Add("PickablityPanel", new UIControlManager.UIPanelFuncs(PickablityPanel.Show, PickablityPanel.Hide));
        panelFuncs.Add("PlayModePanel", new UIControlManager.UIPanelFuncs(PlayModePanel.Show, PlayModePanel.Hide));
        panelFuncs.Add("PointLightPanel", new UIControlManager.UIPanelFuncs(PointLightPanel.Show, PointLightPanel.Hide));
        panelFuncs.Add("PortalGateAnimPanel", new UIControlManager.UIPanelFuncs(PortalGateAnimPanel.Show, PortalGateAnimPanel.Hide));
        panelFuncs.Add("PortalGatePanel", new UIControlManager.UIPanelFuncs(PortalGatePanel.Show, PortalGatePanel.Hide));
        panelFuncs.Add("PortalPlayPanel", new UIControlManager.UIPanelFuncs(PortalPlayPanel.Show, PortalPlayPanel.Hide));
        panelFuncs.Add("PostProcessingPanel", new UIControlManager.UIPanelFuncs(PostProcessingPanel.Show, PostProcessingPanel.Hide));
        panelFuncs.Add("PropEditModePanel", new UIControlManager.UIPanelFuncs(PropEditModePanel.Show, PropEditModePanel.Hide));
        panelFuncs.Add("PropertiesPanel", new UIControlManager.UIPanelFuncs(PropertiesPanel.Show, PropertiesPanel.Hide));
        panelFuncs.Add("PropLittleTipsPanel", new UIControlManager.UIPanelFuncs(PropLittleTipsPanel.Show, PropLittleTipsPanel.Hide));
        panelFuncs.Add("PropTipsPanel", new UIControlManager.UIPanelFuncs(PropTipsPanel.Show, PropTipsPanel.Hide));
        panelFuncs.Add("PVPSurvivalGamePlayPanel", new UIControlManager.UIPanelFuncs(PVPSurvivalGamePlayPanel.Show, PVPSurvivalGamePlayPanel.Hide));
        panelFuncs.Add("PVPWaitAreaPanel", new UIControlManager.UIPanelFuncs(PVPWaitAreaPanel.Show, PVPWaitAreaPanel.Hide));
        panelFuncs.Add("PVPWinConditionGamePlayPanel", new UIControlManager.UIPanelFuncs(PVPWinConditionGamePlayPanel.Show, PVPWinConditionGamePlayPanel.Hide));
        panelFuncs.Add("ReferPanel", new UIControlManager.UIPanelFuncs(ReferPanel.Show, ReferPanel.Hide));
        panelFuncs.Add("ResCoverPanel", new UIControlManager.UIPanelFuncs(ResCoverPanel.Show, ResCoverPanel.Hide));
        panelFuncs.Add("ResStorePanel", new UIControlManager.UIPanelFuncs(ResStorePanel.Show, ResStorePanel.Hide));
        panelFuncs.Add("RoomChatPanel", new UIControlManager.UIPanelFuncs(RoomChatPanel.Show, RoomChatPanel.Hide));
        panelFuncs.Add("RoomMenuPanel", new UIControlManager.UIPanelFuncs(RoomMenuPanel.Show, RoomMenuPanel.Hide));
        panelFuncs.Add("SensorBoxPanel", new UIControlManager.UIPanelFuncs(SensorBoxPanel.Show, SensorBoxPanel.Hide));
        panelFuncs.Add("ShootWeaponCtrlPanel", new UIControlManager.UIPanelFuncs(ShootWeaponCtrlPanel.Show, ShootWeaponCtrlPanel.Hide));
        panelFuncs.Add("ShootWeaponPanel", new UIControlManager.UIPanelFuncs(ShootWeaponPanel.Show, ShootWeaponPanel.Hide));
        panelFuncs.Add("ShotPhotoPanel", new UIControlManager.UIPanelFuncs(ShotPhotoPanel.Show, ShotPhotoPanel.Hide));
        panelFuncs.Add("ShowHidePropPanel", new UIControlManager.UIPanelFuncs(ShowHidePropPanel.Show, ShowHidePropPanel.Hide));
        panelFuncs.Add("SkyboxStylePanel", new UIControlManager.UIPanelFuncs(SkyboxStylePanel.Show, SkyboxStylePanel.Hide));
        panelFuncs.Add("SocialNotificationPanel", new UIControlManager.UIPanelFuncs(SocialNotificationPanel.Show, SocialNotificationPanel.Hide));
        panelFuncs.Add("SoundPanel", new UIControlManager.UIPanelFuncs(SoundPanel.Show, SoundPanel.Hide));
        panelFuncs.Add("SpawnPointPanel", new UIControlManager.UIPanelFuncs(SpawnPointPanel.Show, SpawnPointPanel.Hide));
        panelFuncs.Add("SpotLightPanel", new UIControlManager.UIPanelFuncs(SpotLightPanel.Show, SpotLightPanel.Hide));
        panelFuncs.Add("StorePanel", new UIControlManager.UIPanelFuncs(StorePanel.Show, StorePanel.Hide));
        panelFuncs.Add("TerrainMaterialPanel", new UIControlManager.UIPanelFuncs(TerrainMaterialPanel.Show, TerrainMaterialPanel.Hide));
        panelFuncs.Add("TestPanel", new UIControlManager.UIPanelFuncs(TestPanel.Show, TestPanel.Hide));
        panelFuncs.Add("TipPanel", new UIControlManager.UIPanelFuncs(TipPanel.Show, TipPanel.Hide));
        panelFuncs.Add("TrapBoxPanel", new UIControlManager.UIPanelFuncs(TrapBoxPanel.Show, TrapBoxPanel.Hide));
        panelFuncs.Add("UgcClothItemPanel", new UIControlManager.UIPanelFuncs(UgcClothItemPanel.Show, UgcClothItemPanel.Hide));
        panelFuncs.Add("UserProfilePanel", new UIControlManager.UIPanelFuncs(UserProfilePanel.Show, UserProfilePanel.Hide));
        panelFuncs.Add("VideoFullPanel", new UIControlManager.UIPanelFuncs(VideoFullPanel.Show, VideoFullPanel.Hide));
        panelFuncs.Add("VideoNodePanel", new UIControlManager.UIPanelFuncs(VideoNodePanel.Show, VideoNodePanel.Hide));
        panelFuncs.Add("ChooseClothPanel", new UIControlManager.UIPanelFuncs(ChooseClothPanel.Show, ChooseClothPanel.Hide));
        panelFuncs.Add("BaggagePanel", new UIControlManager.UIPanelFuncs(BaggagePanel.Show, BaggagePanel.Hide));
        panelFuncs.Add("StateEmoPanel", new UIControlManager.UIPanelFuncs(StateEmoPanel.Show, StateEmoPanel.Hide));
        panelFuncs.Add("DCResPanel", new UIControlManager.UIPanelFuncs(DCResPanel.Show, DCResPanel.Hide));
        panelFuncs.Add("ParachuteCtrlPanel", new UIControlManager.UIPanelFuncs(ParachuteCtrlPanel.Show, ParachuteCtrlPanel.Hide));
        panelFuncs.Add("TokenDetectionPanel", new UIControlManager.UIPanelFuncs(TokenDetectionPanel.Show, TokenDetectionPanel.Hide));
        panelFuncs.Add("SpecialEmotePropsPanel", new UIControlManager.UIPanelFuncs(SpecialEmotePropsPanel.Show, SpecialEmotePropsPanel.Hide));
        panelFuncs.Add("SwordPanel", new UIControlManager.UIPanelFuncs(SwordPanel.Show, SwordPanel.Hide));
        panelFuncs.Add("CreateWalletPanel", new UIControlManager.UIPanelFuncs(CreateWalletPanel.Show, CreateWalletPanel.Hide));
        panelFuncs.Add("CrystalStoneRewardPanel", new UIControlManager.UIPanelFuncs(CrystalStoneRewardPanel.Show, CrystalStoneRewardPanel.Hide));
        panelFuncs.Add("CrystalStoneTipsPanel", new UIControlManager.UIPanelFuncs(CrystalStoneTipsPanel.Show, CrystalStoneTipsPanel.Hide));
    }
}