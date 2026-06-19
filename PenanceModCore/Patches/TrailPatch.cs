using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HarmonyLib;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Events;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Models.Cards;
using MegaCrit.Sts2.Core.CardSelection;
using MegaCrit.Sts2.Core.Models.Events;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Runs;

namespace PenanceMod.Scripts.Patches
{
    [HarmonyPatch(typeof(Trial), "Accept")]
    public static class Trial_Accept_Patch
    {
        public static bool Prefix(Trial __instance, ref Task __result)
        {
            // 判断当前角色是否为斥罪 Mod
            if (__instance.Owner?.Character is not PenanceModCode.Character.PenanceMod)
            {
                return true; // 不是斥罪，正常执行原版事件逻辑
            }

            // 如果是斥罪，执行我们魔改后的 Accept 逻辑，并拦截原版
            __result = CustomAccept(__instance);
            return false;
        }

        private static Task CustomAccept(Trial __instance)
        {
            // --- 复刻原版的前置 UI 清理 ---
            if (MegaCrit.Sts2.Core.Context.LocalContext.IsMe(__instance.Owner))
            {
                // 【修复 CS8602】使用 ?. 防止 Layout 为空时报错
                MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom.Instance?.Layout?.RemoveNodesOnPortrait();
            }

            string portraitPath;
            string entryName;
            EventOption[] eventOptions;

            // --- 重写案件分支，加入“公正执法”选项 ---
            int roll = __instance.Rng.NextInt(3);
            switch (roll)
            {
                case 0:
                    // 【修复 CS8600】使用 as string 和 ! 明确告知编译器此反射绝不为 null
                    portraitPath = (AccessTools.Field(typeof(Trial), "_trialMerchantVfx").GetValue(null) as string)!;
                    entryName = "TRIAL.pages.MERCHANT.description";
                    eventOptions = new EventOption[]
                    {
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "MerchantGuilty").Invoke(__instance, null)!, "TRIAL.pages.MERCHANT.options.GUILTY", MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromCardWithCardHoverTips<Regret>()),
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "MerchantInnocent").Invoke(__instance, null)!, "TRIAL.pages.MERCHANT.options.INNOCENT", MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromCardWithCardHoverTips<Shame>()),
                        
