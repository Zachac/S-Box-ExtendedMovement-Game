using Sandbox;

public sealed class AutoBigCityLoader : Component
{
	protected override void OnAwake()
	{
		GameObject.Components.Get<MapInstance>().MapName = "softsplit.gm_bigcity";

	}
}
