using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Kitchen;
using KitchenMods;
using Unity.Entities;
using UnityEngine;

// Make my namespace start with Kitchen.
namespace Plateup_InfiniteReroll
{
    public class Main : GenericSystemBase, IModSystem
    {
        private ReRollLogic rerollComp;

        public static Main instance;

        protected override void Initialise()
        {
            Debug.Log($"[DKatGames] -- Initialise Infinite Reroll");

            base.Initialise();

            instance = this;

            //bpt = new SpawnBlueprintTest();
            //bpt.Initialise(EntityManager);

            rerollComp = ReRollLogic.Create(EntityManager);
            rerollComp.Init();
        }


        // SCInteractionSystem: InteractionSystem, IModSystem // Define the RequiredType property


        protected override void OnUpdate()
        {
            // Show the infinite reroller!
            // BoxTest shows us that we can spawn a box, a primitive, that has collision. 
            // So we can use that to spawn a GameObject for the reroller, with our required stuff on it
            // But we gotta somehow find? or otherwise, rebuild? the reroll item, and place it!

            // For that we need to tap into the layout.

            //Debug.Log($"[DKatGames] We have a GameInfo: {GameInfo.CurrentSetting},   {GameInfo.IsPreparationTime},   {GameInfo.CurrentDay},   {GameInfo.CurrentScene}");

            bool inKitchen = GameInfo.CurrentScene == SceneType.Kitchen;
            bool isPastInitialDay = GameInfo.CurrentDay >= 1;
            bool isPrepTime = GameInfo.IsPreparationTime;

            if (rerollComp != null)
            {
                if (inKitchen && isPastInitialDay && isPrepTime)
                {
                    if (!rerollComp.IsActive)
                    {
                        rerollComp.SetActive(true);
                    }
                }
                else
                {
                    if (rerollComp.IsActive)
                    {
                        rerollComp.SetActive(false);
                    }
                }

                if (inKitchen)
                {
                    rerollComp.OnUpdate(Time.DeltaTime);
                }
            }
        }


        public EntityQuery GetBlueprintEntityQuery()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CApplianceBlueprint)));

            return newQuery;
        }
    }
}
