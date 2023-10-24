using KitchenData;
using KitchenMods;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Kitchen.DKatGames.InfiniteReroll
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
			if (data.Context.Has<CInfiniteReroll>())
			{
				return true;
			}

			return false;
		}

		protected override bool ShouldAct(ref InteractionData interaction_data)
		{
			//Logger.Log($"Does my entity have CInfiniteReroll?: {EntityManager.HasComponent<CInfiniteReroll>(interaction_data.Target)}");
			//Logger.LogEntityComponents(interaction_data.Target, "Entity we're trying to Should Act with's Components: ");
			if (!EntityManager.HasComponent<CInfiniteReroll>(interaction_data.Target))
			{
				return false;
			}

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
				Logger.Log($"Reroll All Blueprints, {Time.TotalTime}, {GameInfo.IsPreparationTime}");
				ReRollAllBlueprints();
			}
		}

		private void ReRollAllBlueprints()
		{
			var bpQuery = Main.instance.GetBlueprintEntityQuery();
			var items = bpQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
			Logger.Log($"Blueprint Items Count: {items.Length}");
			RerollEntities(ref items);

			var letterBPQuery = Main.instance.GetLetterBlueprintQuery();
			var letterItems = letterBPQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
			Logger.Log($"Store Entity Count: {letterItems.Length}");
			RerollEntities(ref letterItems);
		}

		private void RerollEntities(ref NativeArray<Entity> entities)
		{
			foreach (var item in entities)
			{
				ShoppingTags tags = ((GetOrDefault<SDay>().Day % 5 != 0) ? ShoppingTagsExtensions.DefaultShoppingTag : ShoppingTags.Decoration);
				if ((Require<CHeldBy>(item, out CHeldBy comp) && Has<CPlayer>(comp)) || !Require<CPosition>(item, out CPosition comp2))
				{
					CreateShop(isFixedLocation: false, default(Vector3), tags);
				}
				else
				{
					CreateShop(isFixedLocation: true, comp2, tags);
				}

				EntityManager.DestroyEntity(item);
			}
		}

		private void CreateShop(bool isFixedLocation, Vector3 location, ShoppingTags tags)
		{
			Entity entity = base.EntityManager.CreateEntity(typeof(CNewShop));
			base.EntityManager.AddComponentData(entity, new CNewShop
			{
				Tags = tags,
				Location = location,
				FixedLocation = isFixedLocation,
				StartOpen = true
			});
		}
	}
}
