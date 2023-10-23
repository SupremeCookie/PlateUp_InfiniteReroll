using System.Linq;
using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

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
            if (data.Context.Has<CRerollShopAfterDuration>())
            {
                return true;
            }

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
            Vector3[] positions = new Vector3[20];
            Entity[] entityToRemove = new Entity[20];

            int itemIndex = 0;
            int letterIndex = 0;

            var bpQuery = Main.instance.GetBlueprintEntityQuery();
            var items = bpQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            Logger.Log($"Blueprint Items Count: {items.Length}");

            foreach (var item in items)
            {
                Logger.Log($"BP component count: {EntityManager.GetComponentCount(item)}");
                positions[itemIndex] = EntityManager.GetComponentData<CPosition>(item).Position;
                entityToRemove[itemIndex] = item;
                itemIndex++;
            }

            letterIndex = itemIndex;

            var letterBPQuery = Main.instance.GetLetterBlueprintQuery();

            var letterItems = letterBPQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            Logger.Log($"Store Entity Count: {letterItems.Length}");

            foreach (var letterItem in letterItems)
            {
                Logger.Log($"Letter Item component count: {EntityManager.GetComponentCount(letterItem)}");
                positions[letterIndex] = EntityManager.GetComponentData<CPosition>(letterItem).Position;
                entityToRemove[letterIndex] = letterItem;

                letterIndex++;
            }

            int finalIndex = letterIndex;

            for (int i = 0; i < finalIndex; ++i)
            {
                Entity newEntity = EntityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition));
                EntityManager.SetComponentData(newEntity, new CCreateAppliance() { ID = 1063254979, });     // Note DK: 1063254979 is blueprint appliance ID
                EntityManager.SetComponentData(newEntity, new CPosition() { Position = positions[i], });

                // This now makes a blueprint, but the blueprint is empty. So I gotta figure out how to get a random blueprint :)
            }

            for (int i = 0; i < finalIndex; ++i)
            {
                EntityManager.DestroyEntity(entityToRemove[i]);
            }


            items.Dispose();
            letterBPQuery.Dispose();
        }
    }
}
