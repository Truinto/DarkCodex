using HarmonyLib;
using Kingmaker.EntitySystem;
using Kingmaker.PubSubSystem;
using Kingmaker.RuleSystem;
using System.Collections.Generic;

namespace CodexLib.Patches
{
    /// <summary>
    /// Ensure IBeforeRule comes first.
    /// </summary>
    [HarmonyPatch]
    public class Patch_RulebookEventBusPriority
    {
        [HarmonyPatch(typeof(RulebookSubscribersList<RulebookEvent>), "Kingmaker.PubSubSystem.IRulebookSubscribersList.AddSubscriber")]
        [HarmonyPrefix]
        public static bool Prefix(object subscriber, List<object> ___List)
        {
            //Helper.PrintDebug($"SubscribersList adding {subscriber.GetType()}");
            if (subscriber is EntityFactComponent c1)
            {
                if (typeof(IBeforeRule).IsAssignableFrom(c1.SourceBlueprintComponent.GetType()))
                {
                    ___List.Insert(0, c1);
                    return false;
                }
                //if (typeof(IAfterRule).IsAssignableFrom(c1.SourceBlueprintComponent.GetType()))
                //{
                //    ___List.Add(c1);
                //    return false;
                //}
            }

            //int index;
            //for (index = ___List.Count - 1; index >= 0; index--)
            //{
            //    if (___List[index] is not EntityFactComponent c2 || !typeof(IAfterRule).IsAssignableFrom(c2.SourceBlueprintComponent.GetType()))
            //        break;
            //}
            //___List.Insert(index, subscriber);
            ___List.Add(subscriber);
            return false;
        }
    }
}
