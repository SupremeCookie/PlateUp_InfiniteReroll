using System.Linq;
using Kitchen;
using KitchenData;
using Unity.Entities;
using UnityEngine;

namespace Kitchen_InfiniteReroll
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
                GameData.Main.TryGet<Appliance>(rerollID, out var applianceGameDataObject);
                Logger.Log($"Has Appliance Data: {applianceGameDataObject != null}");
                Logger.Log($"appliance: {applianceGameDataObject.ID}, {applianceGameDataObject.Name}, {applianceGameDataObject.Info}, {applianceGameDataObject.Prefab.name}");

                hasSetRerollEntity = true;

                // Alternative: Entity newEntity = entityManager.CreateEntity(typeof(CCreateAppliance), typeof(CPosition));
                rerollEntity = entityManager.CreateEntity();

                // Alternative: entityManager.SetComponentData(newEntity, new CCreateAppliance() { ID = appliance.ID });
                entityManager.AddComponent<CCreateAppliance>(rerollEntity);
                var createApplianceComponent = entityManager.GetComponentData<CCreateAppliance>(rerollEntity);
                createApplianceComponent.ID = applianceGameDataObject.ID;
                entityManager.SetComponentData<CCreateAppliance>(rerollEntity, createApplianceComponent);

                entityManager.AddComponent<CPosition>(rerollEntity);
                entityManager.AddComponent<CInfiniteReroll>(rerollEntity);

                SetPos(new Vector3(-5, 0, 0));
            }
        }

        private void OnTurnedOn()
        {
            // Position properly.
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
                positionComponent.Position = new Vector3(-5, 0, 0);
                entityManager.SetComponentData<CPosition>(rerollEntity, positionComponent);
            }
        }
    }
}
