using System.Linq;
using KitchenData;
using Unity.Entities;
using UnityEngine;
using Windows.ApplicationModel.Activation;

namespace Kitchen.DKatGames.InfiniteReroll
{
	public class ReRollEntityLogic
	{
		private static EntityManager entityManager;

		private bool desiredState;
		private EntityQuery blueprints;

		private Entity rerollEntity = default;
		private bool hasSetRerollEntity;

		private bool hasRemovedRerollElement;
		private bool hasAddedDoNotPersist;
		private bool hasAddedInfiniteReroll;
		private bool hasFinishedInitializing;

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

		private bool debugHasSet = false;
		public void OnUpdate(float deltaSeconds)
		{
			Logger.Log($"Update: {deltaSeconds},  isActive: {IsActive},  desiredState: {desiredState}");

			if (IsActive)
			{
				// Check if element is there
				// If not, try to make it, only do this every 1/10th of a second
				// If there, check if clean, if not, clean up
				// If there, check if properly positioned, if not, do so

				bool elementIsPresent = debugHasSet;
				if (!elementIsPresent)
				{
					Logger.Log($"Trying to make new CInfiniteReroll");
					Entity newE = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition), typeof(CInfiniteReroll), typeof(CDoNotPersist));


					Logger.Log($"Have we created the new entity?: {newE != Entity.Null}");
					debugHasSet = true;
				}


				//TryLoadRerollAppliance();

				//if (hasSetRerollEntity && !hasFinishedInitializing)
				//{
				//	hasFinishedInitializing = hasRemovedRerollElement && hasAddedDoNotPersist && hasAddedInfiniteReroll;

				//	Logger.Log($"isPrep: {GameInfo.IsPreparationTime}");
				//	Logger.LogEntityComponents(rerollEntity, "rerollEntity");

				//	bool hasCreateApplianceComponent = entityManager.HasComponent<CCreateAppliance>(rerollEntity);
				//	bool hasProperlyInstantiated = !hasCreateApplianceComponent;
				//	if (hasProperlyInstantiated)
				//	{
				//		if (entityManager.HasComponent<CRerollShopAfterDuration>(rerollEntity))
				//		{
				//			entityManager.RemoveComponent<CRerollShopAfterDuration>(rerollEntity);
				//			hasRemovedRerollElement = true;
				//		}

				//		if (!entityManager.HasComponent<CInfiniteReroll>(rerollEntity))
				//		{
				//			entityManager.AddComponent<CInfiniteReroll>(rerollEntity);
				//			hasAddedInfiniteReroll = true;
				//			SetPos(GetValidWorldPos());
				//		}

				//		if (!entityManager.HasComponent<CDoNotPersist>(rerollEntity))
				//		{
				//			entityManager.AddComponent<CDoNotPersist>(rerollEntity);
				//			hasAddedDoNotPersist = true;
				//		}
				//	}
				//}
			}


			var items = blueprints.ToEntityArray(Unity.Collections.Allocator.Temp);

			if ((!desiredState && IsActive) || (desiredState && items.Length > 0 && !IsActive))
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


		private void TryLoadRerollAppliance()
		{
			if (GameData.Main == null)
			{
				Logger.Log($"returning as Main GameData is null");
				return;
			}


			const int rerollID = 1171429989;
			if (!hasSetRerollEntity)
			{
				Logger.Log($"Create the reroll appliance");

				GameData.Main.TryGet<Appliance>(rerollID, out var applianceGameDataObject);
				Logger.Log($"Has Appliance Data: {applianceGameDataObject != null}");
				Logger.Log($"appliance: {applianceGameDataObject.ID}, {applianceGameDataObject.Name}, {applianceGameDataObject.Info}, {applianceGameDataObject.Prefab.name}");

				hasSetRerollEntity = true;

				var infiniteRerollQuery = Main.instance.GetCInfiniteRerollQuery();
				var infiniteRerollItems = infiniteRerollQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
				if (infiniteRerollItems.Length > 0)
				{
					Logger.Log($"Destroying {infiniteRerollItems.Length} infinite reroll entities");
					foreach (var item in infiniteRerollItems)
					{
						entityManager.DestroyEntity(item);
					}
				}



				// Alternative: Entity newEntity = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition));
				rerollEntity = entityManager.CreateEntity();

				// Alternative: entityManager.SetComponentData(newEntity, new CCreateAppliance() { ID = appliance.ID });
				entityManager.AddComponent<CCreateAppliance>(rerollEntity);
				var createApplianceComponent = entityManager.GetComponentData<CCreateAppliance>(rerollEntity);
				createApplianceComponent.ID = applianceGameDataObject.ID;
				entityManager.SetComponentData(rerollEntity, createApplianceComponent);

				entityManager.AddComponent<CPosition>(rerollEntity);
			}
		}

		private void OnTurnedOn()
		{
			//SetPos(GetValidWorldPos());
		}

		private void OnTurnedOff()
		{
			hasSetRerollEntity = false;
			rerollEntity = Entity.Null;

			hasRemovedRerollElement = false;
			hasAddedDoNotPersist = false;
			hasAddedInfiniteReroll = false;
			hasFinishedInitializing = false;
		}

		private void SetPos(Vector3 position)
		{
			Logger.Log($"Set Position: ({position}),   hasSet: {hasSetRerollEntity} isnull?: ({rerollEntity.Equals(Entity.Null)})");
			if (hasSetRerollEntity)
			{
				Logger.Log($"EntityManager null? : {entityManager == null}");
				var positionComponent = entityManager.GetComponentData<CPosition>(rerollEntity);
				Logger.Log($"positionComponent?: ({positionComponent})");
				positionComponent.Position = position;
				entityManager.SetComponentData<CPosition>(rerollEntity, positionComponent);
			}
		}

		// Note DK: We sit to the right of the practice mode item, but only if the regular reroll item isn't there
		private Vector3 GetValidWorldPos()
		{
			var practiceModeQuery = Main.instance.GetPracticeStarterQuery();
			var regularRerollQuery = Main.instance.GetRegularRerollQuery();

			var practiceModeItems = practiceModeQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
			Debug.Assert(practiceModeItems.Length == 1, "There's more than 1 practice mode starter item, that's not good!");

			var rerollItems = regularRerollQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
			Debug.Assert(rerollItems.Length <= 1, "There's more than 1 regular reroll item, that's not good!");

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

			Debug.Log($"Get the valid world pos: {resultPos},  {practiceModeLocation},  {rerollLocation}");
			return resultPos;
		}
	}
}
