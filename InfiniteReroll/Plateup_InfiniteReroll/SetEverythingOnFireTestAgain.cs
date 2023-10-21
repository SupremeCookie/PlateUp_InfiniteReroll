//using System.Diagnostics;
//using Kitchen;
//using KitchenMods;
//using Unity.Collections;
//using Unity.Entities;

//namespace Plateup_InfiniteReroll
//{
//  public class SetEverythingOnFireTestAgain : GenericSystemBase, IModSystem
//  {
//    private EntityQuery appliancesQuery;

//    protected override void Initialise()
//    {
//      Logger.Debug.Log($"Initialise SetEverythingOnFire");
//      //base.Initialise();
//      //appliancesQuery = GetEntityQuery(new QueryHelper()
//      //  .All(typeof(CAppliance))
//      //  .None(
//      //      typeof(CFire),
//      //      typeof(CIsOnFire),
//      //      typeof(CFireImmune),
//      //      typeof(CHasBeenSetOnFire)
//      //  ));
//    }

//    protected override void OnUpdate()
//    {

//      //var appliances = appliancesQuery.ToEntityArray(Allocator.TempJob);
//      //string appliancesNames = "";
//      //foreach (var appliance in appliances)
//      //{
//      //  var data = EntityManager.GetComponentData<CAppliance>(appliance);
//      //  appliancesNames += $"({appliance.Index}, {appliance.Version}, {data.Layer}), ";

//      //  if(appliance.Index == 75)
//      //  {
//      //    EntityManager.AddComponent<CIsOnFire>(appliance);
//      //    EntityManager.AddComponent<CIsOnFire>(appliance);
//      //  }
//      //}

//      //Logger.Log($"{Time.TotalTime},  {appliancesNames}");

//      //appliances.Dispose();
//      //var appliances = appliancesQuery.ToEntityArray(Allocator.TempJob);
//      //foreach (var appliance in appliances)
//      //{
//      //  EntityManager.AddComponent<CIsOnFire>(appliance);
//      //  EntityManager.AddComponent<CHasBeenSetOnFire>(appliance);
//      //}

//      //appliances.Dispose();
//    }
//  }
//}
