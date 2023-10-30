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
				return true;
			}

			return false;
		}

		protected override void OnUpdate()
		{
			if (rerollEntities.IsEmpty)
			{
				return;
			}

			var entities = rerollEntities.ToEntityArray(Allocator.Temp);
			if (entities == null || entities.Length == 0)
			{
				return;
			}

			foreach (var entity in entities)
			{
				if (EntityManager.HasComponent<CTakesDuration>(entity) && EntityManager.GetComponentData<CTakesDuration>(entity).Remaining <= 0f)
				{
					ReRollAllBlueprints();
				}
			}
		}

		protected override void Perform(ref InteractionData data)
		{
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
