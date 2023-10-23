using KitchenMods;
using Unity.Entities;

namespace Kitchen.DKatGames.InfiniteReroll
{
    public class Main : GenericSystemBase, IModSystem
    {
        private ReRollEntityLogic rerollComp;

        public static Main instance;

        protected override void Initialise()
        {
            Logger.Log($"Initialise Infinite Reroll");

            base.Initialise();

            instance = this;

            rerollComp = ReRollEntityLogic.Create(EntityManager);
            rerollComp.Init();
        }

        // Adapt the standard Main/View thing to make this look proper
        // Figure out how to get to workshop

        protected override void OnUpdate()
        {
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

        public EntityQuery GetLetterBlueprintQuery()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CLetterBlueprint)));

            return newQuery;
        }

        public EntityQuery GetPracticeStarterQuery()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CTriggerPracticeMode)));

            return newQuery;
        }

        public EntityQuery GetRegularRerollQuery()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CRerollShopAfterDuration)));

            return newQuery;
        }

        public EntityQuery GetCInfiniteRerollQuery()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CInfiniteReroll)));

            return newQuery;
        }

        public EntityQuery GetAllPositionedEntities()
        {
            var newQuery = GetEntityQuery(new QueryHelper()
                .All(typeof(CPosition)));

            return newQuery;




            // Note DK: Is used to diagnose all objects
            //var allPositionsQuery = Main.instance.GetAllPositionedEntities();

            //var allPositions = allPositionsQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
            //Logger.Log($"Position Count: {allPositions.Length}");

            //foreach (var it in allPositions)
            //{
            //    // Remove components we don't want
            //    // Also paste position value
            //    Logger.Log($"Positions component count: {EntityManager.GetComponentCount(it)},  position: {EntityManager.GetComponentData<CPosition>(it).Position}");

            //    var components = EntityManager.GetComponentTypes(it);
            //    string componentsString = "";
            //    foreach (var comp in components)
            //    {
            //        componentsString += $"({comp.TypeIndex}, {comp.GetManagedType()}), ";
            //    }

            //    Logger.Log($"Components on CPosition entity: {componentsString}");
            //}
        }
    }
}
