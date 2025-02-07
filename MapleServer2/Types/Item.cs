﻿using Maple2Storage.Enums;
using Maple2Storage.Types;
using Maple2Storage.Types.Metadata;
using MapleServer2.Data.Static;
using MapleServer2.Database;
using MapleServer2.Enums;
using MapleServer2.Packets;
using MapleServer2.Servers.Game;
using MapleServer2.Tools;
using MoonSharp.Interpreter;

namespace MapleServer2.Types;

public class Item
{
    public int Level { get; set; }
    public InventoryTab InventoryTab { get; private set; }
    public ItemSlot ItemSlot { get; set; }
    public GemSlot GemSlot { get; set; }
    public int Rarity { get; init; }
    public int StackLimit { get; private set; }
    public bool EnableBreak { get; private set; }
    public bool IsTwoHand { get; private set; }
    public bool IsDress { get; private set; }
    public bool IsCustomScore { get; set; }
    public int PlayCount { get; set; }
    public Gender Gender { get; private set; }
    public string FileName { get; set; }
    public int SkillId { get; set; }
    public List<Job> RecommendJobs { get; set; }
    public ItemFunctionMetadata Function { get; set; }
    public string Tag { get; set; }
    public int ShopID { get; set; }
    public ItemHousingCategory HousingCategory;
    public string BlackMarketCategory;
    public string Category;
    public ItemType Type { get; set; }

    public int Id;
    public long Uid;
    public string Name;
    public short Slot;
    public int Amount;
    public bool IsEquipped;

    public long CreationTime;
    public long ExpiryTime;

    public int TimesAttributesChanged;
    public bool IsLocked;
    public long UnlockTime;
    public short RemainingGlamorForges;
    public int GachaDismantleId;
    public int GearScore;
    public int EnchantLevel;
    public int LimitBreakLevel;
    public bool DisableEnchant;

    // EnchantExp (10000 = 100%) for Peachy
    public int EnchantExp;
    public int RemainingRepackageCount;
    public int Charges;
    public ItemTransferFlag TransferFlag;
    public TransferType TransferType;
    public int RemainingTrades;

    // For friendship badges
    public long PairedCharacterId;
    public string PairedCharacterName;
    public int PetSkinBadgeId;
    public byte[] TransparencyBadgeBools;

    public long OwnerAccountId;
    public long OwnerCharacterId;
    public string OwnerCharacterName;
    public EquipColor Color;
    public HairData HairData;
    public HatData HatData;
    public byte[] FaceDecorationData;
    public MusicScore Score;
    public ItemStats Stats;

    public UGC Ugc;

    public long InventoryId;
    public long BankInventoryId;
    public long HomeId;
    public long MailId;

    // For items that are in the field
    public DropInformation DropInformation = new();

    public ItemAdditionalEffectMetadata AdditionalEffects;

    public PetInfo PetInfo;

    public Item() { }

    public Item(int id, int amount = 1, int rarity = -1, bool saveToDatabase = true)
    {
        Id = id;
        Amount = amount;
        Rarity = rarity == -1 ? ItemMetadataStorage.GetRarity(id) : rarity;

        ItemPropertyMetadata property = ItemMetadataStorage.GetPropertyMetadata(Id);
        ItemMusicMetadata music = ItemMetadataStorage.GetMusicMetadata(Id);
        ItemLimitMetadata limit = ItemMetadataStorage.GetLimitMetadata(Id);

        SetMetadataValues();
        Name = ItemMetadataStorage.GetName(id);
        Level = limit.LevelLimitMin;
        ItemSlot = ItemMetadataStorage.GetSlot(id);
        if (ItemMetadataStorage.GetIsUGC(id))
        {
            Ugc = new();
            Ugc.Uid = DatabaseManager.UGC.Insert(Ugc);
        }

        if (GemSlot == GemSlot.TRANS)
        {
            TransparencyBadgeBools = new byte[10];
        }

        PlayCount = music.PlayCount;
        Color = ItemMetadataStorage.GetEquipColor(id);
        CreationTime = TimeInfo.Now();
        RemainingTrades = property.TradeableCount;
        RemainingRepackageCount = property.RepackageCount;
        RemainingGlamorForges = ItemExtractionMetadataStorage.GetExtractionCount(id);
        Slot = -1;
        Score = new();
        Stats = new(this);
        GearScore = GetGearScore();
        ExpiryTime = ItemMetadataStorage.GetExpiration(id);
        if (InventoryTab is InventoryTab.Pets)
        {
            PetInfo = new();
        }

        if (!saveToDatabase)
        {
            return;
        }

        Uid = DatabaseManager.Items.Insert(this);
    }

