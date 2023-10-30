using Unity.Entities;
using UnityEngine;

namespace Kitchen.DKatGames.InfiniteReroll
{
	public class ReRollEntityLogic
	{
		private const int rerollApplianceID = 1171429989;

		private static EntityManager entityManager;

		private bool desiredState;
		private EntityQuery blueprints;

		//private int rerollEntityIndex;
		private int createDelay = 0;

		public bool IsActive { get; private set; }

		public static ReRollEntityLogic Create(EntityManager entityManager)
		{
			ReRollEntityLogic.entityManager = entityManager;
			return new ReRollEntityLogic();
		}


		public void Init()
		{
			blueprints = Main.instance.GetBlueprintEntityQuery();
		}

		public void SetActive(bool active)
		{
			desiredState = active;
		}

		public void OnUpdate()
		{
			//Logger.Log($"Update: {deltaSeconds},  isActive: {IsActive},  desiredState: {desiredState}");

			if (IsActive)
			{
				// Note DK: createDelay shouldn't kick in the first go around, but will catch any hiccups in multi threaded creating of the entity. This way we don't accidentally end up with multiple of the same entities.
				if (createDelay > 0)
				{
					createDelay--;
				}

				var rerollQuery = Main.instance.GetCInfiniteRerollQuery();
				var rerollItems = rerollQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
				//Logger.Log($"Did we find any rerollItems?: ({rerollItems.Length}),  createDelay: {createDelay}");

				bool elementIsPresent = rerollItems.Length > 0;
				if (!elementIsPresent && createDelay <= 0)
				{
					//Logger.Log($"Trying to make new CInfiniteReroll");
					Entity newE = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition), typeof(CInfiniteReroll), typeof(CDoNotPersist));

					SetPos(newE, GetValidWorldPos());
					entityManager.SetComponentData(newE, new CCreateAppliance() { ID = rerollApplianceID, });

					//rerollEntityIndex = newE.Index;
					createDelay = 10;
				}
				else if (elementIsPresent)
				{
					//int index = 0;
					//Logger.Log($"Trying to find reroll items,  did we?: {rerollItems.Length}");
					foreach (var reroll in rerollItems)
					{
						//Logger.LogEntityComponents(reroll, $" i ({index}) ");
						//index++;

						// Cleanup
						bool shouldCleanEntity = entityManager.HasComponent<CRerollShopAfterDuration>(reroll);
						if (shouldCleanEntity)
						{
							entityManager.RemoveComponent<CRerollShopAfterDuration>(reroll);
						}

						// Position set
						SetPos(reroll, GetValidWorldPos());
					}
				}

				// Note DK: Cleanup of excess items, shouldn't be present, but this is there just in case we do. (remnants from upgrading the mod for example)
				if (rerollItems.Length > 1)
				{
					//Logger.Log($"Destroying {rerollItems.Length - 1} excess infinite reroll entities");
					int index = 0;
					foreach (var item in rerollItems)
					{
						if (index == 0) // Note DK: Skip the first element
						{
							index++;
							continue;
						}

						entityManager.DestroyEntity(item);
					}
				}
			}

			var bps = blueprints.ToEntityArray(Unity.Collections.Allocator.Temp);
			//Logger.Log($"BluePrintsCount: {bps.Length}");

			if (bps.Length == 0)
			{
				SelfDestruct();
				desiredState = false;
				OnTurnedOff();
				return;
			}

			if ((!desiredState && IsActive) || (desiredState && bps.Length > 0 && !IsActive))
			{
				IsActive = desiredState;

				if (desiredState)
				{
					OnTurnedOn();
				}
				else
				{
					OnTurnedOff();
				}
			}
		}

		private void OnTurnedOn()
		{
		}

		private void OnTurnedOff()
		{
			createDelay = 0;
		}

		private void SetPos(Entity entity, Vector3 position)
		{
			entityManager.SetComponentData(entity, new CPosition() { Position = position, });
		}

		// Note DK: We sit to the right of the practice mode item, but only if the regular reroll item isn't there
		private Vector3 GetValidWorldPos()
		{
			var practiceModeQuery = Main.instance.GetPracticeStarterQuery();
			var regularRerollQuery = Main.instance.GetRegularRerollQuery();

			var practiceModeItems = practiceModeQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

			var rerollItems = regularRerollQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

			Vector3 practiceModeLocation = new Vector3(-100, 0, 0);
			Vector3 rerollLocation = new Vector3(-100, 0, 0);

			foreach (var item in practiceModeItems)
			{
				practiceModeLocation = entityManager.GetComponentData<CPosition>(item).Position;
			}

			foreach (var item in rerollItems)
			{
				rerollLocation = entityManager.GetComponentData<CPosition>(item).Position;
			}

			Vector3 resultPos = practiceModeLocation + new Vector3(1, 0, 0);
			if ((rerollLocation - resultPos).sqrMagnitude < 0.1f)
			{
				resultPos += new Vector3(1, 0, 0);
			}

			//Logger.Log($"Get the valid world pos: {resultPos},  {practiceModeLocation},  {rerollLocation}");
			return resultPos;
		}

		private void SelfDestruct()
		{
			var itemQuery = Main.instance.GetCInfiniteRerollQuery();
			var items = itemQuery.ToEntityArray(Unity.Collections.Allocator.Temp);

			foreach (var item in items)
			{
				entityManager.DestroyEntity(item);
			}
		}
	}
}
