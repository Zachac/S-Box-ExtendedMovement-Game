using Sandbox;
using Sandbox.Internal;
using Sandbox.Citizen;

public sealed class CameraMovement : Component
{

	[Property] public CharacterController Player { get; set; }
	[Property] public PlayerMovement PlayerMove { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject Head { get; set; }
	[Property, Range( 0f, 300f )] public float Distance { get; set; } = 0f;

	// Variables
	public bool IsFirstPerson => Distance == 0f; // Helpful but not required. You could always just check if Distance == 0f
	private CameraComponent Camera;

	private bool reversedCamera = false;

	Vector3 CurrentOffset = new( 0.0f );

	protected override void OnAwake()
	{
		if ( IsProxy )
		{
			return;
		}

		Player = GameObject.Parent.Components.Get<CharacterController>();
		PlayerMove = GameObject.Parent.Components.Get<PlayerMovement>();

		GameObject.Parent.Children.ForEach( ( e ) =>
		{
			if ( e.Name.Equals( "Head" ) )
			{
				Head = e;
			}
			else if ( e.Name.Equals( "Body" ) )
			{
				Body = e;
			}
		} );

		Camera = Components.Get<CameraComponent>();
		PlayerMove.Hide();
	}

	public void InstantCrouchCamera()
	{
		CurrentOffset = new Vector3( 0, 0, -32f );
		UpdateCamPosition();
	}

	public void InstantUnCrouchCamera( float height )
	{
		CurrentOffset = Vector3.Down * (32 - height);
		UpdateCamPosition();
	}

	protected override void OnUpdate()
	{
		if ( IsProxy )
		{
			return;
		}

		if ( Input.Pressed( "Zoom In" ) )
		{
			if ( Distance < 101f )
			{
				Distance = 0f;
				PlayerMove.Hide();
			}
			else if ( Distance > 199f )
			{
				Distance -= 100f;
			}
		}

		if ( Input.Pressed( "Zoom Out" ) )
		{
			if ( Distance < 201f )
			{
				Distance += 100f;
				PlayerMove.UnHide();
			}
		}

		if ( Input.Pressed( "Reverse Camera" ) )
		{
			reversedCamera = !reversedCamera;
		}

		// Rotate the head based on mouse movement
		var eyeAngles = Head.Transform.Rotation.Angles();
		eyeAngles.pitch += Input.MouseDelta.y * 0.1f;
		eyeAngles.yaw -= Input.MouseDelta.x * 0.1f;
		eyeAngles.roll = 0f;
		eyeAngles.pitch = eyeAngles.pitch.Clamp( -89.9f, 89.9f ); // So we don't break our necks
		Head.Transform.Rotation = eyeAngles.ToRotation();

		Vector3 targetOffset = new( 0.0f );
		if ( PlayerMove.IsCrouching ) targetOffset += Vector3.Down * 32f;
		CurrentOffset = Vector3.Lerp( CurrentOffset, targetOffset, Time.Delta * 10f );

		// apply the changes
		UpdateCamPosition();
	}

	void UpdateCamPosition()
	{
		var eyeAngles = Head.Transform.Rotation.Angles();

		if ( reversedCamera )
		{
			eyeAngles.yaw += 180;
		}

		var camPos = Head.Transform.Position + CurrentOffset;
		if ( !IsFirstPerson )
		{
			// Perform a trace backwards to see where we can safely place the camera
			var camForward = eyeAngles.ToRotation().Forward;
			var wantedPosition = camPos - (camForward * Distance);
			var camTrace = Scene.Trace.Ray( camPos, wantedPosition )
				.WithoutTags( "player", "trigger" )
				.Run();
			if ( camTrace.Hit )
			{
				camPos = camTrace.HitPosition + camTrace.Normal * 7f;
			}
			else
			{
				camPos = camPos - camForward * Distance;
			}
		}

		// Set the position of the camera to our calculated position
		Camera.Transform.Position = camPos;
		Camera.Transform.Rotation = eyeAngles.ToRotation();
	}

}