    // Make a copy of item
    public Item(Item other)
    {
        Id = other.Id;
        Name = other.Name;
        Level = other.Level;
        Gender = other.Gender;
        InventoryTab = other.InventoryTab;
        ItemSlot = other.ItemSlot;
        GemSlot = other.GemSlot;
        Rarity = other.Rarity;
        StackLimit = other.StackLimit;
        EnableBreak = other.EnableBreak;
        IsTwoHand = other.IsTwoHand;
        IsDress = other.IsDress;
        IsCustomScore = other.IsCustomScore;
        PlayCount = other.PlayCount;
        FileName = other.FileName;
        Function = other.Function;
        Uid = other.Uid;
        Slot = other.Slot;
        Amount = other.Amount;
        CreationTime = other.CreationTime;
        ExpiryTime = other.ExpiryTime;
        TimesAttributesChanged = other.TimesAttributesChanged;
        IsLocked = other.IsLocked;
        UnlockTime = other.UnlockTime;
        RemainingGlamorForges = other.RemainingGlamorForges;
        GachaDismantleId = other.GachaDismantleId;
        EnchantLevel = other.EnchantLevel;
        LimitBreakLevel = other.LimitBreakLevel;
        EnchantExp = other.EnchantExp;
        RemainingRepackageCount = other.RemainingRepackageCount;
        Charges = other.Charges;
        TransferFlag = other.TransferFlag;
        RemainingTrades = other.RemainingTrades;
        PairedCharacterId = other.PairedCharacterId;
        PairedCharacterName = other.PairedCharacterName;
        PetSkinBadgeId = other.PetSkinBadgeId;
        RecommendJobs = other.RecommendJobs;
        OwnerCharacterId = other.OwnerCharacterId;
        OwnerCharacterName = other.OwnerCharacterName;
        InventoryId = other.InventoryId;
        BankInventoryId = other.BankInventoryId;
        BlackMarketCategory = other.BlackMarketCategory;
        Category = other.Category;
        HomeId = other.HomeId;
        Color = other.Color;
        HairData = other.HairData;
        HatData = other.HatData;
        Score = new();
        Stats = new(other.Stats);
        Ugc = other.Ugc;
        DropInformation = other.DropInformation;
        if (other.PetInfo is not null)
        {
            PetInfo = new(other.PetInfo);
        }

        SetMetadataValues();
    }

    public bool TrySplit(int splitAmount, out Item splitItem)
    {
        splitItem = null;
        if (Amount < splitAmount)
        {
            return false;
        }

        Amount -= splitAmount;

        splitItem = new(this)
        {
            Amount = splitAmount,
            Slot = -1,
            InventoryId = 0
        };
        splitItem.Uid = DatabaseManager.Items.Insert(splitItem);

        return true;
    }

    public static bool IsWeapon(ItemSlot slot)
    {
        return slot is ItemSlot.RH or ItemSlot.LH or ItemSlot.OH;
    }

    public static bool IsAccessory(ItemSlot slot)
    {
        return slot is ItemSlot.FH or ItemSlot.EA or ItemSlot.PD or ItemSlot.RI or ItemSlot.BE;
    }

