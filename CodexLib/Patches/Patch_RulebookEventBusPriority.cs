using HarmonyLib;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using System.Collections.Generic;
using System;

namespace CodexLib.Patches
{
    /// <summary>
    /// Use the IBeforeRule interface to place your component before any other.<br/>
    /// This patch ensures IBeforeRule comes first.
    /// </summary>
    [HarmonyPatch]
    public class Patch_RulebookEventBusPriority
    {
        [HarmonyPatch(typeof(RulebookSubscribersList<RulebookEvent>), "Kingmaker.PubSubSystem.IRulebookSubscribersList.AddSubscriber")]
        [HarmonyPrefix]
        public static bool Prefix(object subscriber, List<object> ___List)
        {
            try
            {
                //Helper.PrintDebug($"SubscribersList adding {subscriber.GetType()}");
                if (subscriber is EntityFactComponent c1)
                {
                    var source = c1.SourceBlueprintComponent;
                    if (source is IBeforeRule)
                    {
                        ___List.Insert(0, c1);
                        return false;
                    }
                    if (source is IAfterRule)
                    {
                        ___List.Add(c1);
                        return false;
                    }
                }

                int index;
                for (index = ___List.Count - 1; index >= 0; index--)
                {
                    if (___List[index] is not EntityFactComponent c2 || c2.SourceBlueprintComponent is not IAfterRule)
                        break;
                }
                ___List.Insert(index + 1, subscriber);

                return false;
            }
            catch (Exception ex) { Helper.PrintException(ex); }

            return true;
        }
    }
}
