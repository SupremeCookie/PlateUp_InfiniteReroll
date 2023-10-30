using Unity.Entities;

namespace Kitchen.DKatGames.InfiniteReroll
{
	public class CleanupReroll : StartOfDaySystem
	{
		private EntityQuery rerollItemQuery;

		protected override void Initialise()
		{
			base.Initialise();
			rerollItemQuery = GetEntityQuery(typeof(CInfiniteReroll));
		}

		protected override void OnUpdate()
		{
			//Logger.Log($"CleanupReroll, found itemCount: {rerollItemQuery.ToEntityArray(Unity.Collections.Allocator.Temp).Length}");
			base.EntityManager.DestroyEntity(rerollItemQuery);
		}
	}
}