                        new EventOption(__instance, () => MerchantFairEnforcement(__instance), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.MERCHANT_FAIR.title"), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.MERCHANT_FAIR.desc"),
                            "MERCHANT_FAIR", Array.Empty<MegaCrit.Sts2.Core.HoverTips.IHoverTip>())
                    };
                    break;

                case 1:
                    portraitPath = (AccessTools.Field(typeof(Trial), "_trialNobleVfx").GetValue(null) as string)!;
                    entryName = "TRIAL.pages.NOBLE.description";
                    eventOptions = new EventOption[]
                    {
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "NobleGuilty").Invoke(__instance, null)!, "TRIAL.pages.NOBLE.options.GUILTY"),
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "NobleInnocent").Invoke(__instance, null)!, "TRIAL.pages.NOBLE.options.INNOCENT", MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromCardWithCardHoverTips<Regret>()),
                        
                        new EventOption(__instance, () => NobleFairEnforcement(__instance), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.NOBLE_FAIR.title"), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.NOBLE_FAIR.desc"),
                            "NOBLE_FAIR", Array.Empty<MegaCrit.Sts2.Core.HoverTips.IHoverTip>())
                    };
                    break;

                case 2:
                    portraitPath = (AccessTools.Field(typeof(Trial), "_trialNondescriptVfx").GetValue(null) as string)!;
                    entryName = "TRIAL.pages.NONDESCRIPT.description";
                    
                    var doubtTip = MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.FromCardWithCardHoverTips<Doubt>();
                    var transformTip = MegaCrit.Sts2.Core.HoverTips.HoverTipFactory.Static(MegaCrit.Sts2.Core.HoverTips.StaticHoverTip.Transform);

                    eventOptions = new EventOption[]
                    {
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "NondescriptGuilty").Invoke(__instance, null)!, "TRIAL.pages.NONDESCRIPT.options.GUILTY", doubtTip),
                        new EventOption(__instance, () => (Task)AccessTools.Method(typeof(Trial), "NondescriptInnocent").Invoke(__instance, null)!, "TRIAL.pages.NONDESCRIPT.options.INNOCENT", doubtTip.Concat(new[] { transformTip })),
                        
                        new EventOption(__instance, () => NondescriptFairEnforcement(__instance), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.NONDESCRIPT_FAIR.title"), 
                            new MegaCrit.Sts2.Core.Localization.LocString("events", "PENANCEMOD.TRIAL.options.NONDESCRIPT_FAIR.desc"),
                            "NONDESCRIPT_FAIR", Array.Empty<MegaCrit.Sts2.Core.HoverTips.IHoverTip>())
                    };
                    break;

                default:
                    throw new ArgumentOutOfRangeException();
            }

            // --- 复刻原版的后置 UI 刷新和状态设置 ---
            AccessTools.Method(typeof(Trial), "AddVfxAnchoredToPortrait").Invoke(__instance, new object[] { portraitPath });

            if (MegaCrit.Sts2.Core.Context.LocalContext.IsMe(__instance.Owner))
            {
                // 【修复 CS8600 和 CS8604】确保取出的字符串一定不是 null
                string trialStartedPath = (AccessTools.Property(typeof(Trial), "TrialStartedPath").GetValue(null) as string)!;
                MegaCrit.Sts2.Core.Nodes.Rooms.NEventRoom.Instance?.SetPortrait(MegaCrit.Sts2.Core.Assets.PreloadManager.Cache.GetTexture2D(trialStartedPath));
            }

            var l10nMethod = AccessTools.Method(typeof(EventModel), "L10NLookup");
            // 【修复 CS8600 和 CS8603】反射调用后使用 ! 声明返回值非空
            var locString = (MegaCrit.Sts2.Core.Localization.LocString)l10nMethod.Invoke(__instance, new object[] { "TRIAL.trialFormat" })!;
            var entryLoc = (MegaCrit.Sts2.Core.Localization.LocString)l10nMethod.Invoke(__instance, new object[] { entryName })!;
            
            locString.Add(new MegaCrit.Sts2.Core.Localization.DynamicVars.StringVar("TrialStory", entryLoc.GetRawText()));

            AccessTools.Method(typeof(EventModel), "SetEventState").Invoke(__instance, new object[] { locString, eventOptions });

            return Task.CompletedTask;
        }

        // =========================================================
        // 分支效果执行逻辑
        // =========================================================

        private static async Task MerchantFairEnforcement(Trial __instance)
        {
            var owner = __instance.Owner;
            if (owner == null) return;

            var relicToGive = MegaCrit.Sts2.Core.Factories.RelicFactory.PullNextRelicFromFront(owner).ToMutable();
            await RelicCmd.Obtain(relicToGive, owner);

            var cardsToUpgrade = await CardSelectCmd.FromDeckForUpgrade(
                owner,
                new CardSelectorPrefs(CardSelectorPrefs.UpgradeSelectionPrompt, 1)
            );
            if (cardsToUpgrade.FirstOrDefault() is CardModel cardToUpgrade)
            {
                CardCmd.Upgrade(cardToUpgrade);
            }

            FinishTrialWithCustomText(__instance, "PENANCEMOD.TRIAL.pages.MERCHANT_FAIR.desc");
        }

        private static async Task NobleFairEnforcement(Trial __instance)
        {
            var owner = __instance.Owner;
            if (owner == null) return;
            
            await CreatureCmd.Heal(owner.Creature, 5m);
            await PlayerCmd.GainGold(150m, owner);

            FinishTrialWithCustomText(__instance, "PENANCEMOD.TRIAL.pages.NOBLE_FAIR.desc");
        }

        private static async Task NondescriptFairEnforcement(Trial __instance)
        {
            var owner = __instance.Owner;
            if (owner == null) return;

            var cardReward = new MegaCrit.Sts2.Core.Rewards.CardReward(
                CardCreationOptions.ForNonCombatWithDefaultOdds(new[] { owner.Character.CardPool }), 
                3, owner
            );
            await RewardsCmd.OfferCustom(owner, new List<MegaCrit.Sts2.Core.Rewards.Reward> { cardReward });

            var cardsToTransform = (await CardSelectCmd.FromDeckForTransformation(
                prefs: new CardSelectorPrefs(CardSelectorPrefs.TransformSelectionPrompt, 1), 
                player: owner
            )).ToList();
            
            foreach (var item in cardsToTransform)
            {
                await CardCmd.TransformToRandom(item, __instance.Rng, CardPreviewStyle.EventLayout);
            }

            FinishTrialWithCustomText(__instance, "PENANCEMOD.TRIAL.pages.NONDESCRIPT_FAIR.desc");
        }

        // =========================================================
        // 辅助方法：拼接裁判结果和 RNG 反应
        // =========================================================
        private static void FinishTrialWithCustomText(Trial __instance, string verdictKey)
        {
            var verdictLoc = new MegaCrit.Sts2.Core.Localization.LocString("events", verdictKey); 
            string verdictText = verdictLoc.GetRawText();

            int reactionRoll = __instance.Rng.NextInt(3);
            string reactionKey = reactionRoll switch
            {
                0 => "PENANCEMOD.TRIAL.reactions.CHEER",
                1 => "PENANCEMOD.TRIAL.reactions.BORED",
                _ => "PENANCEMOD.TRIAL.reactions.ANGRY"
            };
            
            var reactionLoc = new MegaCrit.Sts2.Core.Localization.LocString("events", reactionKey);
            string reactionText = reactionLoc.GetRawText();

            string finalText = $"{verdictText}\n\n{reactionText}";

            var l10nMethod = AccessTools.Method(typeof(EventModel), "L10NLookup");
            var locString = (MegaCrit.Sts2.Core.Localization.LocString)l10nMethod.Invoke(__instance, new object[] { "TRIAL.trialResult" })!;
            
            locString.Add(new MegaCrit.Sts2.Core.Localization.DynamicVars.StringVar("TrialResult", finalText));
            
            AccessTools.Method(typeof(EventModel), "SetEventFinished").Invoke(__instance, new object[] { locString });
        }
    }
}