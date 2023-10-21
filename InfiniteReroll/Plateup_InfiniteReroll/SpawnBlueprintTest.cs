using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitchen;
using KitchenData;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

// https://github.com/KitchenMods/KitchenLib/blob/7713453c436697fc807ce3efd3fa75bde964f156/KitchenLib/src/References/GDOReferences.cs#L3
// GameDataObject All things that are things run on this. 

namespace Plateup_InfiniteReroll
{
    public class SpawnBlueprintTest
    {
        // Gotta figure out:
        // - How to position my entity
        // - Setting a specific appliance on it so I know it works :)
        // - How to spawn it when the day ends, and only within the game room, not hub
        // - How to destroy it when the day starts
        // - How to interact with it

        public void Initialise(EntityManager entityManager)
        {
            const int id = 1063254979;  // Blueprint
            Entity newEntity = entityManager.CreateEntity();
            if (GameData.Main.TryGet<Item>(id, out Item newItem, warn_if_fail: true))
            {
                entityManager.AddComponentData(newEntity, new CItem
                {
                    ID = id,
                    IsPartial = false,
                    IsTransient = false,
                    IsGroup = false,
                    Category = newItem.ItemCategory,
                    Items = new ItemList(id)
                });

                entityManager.AddComponentData(newEntity, new CRequiresView
                {
                    Type = ViewType.Item,
                });
            }

            Logger.Log($"Spawned a new entity: {newEntity.Index}");
        }

        public void OnUpdate()
        {
        }
    }
}
