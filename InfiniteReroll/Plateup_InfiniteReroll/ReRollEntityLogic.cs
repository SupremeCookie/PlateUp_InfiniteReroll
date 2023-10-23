using System.Linq;
using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace Kitchen.DKatGames.InfiniteReroll
{
    public class ReRollEntityLogic
    {
        private static EntityManager entityManager;

        private bool desiredState;
        private EntityQuery blueprints;

        private Entity rerollEntity = default;
        private bool hasSetRerollEntity;

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

        public void OnUpdate(float deltaSeconds)
        {
            if (IsActive)
            {
                TryLoadRerollAppliance();

                if (hasSetRerollEntity)
                {
                    //Logger.LogEntityComponents(rerollEntity, "rerollEntity");

                    bool hasCreateApplianceComponent = entityManager.HasComponent<CCreateAppliance>(rerollEntity);
                    bool hasProperlyInstantiated = !hasCreateApplianceComponent;
                    if (hasProperlyInstantiated)
                    {
                        if (entityManager.HasComponent<CRerollShopAfterDuration>(rerollEntity))
                        {
                            entityManager.RemoveComponent<CRerollShopAfterDuration>(rerollEntity);
                            entityManager.AddComponent<CInfiniteReroll>(rerollEntity);
                            SetPos(GetValidWorldPos());
                        }
                    }
                }
            }


            var items = blueprints.ToEntityArray(Unity.Collections.Allocator.Temp);

            if ((!desiredState && IsActive) || (desiredState && items.Count() > 0 && !IsActive))
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

            items.Dispose();
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

                foreach (var entity in Main.instance.GetAllPositionedEntities().ToEntityArray(Unity.Collections.Allocator.Temp))
                {
                    Logger.LogEntityComponents(entity, "Create new Reroll Appliance");
                }

                // Alternative: Entity newEntity = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition));
                rerollEntity = entityManager.CreateEntity();

                // Alternative: entityManager.SetComponentData(newEntity, new CCreateAppliance() { ID = appliance.ID });
                entityManager.AddComponent<CCreateAppliance>(rerollEntity);
                var createApplianceComponent = entityManager.GetComponentData<CCreateAppliance>(rerollEntity);
                createApplianceComponent.ID = applianceGameDataObject.ID;
                entityManager.SetComponentData<CCreateAppliance>(rerollEntity, createApplianceComponent);

                entityManager.AddComponent<CPosition>(rerollEntity);
            }
        }

        private void OnTurnedOn()
        {
            SetPos(GetValidWorldPos());
        }

        private void OnTurnedOff()
        {
            hasSetRerollEntity = false;
            rerollEntity = default;
        }

        private void SetPos(Vector3 position)
        {
            if (hasSetRerollEntity)
            {
                var positionComponent = entityManager.GetComponentData<CPosition>(rerollEntity);
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
