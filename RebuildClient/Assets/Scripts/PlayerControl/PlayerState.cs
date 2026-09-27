using System;
using System.Collections.Generic;
using Assets.Scripts.Data;
using Assets.Scripts.Network;
using RebuildSharedData.ClientTypes;
using RebuildSharedData.Data;
using RebuildSharedData.Enum;
using RebuildSharedData.Enum.EntityStats;

namespace Assets.Scripts.PlayerControl
{
    public class PlayerState
    {
        public static PlayerState Instance = new();

        public event Action AppearanceChanged;
        //entity id of an online player whose party membership may have changed
        public event Action<int> PartyMembershipChanged;

        public bool IsValid { get; set; } = false;
        public int EntityId { get; set; }
        public string PlayerName;
        public int Level;
        public int Exp;
        public int Hp;
        public int MaxHp;
        public int Sp;
        public int MaxSp;
        public int AmmoId;
        public int[] CharacterData = new int[(int)PlayerStat.PlayerStatsMax];
        public int[] CharacterStats = new int[(int)CharacterStat.CharacterStatsMax];
        public float AttackSpeed;
        public ClientSkillTree SkillTree;
        public int SkillPoints;
        public int JobId;
        public bool IsMale;
        public int WeaponClass;
        public int HairStyleId;
        public int HairColorId;
        public int Headgear1;
        public int Headgear2;
        public int Headgear3;

        //Only fires when something actually changed - avoids redundant async sprite reloads.
        public void SetAppearance(int jobId, int hairStyleId, int hairColorId, bool isMale,
            int headgear1, int headgear2, int headgear3)
        {
            if (jobId == JobId && hairStyleId == HairStyleId && hairColorId == HairColorId && isMale == IsMale
                && headgear1 == Headgear1 && headgear2 == Headgear2 && headgear3 == Headgear3)
                return;

            JobId = jobId;
            HairStyleId = hairStyleId;
            HairColorId = hairColorId;
            IsMale = isMale;
            Headgear1 = headgear1;
            Headgear2 = headgear2;
            Headgear3 = headgear3;

            AppearanceChanged?.Invoke();
        }
        public int CartWeight = 0;
        public int CurrentWeight = 0;
        public int MaxWeight = 3000;
        public int Zeny = 0;
        public Dictionary<CharacterSkill, int> KnownSkills = new();
        public Dictionary<CharacterSkill, int> GrantedSkills = new();
        public bool IsAdminHidden = false;
        public ClientInventory Inventory = new();
        public ClientInventory Cart = new();
        public ClientInventory Storage = new();
        public int[] EquippedItems = new int[10];
        public HashSet<int> EquippedBagIdHashes = new();
        public MapMemoLocation[] MemoLocations = new MapMemoLocation[4];
        public bool IsInParty;
        public bool HasCart;
        public bool HasBird;
        public int PartyId;
        public int PartyLeader = -1;
        public int PartyMemberId;
        public int InvitedPartyId = -1;
        public string PartyName;
        public string MapName;
        public Dictionary<int, PartyMemberInfo> PartyMembers = new();
        public Dictionary<int, int> PartyMemberEntityLookup = new(); //member id to entity id
        public Dictionary<int, int> PartyMemberIdLookup = new(); //entity id to member id

        public int GetData(PlayerStat stat) => CharacterData[(int)stat];
        public int GetStat(CharacterStat stat) => CharacterStats[(int)stat];

        public void SortPartyMembers()
        {
            
        }
        
        public void UpdatePlayerName()
        {
            if(IsInParty && PartyLeader == PartyMemberId)
                CameraFollower.Instance.CharacterDetailBox.CharacterName.text = $"★{PlayerName}";
            else
                CameraFollower.Instance.CharacterDetailBox.CharacterName.text = PlayerName;
        }

        //The lookup holds online members on any map.
        public bool TryGetPartyMember(int entityId, out PartyMemberInfo member)
        {
            member = null;
            return IsInParty
                   && PartyMemberIdLookup.TryGetValue(entityId, out var memberId)
                   && PartyMembers.TryGetValue(memberId, out member);
        }

        public void AssignPartyMemberControllable(int entityId, ServerControllable controllable)
        {
            if(PartyMemberIdLookup.TryGetValue(entityId, out var partyMemberId) && PartyMembers.TryGetValue(partyMemberId, out var member))
                member.Controllable = controllable;
        }

        public void RegisterOrUpdatePartyMember(PartyMemberInfo info)
        {
            var memberId = info.PartyMemberId;
            var entityId = info.EntityId;
            PartyMembers[memberId] = info;
            if(info.PlayerName == PlayerName || entityId == EntityId)
                PartyMemberId = memberId;
            
            if (entityId > 0)
            {
                PartyMemberEntityLookup.TryAdd(memberId, entityId);
                PartyMemberIdLookup.TryAdd(entityId, memberId);
                PartyMembershipChanged?.Invoke(entityId);
            }
            else
            {
                //logged out: the update carries -1, so remove by the previous entity id
                if (PartyMemberEntityLookup.TryGetValue(memberId, out var oldEntityId))
                    PartyMemberIdLookup.Remove(oldEntityId);

                PartyMemberEntityLookup.Remove(memberId);
            }

            if (info.IsLeader)
                PartyLeader = memberId;
        }

        public PartyMemberInfo RemovePartyMember(int memberId)
        {
            PartyMembers.Remove(memberId, out var info);
            if (PartyLeader == memberId)
                PartyLeader = -1;
            
            if (PartyMemberEntityLookup.TryGetValue(memberId, out var entityId))
            {
                PartyMemberIdLookup.Remove(entityId);
                PartyMemberEntityLookup.Remove(memberId);
                PartyMembershipChanged?.Invoke(entityId);
            }

            return info;
        }

        public void ClearParty()
        {
            IsInParty = false;
            PartyMembers.Clear();
            PartyMemberEntityLookup.Clear();

            var formerMembers = new List<int>(PartyMemberIdLookup.Keys);
            PartyMemberIdLookup.Clear();

            foreach (var entityId in formerMembers)
                PartyMembershipChanged?.Invoke(entityId);
        }
    }
}
