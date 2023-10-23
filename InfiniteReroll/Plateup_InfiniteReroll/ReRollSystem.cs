using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitchen;
using KitchenData.Workshop;
using KitchenMods;
using Unity.Entities;

namespace Kitchen_InfiniteReroll
{
    public class ReRollSystem : InteractionSystem, IModSystem
    {
        protected override InteractionMode RequiredMode => InteractionMode.Appliances;
        protected override bool RequireHold => true;
        protected override bool AllowActOrGrab => true;
        protected override bool RequirePress => false;
        protected override InteractionType RequiredType => InteractionType.Act;

        private EntityQuery rerollEntities;

        protected override void Initialise()
        {
            base.Initialise();

            rerollEntities = GetEntityQuery(typeof(CInfiniteReroll));
        }

        protected override bool IsPossible(ref InteractionData data)
        {
            Logger.Log($"IsPossible?");
            if (data.Context.Has<CRerollShopAfterDuration>())
            {
                Logger.Log($"Yep");
                return true;
            }

            Logger.Log("Nope");
            return false;
        }

        protected override bool ShouldAct(ref InteractionData interaction_data)
        {
            interaction_data.Attempt.Type = InteractionType.Act;

            //Logger.Log($"Should Act: {base.ShouldAct(ref interaction_data)}");

            //Logger.Log($"Data: AllowActOrGrab: {AllowActOrGrab}");
            //Logger.Log($"Data: AttemptType: {interaction_data.Attempt.Type}, RequiredType: {base.RequiredType}");

            //Logger.Log($"Test: {interaction_data.Attempt.Type == InteractionType.Act || interaction_data.Attempt.Type == InteractionType.Grab}");
            //Logger.Log($"Test: {(!RequirePress || !interaction_data.Attempt.IsHeld)}");

            //Logger.Log($"interaction_data.Attempt.IsHeld: {interaction_data.Attempt.IsHeld}");

            return base.ShouldAct(ref interaction_data);
        }

        protected override void OnUpdate()
        {
            base.OnUpdate();

            var entities = rerollEntities.ToEntityArray(Unity.Collections.Allocator.Temp);

            foreach (var entity in entities)
            {
                Logger.Log($"OnUpdate, RerollSystem:  {EntityManager.GetComponentCount(entity)}");
            }

            entities.Dispose();
        }

        protected override void Perform(ref InteractionData data)
        {
            if (data.Target != Entity.Null && GameInfo.IsPreparationTime)
            {
                //Logger.Log($"Performing ReRollSystem's Perform method:  {data.Context}, {data.Interactor}, {data.Attempt}, {data.ShouldAct}, {data.Target}");
                Logger.Log($"Reroll All Blueprints, {Time.TotalTime}, {GameInfo.IsPreparationTime}");
                ReRollAllBlueprints();
            }
        }

        private void ReRollAllBlueprints()
        {


            var bpQuery = Main.instance.GetBlueprintEntityQuery();

            var items = bpQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            Logger.Log($"Blueprint Items Count: {items.Length}");

            foreach (var it in items)
            {
                Logger.Log($"BP component count: {EntityManager.GetComponentCount(it)}");
            }


            var letterBPQuery = Main.instance.GetLetterBlueprintQuery();

            var letterItems = letterBPQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            Logger.Log($"Store Entity Count: {letterItems.Length}");

            foreach (var si in letterItems)
            {
                Logger.Log($"BPStoreEntity component count: {EntityManager.GetComponentCount(si)}");

                //var components = EntityManager.GetComponentTypes(si, Unity.Collections.Allocator.Temp);
                //string componentsString = "";
                //foreach (var comp in components)
                //{
                //    componentsString += $"({comp.TypeIndex}, {comp.GetManagedType()}), ";
                //}

                //Logger.Log($"Components on Store Items entity: {componentsString}");
                //components.Dispose();
            }



            items.Dispose();
            letterBPQuery.Dispose();
        }
    }
}
