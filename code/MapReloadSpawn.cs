using Sandbox;

public sealed class MapReloadSpawn : Component
{
	static MapInstance Map;

	protected override void OnAwake()
	{
		Map = GameObject.Components.Get<MapInstance>();
		Map.OnMapLoaded = () => {
			var players = Scene.GetAllComponents<PlayerMovement>();
			foreach (var p in players) {

				if (!p.IsProxy) {
					var spawnPoint = NetworkHelper2.FindSpawnLocation(Scene);
					p.GameObject.WorldPosition = spawnPoint.Position;
					p.GameObject.WorldRotation = spawnPoint.Rotation;
				}
			}
		};
	}

	public static void LoadMap(string mapName) {

		if (Map == null) {
			Log.Error("Cannot change map. No MapInstance found.");
			return;
		}

		Map.MapName = mapName;
	}
}
