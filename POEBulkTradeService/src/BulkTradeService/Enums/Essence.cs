namespace WorkerService1.BulkTradeService;

using System;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;

public enum Essence
{
    // Shrieking Essences
    [EnumMember(Value = "shrieking-essence-of-anger")] ShriekingEssenceOfAnger,
    [EnumMember(Value = "shrieking-essence-of-anguish")] ShriekingEssenceOfAnguish,
    [EnumMember(Value = "shrieking-essence-of-contempt")] ShriekingEssenceOfContempt,
    [EnumMember(Value = "shrieking-essence-of-doubt")] ShriekingEssenceOfDoubt,
    [EnumMember(Value = "shrieking-essence-of-dread")] ShriekingEssenceOfDread,
    [EnumMember(Value = "shrieking-essence-of-envy")] ShriekingEssenceOfEnvy,
    [EnumMember(Value = "shrieking-essence-of-fear")] ShriekingEssenceOfFear,
    [EnumMember(Value = "shrieking-essence-of-greed")] ShriekingEssenceOfGreed,
    [EnumMember(Value = "shrieking-essence-of-hatred")] ShriekingEssenceOfHatred,
    [EnumMember(Value = "shrieking-essence-of-loathing")] ShriekingEssenceOfLoathing,
    [EnumMember(Value = "shrieking-essence-of-misery")] ShriekingEssenceOfMisery,
    [EnumMember(Value = "shrieking-essence-of-rage")] ShriekingEssenceOfRage,
    [EnumMember(Value = "shrieking-essence-of-scorn")] ShriekingEssenceOfScorn,
    [EnumMember(Value = "shrieking-essence-of-sorrow")] ShriekingEssenceOfSorrow,
    [EnumMember(Value = "shrieking-essence-of-spite")] ShriekingEssenceOfSpite,
    [EnumMember(Value = "shrieking-essence-of-suffering")] ShriekingEssenceOfSuffering,
    [EnumMember(Value = "shrieking-essence-of-torment")] ShriekingEssenceOfTorment,
    [EnumMember(Value = "shrieking-essence-of-woe")] ShriekingEssenceOfWoe,
    [EnumMember(Value = "shrieking-essence-of-wrath")] ShriekingEssenceOfWrath,

    // Deafening Essences
    [EnumMember(Value = "deafening-essence-of-anger")] DeafeningEssenceOfAnger,
    [EnumMember(Value = "deafening-essence-of-anguish")] DeafeningEssenceOfAnguish,
    [EnumMember(Value = "deafening-essence-of-contempt")] DeafeningEssenceOfContempt,
    [EnumMember(Value = "deafening-essence-of-doubt")] DeafeningEssenceOfDoubt,
    [EnumMember(Value = "deafening-essence-of-dread")] DeafeningEssenceOfDread,
    [EnumMember(Value = "deafening-essence-of-envy")] DeafeningEssenceOfEnvy,
    [EnumMember(Value = "deafening-essence-of-fear")] DeafeningEssenceOfFear,
    [EnumMember(Value = "deafening-essence-of-greed")] DeafeningEssenceOfGreed,
    [EnumMember(Value = "deafening-essence-of-hatred")] DeafeningEssenceOfHatred,
    [EnumMember(Value = "deafening-essence-of-loathing")] DeafeningEssenceOfLoathing,
    [EnumMember(Value = "deafening-essence-of-misery")] DeafeningEssenceOfMisery,
    [EnumMember(Value = "deafening-essence-of-rage")] DeafeningEssenceOfRage,
    [EnumMember(Value = "deafening-essence-of-scorn")] DeafeningEssenceOfScorn,
    [EnumMember(Value = "deafening-essence-of-sorrow")] DeafeningEssenceOfSorrow,
    [EnumMember(Value = "deafening-essence-of-spite")] DeafeningEssenceOfSpite,
    [EnumMember(Value = "deafening-essence-of-suffering")] DeafeningEssenceOfSuffering,
    [EnumMember(Value = "deafening-essence-of-torment")] DeafeningEssenceOfTorment,
    [EnumMember(Value = "deafening-essence-of-woe")] DeafeningEssenceOfWoe,
    [EnumMember(Value = "deafening-essence-of-wrath")] DeafeningEssenceOfWrath,

    // Special Essences
    [EnumMember(Value = "essence-of-hysteria")] EssenceOfHysteria,
    [EnumMember(Value = "essence-of-delirium")] EssenceOfDelirium,
    [EnumMember(Value = "essence-of-horror")] EssenceOfHorror,
    [EnumMember(Value = "essence-of-insanity")] EssenceOfInsanity,
    [EnumMember(Value = "remnant-of-corruption")] RemnantOfCorruption
}