    public static bool IsArmor(ItemSlot slot)
    {
        return slot is ItemSlot.CP or ItemSlot.CL or ItemSlot.PA or ItemSlot.GL or ItemSlot.SH or ItemSlot.MT;
    }

    public bool IsPet()
    {
        return ItemMetadataStorage.GetPetId(Id) != 0;
    }

    public bool BindItem(Player player)
    {
        if (OwnerCharacterId != 0 && OwnerCharacterId != player.CharacterId)
        {
            return false;
        }

        if (OwnerCharacterId == player.CharacterId)
        {
            return true;
        }

        OwnerAccountId = player.AccountId;
        OwnerCharacterId = player.CharacterId;
        OwnerCharacterName = player.Name;
        RemainingTrades = 0;

        player.Session?.Send(ItemInventoryPacket.UpdateItem(this));
        return true;
    }

    public bool IsBound()
    {
        return OwnerCharacterId != 0;
    }

    public bool IsSelfBound(long characterId)
    {
        return OwnerCharacterId == characterId;
    }

    public bool IsExpired()
    {
        return TimeInfo.Now() > ExpiryTime && ExpiryTime != 0;
    }

    public void DecreaseTradeCount()
    {
        if (!TransferFlag.HasFlag(ItemTransferFlag.LimitedTradeCount))
        {
            return;
        }

        RemainingTrades--;
    }

    public void SetMetadataValues()
    {
        ItemPropertyMetadata property = ItemMetadataStorage.GetPropertyMetadata(Id);
        ItemLimitMetadata limit = ItemMetadataStorage.GetLimitMetadata(Id);
        ItemMusicMetadata music = ItemMetadataStorage.GetMusicMetadata(Id);
        ItemSkillMetadata skill = ItemMetadataStorage.GetSkillMetadata(Id);
        ItemHousingMetadata housing = ItemMetadataStorage.GetHousingMetadata(Id);
        InventoryTab = ItemMetadataStorage.GetTab(Id);
        GemSlot = ItemMetadataStorage.GetGem(Id);
        StackLimit = property.StackLimit;
        EnableBreak = limit.Breakable;
        IsTwoHand = ItemMetadataStorage.GetIsTwoHand(Id);
        IsDress = ItemMetadataStorage.GetIsDress(Id);
        IsCustomScore = music.IsCustomScore;
        FileName = music.FileName;
        Gender = limit.Gender;
        SkillId = skill.SkillId;
        RecommendJobs = ItemMetadataStorage.GetRecommendJobs(Id);
        Function = ItemMetadataStorage.GetFunctionMetadata(Id);
        Tag = ItemMetadataStorage.GetTag(Id);
        ShopID = ItemMetadataStorage.GetShopID(Id);
        TransferType = limit.TransferType;
        TransferFlag = ItemMetadataStorage.GetTransferFlag(Id, Rarity);
        HousingCategory = housing.HousingCategory;
        BlackMarketCategory = property.BlackMarketCategory;
        Category = property.Category;
        DisableEnchant = limit.DisableEnchant;
        AdditionalEffects = ItemMetadataStorage.GetAdditionalEffects(Id);
        Type = GetItemType();
    }

    public ItemType GetItemType()
    {
        //TODO: Find a better method to find the item type
        return (Id / 100000) switch
        {
            112 => ItemType.Earring,
            113 => ItemType.Hat,
            114 => ItemType.Clothes,
            115 => ItemType.Pants,
            116 => ItemType.Gloves,
            117 => ItemType.Shoes,
            118 => ItemType.Cape,
            119 => ItemType.Necklace,
            120 => ItemType.Ring,
            121 => ItemType.Belt,
            122 => ItemType.Overall,
            130 => ItemType.Bludgeon,
            131 => ItemType.Dagger,
            132 => ItemType.Longsword,
            133 => ItemType.Scepter,
            134 => ItemType.ThrowingStar,
            140 => ItemType.Spellbook,
            141 => ItemType.Shield,
            150 => ItemType.Greatsword,
            151 => ItemType.Bow,
            152 => ItemType.Staff,
            153 => ItemType.Cannon,
            154 => ItemType.Blade,
            155 => ItemType.Knuckle,
            156 => ItemType.Orb,
            209 => ItemType.Medal,
            410 or 420 or 430 => ItemType.Lapenshard,
            501 or 502 or 503 or 504 or 505 => ItemType.Furnishing,
            600 => ItemType.Pet,
            900 => ItemType.Currency,
            _ => ItemType.None
        };
    }

