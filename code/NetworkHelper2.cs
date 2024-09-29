using Sandbox;
using Sandbox.Internal;
using Sandbox.Citizen;
using System;
using System.Threading.Tasks;

public sealed class NetworkHelper2 : Component, Component.INetworkListener
{
	/// <summary>
	/// Create a server (if we're not joining one)
	/// </summary>
	[Property] public bool StartServer { get; set; } = true;

	/// <summary>
	/// The prefab to spawn for the player to control.
	/// </summary>
	[Property] public GameObject PlayerPrefab { get; set; }

	/// <summary>
	/// A list of points to choose from randomly to spawn the player in. If not set, we'll spawn at the
	/// location of the NetworkHelper object.
	/// </summary>

	bool CreatedFirst = false;

	protected override async Task OnLoad()
	{
		if ( Scene.IsEditor )
		{
			return;
		}

		if ( StartServer && !Networking.IsActive )
		{
			LoadingScreen.Title = "Creating Lobby";
			await Task.DelayRealtimeSeconds( 0.1f );
			Networking.CreateLobby();
		}
	}

	/// <summary>
	/// A client is fully connected to the server. This is called on the host.
	/// </summary>
	public void OnActive( Connection channel )
	{
		Log.Info( $"Player '{channel.DisplayName}' has joined the game" );

		if ( !PlayerPrefab.IsValid() )
			return;

		//
		// Find a spawn location for this player
		//
		var startLocation = FindSpawnLocation().WithScale( 1 );

		// Spawn this object and make the client the owner
		var player = PlayerPrefab.Clone( startLocation, name: $"Player - {channel.DisplayName}" );

		var avatarJson = channel.GetUserData( "avatar" );
		var container = new ClothingContainer();

		if ( avatarJson != null )
		{
			container.Deserialize( avatarJson );

			player.Children.ForEach( e =>
			{
				if ( e.Name.Equals( "Body" ) )
				{
					container.Apply( e.Components.Get<SkinnedModelRenderer>() );
				}
			} );
		}

		player.NetworkSpawn( channel );

		if ( !CreatedFirst )
		{
			CreatedFirst = true;
			player.Components.Get<PlayerMovement>().PostOwnerInitialize();
		}
	}

	/// <summary>
	/// Find the most appropriate place to respawn
	/// </summary>
	Transform FindSpawnLocation()
	{

		//
		// If we have any SpawnPoint components in the scene, then use those
		//
		var spawnPoints = Scene.GetAllComponents<SpawnPoint>().ToArray();
		if ( spawnPoints.Length > 0 )
		{
			return Random.Shared.FromArray( spawnPoints ).Transform.World;
		}

		//
		// Failing that, spawn where we are and place the player on the ground
		//
		var offsetCheck = Vector3.Down * 50_000f;
		var start = Transform.Position;

		var trace = Scene.Trace.Ray( start, start + offsetCheck )
			.WithoutTags( "player", "trigger" )
			.Run();

		if (trace.Hit) {
			return Transform.World.Add(offsetCheck * trace.Fraction, true);
		}

		Log.Info("No ground found!");
		return Transform.World;
	}
}