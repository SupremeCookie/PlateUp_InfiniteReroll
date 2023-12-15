using System.Linq;
using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace Kitchen.DKatGames.InfiniteReroll
{
	public class ReRollEntityLogic
	{
		public const int rerollApplianceID = 1171429989;

		private static EntityManager entityManager;
		private static EntityViewManager entityViewManager;

		private bool desiredState;
		private EntityQuery blueprints;

		//private int rerollEntityIndex;
		private int createDelay = 0;

		public bool IsActive { get; private set; }

		public static ReRollEntityLogic Create(EntityManager entityManager, EntityViewManager entityViewManager)
		{
			ReRollEntityLogic.entityManager = entityManager;
			ReRollEntityLogic.entityViewManager = entityViewManager;
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
					Entity newE = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition), typeof(CInfiniteReroll), typeof(CDoNotPersist));

					SetPos(newE, GetValidWorldPos());
					entityManager.SetComponentData(newE, new CCreateAppliance() { ID = rerollApplianceID, });

					createDelay = 10;
				}
				else if (elementIsPresent)
				{
					//int index = 0;
					//Logger.Log($"Trying to find reroll items,  did we?: {rerollItems.Length}");

					foreach (var reroll in rerollItems)
					{
#if false
						Logger.Log($"---");
						Logger.LogEntityComponents(reroll, $" i ({index}) ");
						index++;

						var applianceData = entityManager.GetComponentData<CAppliance>(reroll);
						Logger.Log($"{applianceData.Layer},  {applianceData.Layer}");

						var genericInputIndicator = entityManager.GetComponentData<CRequiresGenericInputIndicator>(reroll);
						Logger.Log($"genericInputIndicator {genericInputIndicator.Message.ToString()}");

						var requiresView = entityManager.GetComponentData<CRequiresView>(reroll);
						Logger.Log($"requiresView {requiresView.ViewMode},  {requiresView.Type},  {requiresView.PhysicsDriven}");

						if (entityManager.HasComponent<CLinkedView>(reroll))
						{
							var linkedViewData = entityManager.GetComponentData<CLinkedView>(reroll);
							Logger.Log($"linkedView {linkedViewData.DoNotUpdate},  {linkedViewData.Identifier.Identifier}");
						}

						var actedOnBy = entityManager.GetBuffer<CBeingActedOnBy>(reroll);
						for (int i = 0; i < actedOnBy.Length; ++i)
						{
							Logger.Log($"actedOnBy {i} : {actedOnBy[i].Interactor},  {actedOnBy[i].IsTransferOnly}");
						}

						var attachments = entityManager.GetBuffer<CAttachments>(reroll);
						for (int i = 0; i < attachments.Length; ++i)
						{
							Logger.Log($"attachment {i} : {attachments[i].Entity}");
						}

						var takesDuration = entityManager.GetComponentData<CTakesDuration>(reroll);
						Logger.Log($"takesDuration {takesDuration.Active}, {takesDuration.PreserveProgress}, {takesDuration.RequiresRelease}, {takesDuration.CurrentChange}, {takesDuration.Remaining}, {takesDuration.Total}");
#endif



						// Cleanup
						bool shouldCleanEntity = entityManager.HasComponent<CRerollShopAfterDuration>(reroll);
						if (shouldCleanEntity)
						{
							entityManager.RemoveComponent<CRerollShopAfterDuration>(reroll);
						}
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
