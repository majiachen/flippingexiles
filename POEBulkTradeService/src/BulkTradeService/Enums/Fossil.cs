using System.Runtime.Serialization;

namespace WorkerService1.BulkTradeService.Enums;

public enum Fossil
{
    [EnumMember(Value = "frigid-fossil")] FrigidFossil,
    [EnumMember(Value = "scorched-fossil")] ScorchedFossil,
    [EnumMember(Value = "metallic-fossil")] MetallicFossil,
    [EnumMember(Value = "jagged-fossil")] JaggedFossil,
    [EnumMember(Value = "aberrant-fossil")] AberrantFossil,
    [EnumMember(Value = "pristine-fossil")] PristineFossil,
    [EnumMember(Value = "dense-fossil")] DenseFossil,
    [EnumMember(Value = "corroded-fossil")] CorrodedFossil,
    [EnumMember(Value = "prismatic-fossil")] PrismaticFossil,
    [EnumMember(Value = "aetheric-fossil")] AethericFossil,
    [EnumMember(Value = "serrated-fossil")] SerratedFossil,
    [EnumMember(Value = "lucent-fossil")] LucentFossil,
    [EnumMember(Value = "shuddering-fossil")] ShudderingFossil,
    [EnumMember(Value = "bound-fossil")] BoundFossil,
    [EnumMember(Value = "perfect-fossil")] PerfectFossil,
    [EnumMember(Value = "deft-fossil")] DeftFossil,
    [EnumMember(Value = "fundamental-fossil")] FundamentalFossil,
    [EnumMember(Value = "faceted-fossil")] FacetedFossil,
    [EnumMember(Value = "bloodstained-fossil")] BloodstainedFossil,
    [EnumMember(Value = "hollow-fossil")] HollowFossil,
    [EnumMember(Value = "fractured-fossil")] FracturedFossil,
    [EnumMember(Value = "glyphic-fossil")] GlyphicFossil,
    [EnumMember(Value = "tangled-fossil")] TangledFossil,
    [EnumMember(Value = "sanctified-fossil")] SanctifiedFossil,
    [EnumMember(Value = "gilded-fossil")] GildedFossil
}