    public bool CanEquip(GameSession session)
    {
        NoticeType noticeType = NoticeType.Chat | NoticeType.FastText;
        if (ItemMetadataStorage.GetLimitMetadata(Id).VipOnly && !session.Player.Account.IsVip())
        {
            return false;
        }

        if (Gender != Gender.Neutral && Gender != session.Player.Gender)
        {
            session.Send(NoticePacket.Notice(SystemNotice.ErrorGender, noticeType));
            return false;
        }

        if (IsBound() && !IsSelfBound(session.Player.CharacterId))
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrPutonInvalidBinding, noticeType));
            return false;
        }

        List<Job> jobs = ItemMetadataStorage.GetRequiredJobs(Id);
        if (!jobs.Contains(Job.None) && !jobs.Contains(session.Player.Job))
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrPutonJob, noticeType));
            return false;
        }

        if (IsExpired())
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrPutonExpired, noticeType));
            return false;
        }

        // TODO: Handle PC Bang

        if (Level > session.Player.Levels.Level)
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrPutonLowLevel, noticeType));
            return false;
        }

        int maxLevel = ItemMetadataStorage.GetLimitMetadata(Id).LevelLimitMax;
        return maxLevel == 0 || maxLevel >= session.Player.Levels.Level;
    }

    public bool CanUse(GameSession session)
    {
        NoticeType noticeType = NoticeType.Chat | NoticeType.FastText;

        if (ItemMetadataStorage.GetLimitMetadata(Id).VipOnly && !session.Player.Account.IsVip())
        {
            return false;
        }

        if (Gender != Gender.Neutral && Gender != session.Player.Gender)
        {
            session.Send(NoticePacket.Notice(SystemNotice.ErrorGender, noticeType));
            return false;
        }

        if (IsBound() && !IsSelfBound(session.Player.CharacterId))
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrUseInvalidBinding, noticeType));
            return false;
        }

        List<Job> jobs = ItemMetadataStorage.GetRequiredJobs(Id);
        if (!jobs.Contains(Job.None) && !jobs.Contains(session.Player.Job))
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrDisableJob, noticeType));
            return false;
        }

        if (IsExpired())
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrPutonExpired, noticeType));
            return false;
        }

        // TODO: Handle PC Bang

        if (Level > session.Player.Levels.Level)
        {
            session.Send(NoticePacket.Notice(SystemNotice.ItemErrUseLowLevel, noticeType));
            return false;
        }

        int maxLevel = ItemMetadataStorage.GetLimitMetadata(Id).LevelLimitMax;
        if (maxLevel != 0)
        {
            return maxLevel >= session.Player.Levels.Level;
        }

        return true;
    }

    public int GetGearScore()
    {
        int gearScoreFactor = ItemMetadataStorage.GetPropertyMetadata(Id).GearScoreFactor;
        Script script = ScriptLoader.GetScript("Functions/calcItemValues");
        DynValue result = script.RunFunction("calcItemGearScore", gearScoreFactor, Rarity, (int) Type, EnchantLevel, LimitBreakLevel);
        return (int) result.Tuple[0].Number + (int) result.Tuple[1].Number;
    }
}

public class DropInformation
{
    public int SourceObjectId;

    public long BoundToCharacterId;

    // Used to stop fadeout task
    public readonly CancellationTokenSource CancellationToken = new();
}
