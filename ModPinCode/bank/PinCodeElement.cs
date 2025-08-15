using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NQutils.Def;
using NQ;

namespace NQutils.Def
{
    public partial class PinCodeElement : DoorUnit
    {
        public static new ulong Id = GameplayObject.IdFor("PinCodeElement");

        /// <summary>Minimum length for pin codes</summary>
        public int pinCodeMinLength { get; internal set; } = 4;

        /// <summary>Maximum length for pin codes</summary>
        public int pinCodeMaxLength { get; internal set; } = 12;

        /// <summary>Maximum number of failed attempts before cooldown</summary>
        public int maxFailAttempts { get; internal set; } = 3;

        /// <summary>Cooldown time in seconds after max failed attempts</summary>
        public double failCooldownTime { get; internal set; } = 300.0; // 5 minutes

        /// <summary>Number of failed attempts</summary>
        public static readonly DynamicPropertyInt d_nFailedAttempt;
        /// <summary>compatibility accessor, don't use it, use 'd_nFailedAttempt' instead.</summary>
        [JsonIgnore]
        public static DynamicPropertyInt d_NFailedAttempt { get { return d_nFailedAttempt; }}

        /// <summary>PIN code for access control</summary>
        public static readonly DynamicPropertyString d_pinCode;
        /// <summary>compatibility accessor, don't use it, use 'd_pinCode' instead.</summary>
        [JsonIgnore]
        public static DynamicPropertyString d_PinCode { get { return d_pinCode; }}

        /// <summary>Timestamp until when the element is in cooldown</summary>
        public static readonly DynamicPropertyDouble d_coolDownUntil;
        /// <summary>compatibility accessor, don't use it, use 'd_coolDownUntil' instead.</summary>
        [JsonIgnore]
        public static DynamicPropertyDouble d_CoolDownUntil { get { return d_coolDownUntil; }}

        static PinCodeElement()
        {
            d_nFailedAttempt = GameplayObject.MakeDynamicPropertyInt(
                name: "nFailedAttempt",
                defaultValue: 0,
                hasMin: true,
                minValue: 0,
                hasMax: true,
                maxValue: 3, // maxFailAttempts default value
                isSavedInBlueprint: true,
                isSavedInInventory: true,
                requireInstance: true
            );

            d_pinCode = GameplayObject.MakeDynamicPropertyString(
                name: "pinCode",
                defaultValue: "",
                isSavedInBlueprint: true,
                isSavedInInventory: true,
                requireInstance: true
            );

            d_coolDownUntil = GameplayObject.MakeDynamicPropertyDouble(
                name: "coolDownUntil",
                defaultValue: 0.0,
                hasMin: true,
                minValue: 0.0,
                isSavedInBlueprint: true,
                isSavedInInventory: true,
                requireInstance: true
            );
        }

        public override List<string> InSurrogateProperties() 
        { 
            return new List<string> {};
        }

        public override void recomputePlugMap() 
        {
            PlugMap.clear();
            PlugMap.addPlug(PlugMapDefinition.Direction.PLUG_IN, PlugType.PLUG_SIGNAL, 1, Link.PLUG_NAME_IN, new (){});
            PlugMap.addPlug(PlugMapDefinition.Direction.PLUG_OUT, PlugType.PLUG_SIGNAL, 1, Link.PLUG_NAME_OUT, new (){});
            addExtraPlugs();
        }
    }
}