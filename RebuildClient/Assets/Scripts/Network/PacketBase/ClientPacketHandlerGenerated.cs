using Assets.Scripts.Network;
using Assets.Scripts.Network.PacketBase;
using Assets.Scripts.Network.IncomingPacketHandlers;
using Assets.Scripts.Network.IncomingPacketHandlers.Character;
using Assets.Scripts.Network.IncomingPacketHandlers.Party;
using Assets.Scripts.Network.IncomingPacketHandlers.Combat;
using Assets.Scripts.Network.IncomingPacketHandlers.Environment;
using Assets.Scripts.Network.IncomingPacketHandlers.Network;
using Assets.Scripts.Network.IncomingPacketHandlers.System;
using Assets.Scripts.Network.HandlerBase;

namespace Assets.Scripts.Network.PacketBase
{
	public static partial class ClientPacketHandler
	{
		static ClientPacketHandler()
		{
			handlers = new ClientPacketHandlerBase[114];
			handlers[0] = new PacketOnConnectionApproved(); //ConnectionApproved
			handlers[1] = new InvalidPacket(); //ConnectionDenied
			handlers[2] = new InvalidPacket(); //PlayerReady
			handlers[3] = new PacketOnEnterServer(); //EnterServer
			handlers[4] = new InvalidPacket(); //Ping
			handlers[5] = new PacketPong(); //Pong
			handlers[6] = new PacketCreateEntity(); //CreateEntity
			handlers[7] = new PacketCreateEntity2(); //CreateEntity2
			handlers[8] = new InvalidPacket(); //StartWalk
			handlers[9] = new InvalidPacket(); //PauseMove
			handlers[10] = new InvalidPacket(); //ResumeMove
			handlers[11] = new InvalidPacket(); //Move
			handlers[12] = new PacketAttack(); //Attack
			handlers[13] = new PacketTakeDamage(); //TakeDamage
			handlers[14] = new PacketLookTowards(); //LookTowards
			handlers[15] = new InvalidPacket(); //SitStand
			handlers[16] = new PacketRemoveEntity(); //RemoveEntity
			handlers[17] = new PacketRemoveAllEntities(); //RemoveAllEntities
			handlers[18] = new InvalidPacket(); //Disconnect
			handlers[19] = new PacketOnChangeMaps(); //ChangeMaps
			handlers[20] = new InvalidPacket(); //StopAction
			handlers[21] = new InvalidPacket(); //StopImmediate
			handlers[22] = new InvalidPacket(); //RandomTeleport
			handlers[23] = new InvalidPacket(); //UnhandledPacket
			handlers[24] = new InvalidPacket(); //HitTarget
			handlers[25] = new PacketStartCasting(); //StartCast
			handlers[26] = new InvalidPacket(); //StartAreaCast
			handlers[27] = new PacketUpdateExistingCast(); //UpdateExistingCast
			handlers[28] = new PacketStopCasting(); //StopCast
			handlers[29] = new InvalidPacket(); //CreateCastCircle
			handlers[30] = new PacketOnSkill(); //Skill
			handlers[31] = new PacketSkillIndirect(); //SkillIndirect
			handlers[32] = new PacketSkillFailure(); //SkillError
			handlers[33] = new PacketErrorMessage(); //ErrorMessage
			handlers[34] = new PacketChangeTarget(); //ChangeTarget
			handlers[35] = new InvalidPacket(); //GainExp
			handlers[36] = new InvalidPacket(); //LevelUp
			handlers[37] = new InvalidPacket(); //Death
			handlers[38] = new PacketHpRecovery(); //HpRecovery
			handlers[39] = new PacketImprovedRecoveryTick(); //ImprovedRecoveryTick
			handlers[40] = new PacketChangeSpValue(); //ChangeSpValue
			handlers[41] = new PacketUpdateZeny(); //UpdateZeny
			handlers[42] = new InvalidPacket(); //Respawn
			handlers[43] = new PacketRequestFailed(); //RequestFailed
			handlers[44] = new PacketTargeted(); //Targeted
			handlers[45] = new PacketSay(); //Say
			handlers[46] = new InvalidPacket(); //ChangeName
			handlers[47] = new PacketResurrection(); //Resurrection
			handlers[48] = new InvalidPacket(); //UseInventoryItem
			handlers[49] = new PacketEquipUnequipGear(); //EquipUnequipGear
			handlers[50] = new PacketUpdateCharacterDisplayState(); //UpdateCharacterDisplayState
			handlers[51] = new PacketAddOrRemoveInventoryItem(); //AddOrRemoveInventoryItem
			handlers[52] = new PacketEffectOnCharacter(); //EffectOnCharacter
			handlers[53] = new PacketEffectAtLocation(); //EffectAtLocation
			handlers[54] = new PacketPlayOneShotSound(); //PlayOneShotSound
			handlers[55] = new PacketEmote(); //Emote
			handlers[56] = new InvalidPacket(); //ClientTextCommand
			handlers[57] = new PacketUpdatePlayerData(); //UpdatePlayerData
			handlers[58] = new PacketApplySkillPoint(); //ApplySkillPoint
			handlers[59] = new InvalidPacket(); //ApplyStatPoints
			handlers[60] = new PacketChangeTargetableState(); //ChangeTargetableState
			handlers[61] = new PacketUpdateMapImportantEntityTracking(); //UpdateMapImportantEntityTracking
			handlers[62] = new PacketApplyStatusEffect(); //ApplyStatusEffect
			handlers[63] = new PacketRemoveStatusEffect(); //RemoveStatusEffect
			handlers[64] = new PacketSocketEquipment(); //SocketEquipment
			handlers[65] = new InvalidPacket(); //AdminRequestMove
			handlers[66] = new InvalidPacket(); //AdminServerAction
			handlers[67] = new InvalidPacket(); //AdminLevelUp
			handlers[68] = new InvalidPacket(); //AdminEnterServerSpecificMap
			handlers[69] = new InvalidPacket(); //AdminChangeAppearance
			handlers[70] = new InvalidPacket(); //AdminSummonMonster
			handlers[71] = new PacketAdminHideCharacter(); //AdminHideCharacter
			handlers[72] = new InvalidPacket(); //AdminChangeSpeed
			handlers[73] = new InvalidPacket(); //AdminFindTarget
			handlers[74] = new InvalidPacket(); //AdminResetSkills
			handlers[75] = new InvalidPacket(); //AdminResetStats
			handlers[76] = new InvalidPacket(); //AdminCreateItem
			handlers[77] = new InvalidPacket(); //NpcClick
			handlers[78] = new PacketNpcInteraction(); //NpcInteraction
			handlers[79] = new InvalidPacket(); //NpcAdvance
			handlers[80] = new InvalidPacket(); //NpcSelectOption
			handlers[81] = new InvalidPacket(); //NpcRefineSubmit
			handlers[82] = new PacketDropItem(); //DropItem
			handlers[83] = new PacketPickUpItem(); //PickUpItem
			handlers[84] = new PacketOpenShop(); //OpenShop
			handlers[85] = new PacketOpenStorage(); //OpenStorage
			handlers[86] = new PacketStartNpcTrade(); //StartNpcTrade
			handlers[87] = new PacketStorageInteraction(); //StorageInteraction
			handlers[88] = new InvalidPacket(); //ShopBuySell
			handlers[89] = new InvalidPacket(); //NpcTradeItem
			handlers[90] = new PacketCartInventoryInteraction(); //CartInventoryInteraction
			handlers[91] = new PacketChangeFollower(); //ChangeFollower
			handlers[92] = new PacketServerEvent(); //ServerEvent
			handlers[93] = new PacketServerResult(); //ServerResult
			handlers[94] = new InvalidPacket(); //DebugEntry
			handlers[95] = new PacketMemoMapLocation(); //MemoMapLocation
			handlers[96] = new InvalidPacket(); //DeleteCharacter
			handlers[97] = new InvalidPacket(); //AdminCharacterAction
			handlers[98] = new PacketChangePlayerSpecialActionState(); //ChangePlayerSpecialActionState
			handlers[99] = new PacketRefreshGrantedSkills(); //RefreshGrantedSkills
			handlers[100] = new InvalidPacket(); //CreateParty
			handlers[101] = new PacketInvitePartyMember(); //InvitePartyMember
			handlers[102] = new PacketAcceptPartyInvite(); //AcceptPartyInvite
			handlers[103] = new PacketUpdateParty(); //UpdateParty
			handlers[104] = new PacketNotifyPlayerPartyChange(); //NotifyPlayerPartyChange
			handlers[105] = new PacketSkillWithMaskedArea(); //SkillWithMaskedArea
			handlers[106] = new PacketStartVending(); //VendingStart
			handlers[107] = new PacketVendingStop(); //VendingStop
			handlers[108] = new PacketVendingStoreView(); //VendingViewStore
			handlers[109] = new PacketVendingNotifyOfSale(); //VendingNotifyOfSale
			handlers[110] = new InvalidPacket(); //VendingPurchaseFromStore
			handlers[111] = new InvalidPacket(); //StartWalkInDirection
			handlers[112] = new PacketResetMotion(); //ResetMotion
			handlers[113] = new PacketToggleActivatedState(); //ToggleActivatedState
		}
	}
}
