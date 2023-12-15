using System.Diagnostics;
using Kitchen;
using Kitchen.DKatGames.InfiniteReroll;
using KitchenMods;
using MessagePack;
using TMPro;
using Unity.Entities;
using Windows.Foundation.Metadata;

public class InfiniteRerollSubview : UpdatableObjectView<InfiniteRerollSubview.ViewData>
{
	public class UpdateView : IncrementalViewSystemBase<ViewData>, IModSystem
	{
		private EntityQuery views;

		protected override void Initialise()
		{
			base.Initialise();
			views = GetEntityQuery(new QueryHelper()
				.All(typeof(CLinkedView), typeof(CInfiniteReroll))
				);
		}

		protected override void OnUpdate()
		{
			var viewsArray = views.ToComponentDataArray<CLinkedView>(Unity.Collections.Allocator.Temp);
			var componentsArray = views.ToComponentDataArray<CInfiniteReroll>(Unity.Collections.Allocator.Temp);

			for (int i = 0; i < viewsArray.Length; ++i)
			{
				var view = viewsArray[i];
				var data = componentsArray[i];

				SendUpdate(view, new ViewData { }, MessageType.SpecificViewUpdate);
			}
		}
	}

	[MessagePackObject]
	public struct ViewData : ISpecificViewData, IViewData.ICheckForChanges<ViewData>
	{
		public IUpdatableObject GetRelevantSubview(IObjectView view)
		{
			return view.GetSubView<InfiniteRerollSubview>();
		}

		public bool IsChangedFrom(ViewData check)
		{
			return true;
		}
	}

	private TextMeshPro textComp;

	protected override void UpdateData(ViewData viewData)
	{
		if (textComp == null)
		{
			textComp = GetComponentInChildren<TextMeshPro>();
		}
		else
		{
			textComp.text = base.Localisation["LABEL_REROLL"];
		}
	}
}