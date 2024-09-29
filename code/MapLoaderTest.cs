using Sandbox;

public sealed class MapLoaderTest : Component
{
	static MapInstance Map;

	protected override void OnAwake()
	{
		Map = GameObject.Components.Get<MapInstance>();
	}

	public static void LoadMap(string mapName) {

		if (Map == null) {
			Log.Error("Cannot change map. No MapInstance found.");
			return;
		}

		Map.MapName = mapName;
	}
}
