using System.Linq;
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
		//protected override bool RequireHold => true;
		//protected override bool AllowActOrGrab => true;
		protected override bool RequirePress => true;
		protected override InteractionType RequiredType => InteractionType.Act;

		private EntityQuery rerollEntities;

		protected override void Initialise()
		{
			base.Initialise();

			rerollEntities = GetEntityQuery(typeof(CInfiniteReroll));
		}

		protected override bool IsPossible(ref InteractionData data)
		{
			if (EntityManager.HasComponent<CInfiniteReroll>(data.Target))
			{
				Logger.Log($"We've got the CInfiniteReroll component present");
				return true;
			}

			Logger.Log($"We do NOT have the component");
			return false;
		}

		private float rerollAfterDuration = 2f;
		private bool shouldReroll = false;

		protected override bool ShouldAct(ref InteractionData interaction_data)
		{
			Logger.Log($"Does my entity have CInfiniteReroll?: {EntityManager.HasComponent<CInfiniteReroll>(interaction_data.Target)}");
			Logger.LogEntityComponents(interaction_data.Target, "Entity we're trying to Should Act with's Components: ");
			Logger.Log($"ShouldAct:{base.ShouldAct(ref interaction_data)},  {interaction_data.Attempt.IsHeld}, {interaction_data.ShouldAct}, {interaction_data.Attempt.Type}, {interaction_data.Attempt.Result}, {interaction_data.Attempt.Mode}" +
				$",  {interaction_data.Attempt.TransferOnly},  {interaction_data.Attempt.Process},  {interaction_data.Attempt.PerformedBy}");

			return base.ShouldAct(ref interaction_data);
		}

		//protected override bool ShouldAct(ref InteractionData interaction_data)
		//{
		//	Logger.Log($"Does my entity have CInfiniteReroll?: {EntityManager.HasComponent<CInfiniteReroll>(interaction_data.Target)},   hasAttemptInteracton: {EntityManager.HasComponent<CAttemptingInteraction>(interaction_data.Target)}");
		//	Logger.LogEntityComponents(interaction_data.Target, "Entity we're trying to Should Act with's Components: ");
		//	bool shouldAct = base.ShouldAct(ref interaction_data);
		//	if (!EntityManager.HasComponent<CInfiniteReroll>(interaction_data.Target))
		//	{
		//		rerollAfterDuration = 2f;
		//		shouldReroll = false;
		//		return false;
		//	}

		//	//shouldReroll = true;

		//	Logger.Log($"ShouldAct: {shouldAct} {base.ShouldAct(ref interaction_data)},  {interaction_data.Attempt.IsHeld}, {interaction_data.ShouldAct}, {interaction_data.Attempt.Type}, {interaction_data.Attempt.Mode}");

		//	interaction_data.Attempt.Type = InteractionType.Act;

		//	Logger.Log($"ShouldAct: {shouldAct} {base.ShouldAct(ref interaction_data)},  {interaction_data.Attempt.IsHeld}, {interaction_data.ShouldAct}, {interaction_data.Attempt.Type}, {interaction_data.Attempt.Mode}");
		//	return base.ShouldAct(ref interaction_data);
		//}

		//protected override void OnUpdate()
		//{
		//	//if (shouldReroll)
		//	//{
		//	//	rerollAfterDuration -= Time.DeltaTime;
		//	//	if (rerollAfterDuration < 0)
		//	//	{
		//	//		rerollAfterDuration = 2f;
		//	//		EntityManager.CreateEntity(typeof(CShopRerollRequest));
		//	//		//EntityManager.AddComponent<CShopRerollRequest>(rerollEntities.ToEntityArray(Allocator.Temp).FirstOrDefault());
		//	//	}
		//	//}

		//	//base.OnUpdate();

		//	//var entities = rerollEntities.ToEntityArray(Unity.Collections.Allocator.Temp);

		//	//entities.Dispose();
		//}

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
