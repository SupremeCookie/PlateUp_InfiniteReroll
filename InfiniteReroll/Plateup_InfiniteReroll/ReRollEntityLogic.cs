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

        // AttemptInteraction is interesting to read.
        // The C types are all just data components, there are systems that then react upon these components, read the ECS docs a bit more
        // I need to add my own reroll logic, and interaction logic. So gotta figure out some long hold things to see how they work, maybe look at filth/mess components? 

        public void OnUpdate(float deltaSeconds)
        {
            if (IsActive)
            {
                TryLoadRerollAppliance();

                if (hasSetRerollEntity)
                {
                    // Default entities are 2 different entities
                    // [INFO] [DKatGames] Components on reroll entity: 16777561, 16777778, 

                    // After a frame we've got
                    // [INFO] [DKatGames] Components on reroll entity: 16777562, 16777563, 16777578, 16777604, 16777620, 16777778, 16777780, 16778091, 16778094, 67109265, 67109790, 1090519419, 1090519883, 1090519910, 

                    var components = entityManager.GetComponentTypes(rerollEntity);
                    string componentsString = "";
                    foreach (var comp in components)
                    {
                        componentsString += $"({comp.TypeIndex}, {comp.GetManagedType()}), ";
                    }

                    Logger.Log($"Components on reroll entity: {componentsString}");

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
            Logger.Log($"Count: {items.Count()}");
            foreach (var item in items)
            {
                var bpData = entityManager.GetComponentData<CApplianceBlueprint>(item);
                Logger.Log($"bpData: {bpData.Appliance}, {bpData.IsCopy}");
            }

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
            //bool hasMyAppliance = GameData.Main.Has<Appliance>(rerollID);
            //Logger.Log($"do we have my Appliance?: {hasMyAppliance}");
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
