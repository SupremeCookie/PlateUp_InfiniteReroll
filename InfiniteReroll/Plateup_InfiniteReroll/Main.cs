using KitchenData;
using KitchenMods;
using Unity.Entities;

namespace Kitchen.DKatGames.InfiniteReroll
{
	public class Main : GenericSystemBase, IModSystem, IModInitializer
	{
		// Note DK: This identification stuff is only present for administration, it has no use as I don't use kitchen mod
		public const string MOD_GUID = "InfiniteReroll.DKatGames.Kitchen";
		public const string MOD_NAME = "Infinite Reroll";
		public const string MOD_VERSION = "1.1.0";
		public const string MOD_AUTHOR = "DKatGames";
		public const string MOD_GAMEVERSION = ">=1.1.7";
		public const string MOD_STEAMWORK_ID = "3060007269";


		public static Main instance;

		private ReRollEntityLogic rerollComp;

		protected override void Initialise()
		{
			Logger.Log($"Initialise {MOD_AUTHOR}'s mod: {MOD_NAME}");

			base.Initialise();

			instance = this;

			rerollComp = ReRollEntityLogic.Create(EntityManager, EntityViewManager);
			rerollComp.Init();
		}

		public void PostActivate(Mod mod) { }

		public void PreInject()
		{
			if (GameData.Main.TryGet<Appliance>(ReRollEntityLogic.rerollApplianceID, out Appliance rerollAppliance)
				&& rerollAppliance.Prefab != null && rerollAppliance.Prefab.GetComponent<InfiniteRerollSubview>() == null)
			{
				rerollAppliance.Prefab.AddComponent<InfiniteRerollSubview>();
			}
		}

		public void PostInject() { }

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
					rerollComp.OnUpdate(/*Time.DeltaTime*/);    // Time is sometimes null
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
		}



		// Note DK: Below code can be used to diagnose all objects

		//var allPositionsQuery = Main.instance.GetAllPositionedEntities();

		//var allPositions = allPositionsQuery.ToEntityArray(Unity.Collections.Allocator.Temp);
		//Logger.Log($"Entity Count: {allPositions.Length}");

		//foreach (var it in allPositions)
		//{
		//    Logger.Log($"Entity's component count: {EntityManager.GetComponentCount(it)},  position: {EntityManager.GetComponentData<CPosition>(it).Position}");

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
