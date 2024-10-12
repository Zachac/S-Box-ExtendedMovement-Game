using Sandbox;

public sealed class MapSelect : Component
{
	protected override void OnAwake()
	{

		if (Networking.IsHost) {
			Game.Overlay.ShowPackageSelector( "type:map sort:trending", (e) => {
				Scene.GetComponentsInChildren<MapInstance>().First().MapName = e.FullIdent;
			});
		}
	}
}
