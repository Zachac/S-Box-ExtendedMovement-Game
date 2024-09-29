using Sandbox;
using Sandbox.Internal;
using Sandbox.Citizen;
using System.Diagnostics;
using System;
using System.Security;

public sealed class PlayerMovement : Component
{
	[Property] public float GroundControl { get; set; } = 4.0f;
	[Property] public float AirControl { get; set; } = 0.1f;
	[Property] public float MaxForce { get; set; } = 50f;
	[Property] public float RunSpeed { get; set; } = 290f;
	[Property] public float CrouchSpeed { get; set; } = 290f;
	[Property] public float JumpForce { get; set; } = 200f;

	[Property] public GameObject Head { get; set; }
	[Property] public GameObject Body { get; set; }
	[Property] public GameObject CameraPrefab { get; set; }

	[Sync] public bool IsCrouching { get; set; }
	[Sync] Vector3 WishVelocity { get; set; }
	[Sync] Rotation HeadRotation { get; set; }
	[Sync] Vector3 Velocity { get; set; }
	[Sync] bool IsOnGround { get; set; }

	public bool IsSprinting = false;
	private bool Jumped = false;
	private CharacterController characterController;
	private CitizenAnimationHelper animationHelper;
	private SkinnedModelRenderer Model;
	private CameraMovement Camera;
	private float LastWallJump;
	private SoundHandle LastSlideSound;
	private float LastSlideTime;
	private float SlideStart;
	private bool IsSliding;
	private float LastFootStep;

	protected override void OnAwake()
	{
		characterController = Components.Get<CharacterController>();
		animationHelper = Components.Get<CitizenAnimationHelper>();
		Model = Body.Components.Get<SkinnedModelRenderer>();
		Model.OnFootstepEvent += HandleFootstepEvent;

		if ( IsProxy )
		{
			UnHide();
			return;
		}

		IsCrouching = false;
		WishVelocity = new Vector3( 0.0f );
		HeadRotation = Head.Transform.Rotation;


		if ( Network.Owner == null )
		{
			// we are in local, and need to wait for our owner to be assigned if we are going to initialize
			return;
		}

		var camera = CameraPrefab.Clone( Transform.World, GameObject, true, "Camera" );
		Camera = camera.Components.Get<CameraMovement>();
		camera.NetworkMode = NetworkMode.Never;

		// Head.AddComponent<AudioListener>();

		// assign the cc for the UI to track the speed/location/etc
		GameObject.Parent.Children.ForEach( e =>
		{

			if ( e.Name.Equals( "GameModeUI" ) )
			{
				var root = e.Components.Get<UiRootPanel>();
				PlayerInfo.SetCamera( Camera );
				PlayerInfo.SetPlayer( characterController );

			}
		} );
	}

	public void Hide()
	{
		Model.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;

		Model.GameObject.Children.ForEach( e =>
		{
			var subrenderer = e.Components.Get<ModelRenderer>();
			if ( subrenderer != null )
			{
				subrenderer.RenderType = ModelRenderer.ShadowRenderType.ShadowsOnly;
			}
		} );
	}

	public void UnHide()
	{
		Model.RenderType = ModelRenderer.ShadowRenderType.On;

		Model.GameObject.Children.ForEach( e =>
		{
			var subrenderer = e.Components.Get<ModelRenderer>();
			if ( subrenderer != null )
			{
				subrenderer.RenderType = ModelRenderer.ShadowRenderType.On;
			}
		} );
	}

	protected override void OnFixedUpdate()
	{
		if ( IsProxy )
		{
			return;
		}

		BuildWishVelocity();
		Move();
		DoPlayerMovementActions();
	}

	protected override void OnUpdate()
	{
		// Gizmo.Draw.LineBBox( characterController.BoundingBox + Transform.World.Position );
		HeadRotation = Head.Transform.Rotation;
		Velocity = characterController.Velocity;
		IsOnGround = characterController.IsOnGround;

		UpdateAnimation();
		RotateBody();
		// PlayFootStepSounds();
	}

	private void HandleFootstepEvent( SceneModel.FootstepEvent step )
	{
		HandleFootstepEvent( step.Transform.Position, false );
	}
	private void HandleFootstepEvent( Vector3 position, bool alwaysPlay )
	{
		var speed = Velocity.Length;
		var wishSpeed = WishVelocity.Length;


		// Gizmo.Draw.LineBBox(characterController.BoundingBox.Translate(position));

		// Log.Info("New Step Attempted");
		if ( alwaysPlay || (characterController.IsOnGround && (LastFootStep + 0.2f < Time.Now) && (speed > 100 || wishSpeed > 10)) )
		{
			// Log.Info("New Step Created");

			var trace = Scene.Trace.Ray( position, position + Vector3.Down * 10f )
				.IgnoreGameObjectHierarchy( GameObject )
				.Run();

			var surface = trace.Surface;
			var sound = surface.Sounds.FootLand;
			var stepSound = Sound.Play( sound, position );
			stepSound.Volume = 0.5f;
			LastFootStep = Time.Now;
		}
	}

	public void PostOwnerInitialize()
	{
		// call it again now that the owner has been set...
		OnAwake();
	}

	void BuildWishVelocity()
	{
		WishVelocity = 0;

		var rot = Head.Transform.Rotation;
		if ( Input.Down( "Forward" ) ) WishVelocity += rot.Forward;
		if ( Input.Down( "Backward" ) ) WishVelocity += rot.Backward;
		if ( Input.Down( "Left" ) ) WishVelocity += rot.Left;
		if ( Input.Down( "Right" ) ) WishVelocity += rot.Right;

		WishVelocity = WishVelocity.Normal.WithZ( 0 );

		if ( !WishVelocity.IsNearZeroLength ) WishVelocity = WishVelocity.Normal;

		if ( IsCrouching ) WishVelocity *= CrouchSpeed; // Crouching takes presedence over sprinting
		else WishVelocity *= RunSpeed;
	}

	void Move()
	{
		// Get gravity from our scene
		var gravity = Scene.PhysicsWorld.Gravity;

		if ( characterController.IsOnGround )
		{
			if ( Settings.AutoHop && Input.Down( "Jump" ) )
			{
				Jump();
			}
			else
			{
				// Apply Friction/Acceleration
				characterController.Velocity = characterController.Velocity.WithZ( 0 );
				characterController.Accelerate( WishVelocity );
				characterController.ApplyFriction( GroundControl );
			}

		}
		else
		{

			var velocity = characterController.Velocity.WithZ( 0 );
			var wishDir = WishVelocity.Normal;
			var veer = velocity.Dot( wishDir );

			if ( Settings.HighAirAcceleration )
			{
				var wishSpeed = 30f;
				var addSpeed = wishSpeed - veer;
				if ( addSpeed > 0 )
				{
					var accelSpeed = 100f * wishSpeed * Time.Delta;

					if ( accelSpeed > addSpeed )
						accelSpeed = addSpeed;

					characterController.Velocity += accelSpeed * wishDir;
				}
			}
			else
			{
				var wishSpeed = 100f;
				var addSpeed = wishSpeed - veer;
				if ( addSpeed > 0 )
				{
					var accelSpeed = 10f * wishSpeed;

					if ( accelSpeed > addSpeed )
						accelSpeed = addSpeed;

					characterController.Accelerate( accelSpeed * wishDir );
				}
			}
		}

		// Move the character controller
		characterController.Move();

		// Apply gravity
		if ( !characterController.IsOnGround )
		{
			var gravityMultiplier = Settings.LowGravity ? 0.25f : 0.5f;
			characterController.Velocity += gravity * Time.Delta * gravityMultiplier;
		}

		Jumped = false;
	}

	void DoPlayerMovementActions()
	{

		// Set our sprinting and crouching states
		UpdateCrouch();
		IsSprinting = Input.Down( "Run" );
		var altJumped = Input.Pressed( "Jump1" ) || Input.Pressed( "Jump2" );
		var jumped = Input.Pressed( "Jump" );
		if ( jumped || altJumped ) Jump();

		// Log.Info( $"Speed {System.Math.Round(characterController.Velocity.Length/100) * 10}" );

		var wallSlide = false;

		if ( Input.Down( "attack2" ) && Settings.WallSlide )
		{
			wallSlide = WallSlide();
		}
		else
		{
			IsSliding = false;
		}


		if ( !wallSlide && (Input.Down( "Jump" ) || altJumped) && Settings.WallJump )
		{

			var rot = Head.Transform.Rotation.Angles();
			rot.pitch = 0;
			var rot2 = rot.ToRotation();

			if ( altJumped && LastWallJump + 0.25f < Time.Now )
			{
				jumped = jumped || altJumped;
			}

			if ( Input.Pressed( "Forward" ) || (jumped && Input.Down( "Forward" )) ) WallJump( rot2.Backward );
			if ( Input.Pressed( "Backward" ) || (jumped && Input.Down( "Backward" )) ) WallJump( rot2.Forward );
			if ( Input.Pressed( "Left" ) || (jumped && Input.Down( "Left" )) ) WallJump( rot2.Right );
			if ( Input.Pressed( "Right" ) || (jumped && Input.Down( "Right" )) ) WallJump( rot2.Left );
		}
	}

	bool WallSlide()
	{

		var rot = HeadRotation.Angles();

		var camTrace = Scene.Trace.Ray( Head.Transform.Position - rot.Forward * 80f, Head.Transform.Position + rot.Forward * 80f )
					.WithoutTags( "player", "trigger" )
					.Run();

		if ( camTrace.Hit )
		{

			if ( !IsSliding )
			{
				IsSliding = true;
				SlideStart = Time.Now;
			}

			var slideDuration = Time.Now - SlideStart;

			var vel = characterController.Velocity;

			if ( vel.z != 0 )
			{
				characterController.Punch( Vector3.Up * -vel );

				if ( LastSlideSound == null || LastSlideTime + 0.5f < Time.Now )
				{
					LastSlideSound = Sound.Play( "impact-melee-cloth", Transform.World.Position );
					LastSlideSound.Pitch = 0.7f;
					LastSlideSound.Volume = 0.25f;
					LastSlideTime = Time.Now;
				}
				else
				{
					LastSlideSound.Position = Transform.Position;
				}
			}

			var friction = slideDuration / 1;
			if ( friction > 1 )
			{
				friction = 1;
			}

			characterController.ApplyFriction( friction );
			characterController.Accelerate( rot.Forward * 30f );
		}
		else
		{
			IsSliding = false;
		}

		return camTrace.Hit;
	}

	void WallJump( Vector3 direction )
	{

		var camTrace = Scene.Trace.Ray( Body.Transform.Position, Body.Transform.Position + direction * 45f )
					.WithoutTags( "player", "trigger" )
					.Run();

		if ( camTrace.Hit )
		{
			characterController.Punch( Vector3.Up * 200f + direction * -200f );
			LastWallJump = Time.Now;
			var handle = Sound.Play( "footstep-concrete-land", camTrace.HitPosition );
			handle.Pitch = 0.5f;
			handle.Volume = 1f;
		}
	}

	void Jump()
	{
		if ( !characterController.IsOnGround ) return;
		if ( Jumped ) return;

		HandleFootstepEvent( GameObject.Transform.Position, true );
		characterController.Punch( Vector3.Up * JumpForce );
		animationHelper?.TriggerJump(); // Trigger our jump animation if we have one
	}

	void UpdateAnimation()
	{
		if ( animationHelper is null ) return;

		animationHelper.WithVelocity( Velocity );
		animationHelper.AimAngle = HeadRotation;
		animationHelper.IsGrounded = IsOnGround;
		animationHelper.WithLook( HeadRotation.Forward, 1, 0.75f, 0.5f );

		animationHelper.MoveStyle = CitizenAnimationHelper.MoveStyles.Auto;
		animationHelper.DuckLevel = IsCrouching ? 1f : 0f;

		if ( !IsOnGround )
		{
			animationHelper.WithWishVelocity( -WishVelocity );
			animationHelper.SpecialMove = IsCrouching ? CitizenAnimationHelper.SpecialMoveStyle.Roll : CitizenAnimationHelper.SpecialMoveStyle.None;
		}
		else
		{
			animationHelper.WithWishVelocity( WishVelocity );

			if ( animationHelper.SpecialMove == CitizenAnimationHelper.SpecialMoveStyle.Roll )
			{
				if ( !IsCrouching || Velocity.Length == 0 )
				{
					animationHelper.SpecialMove = CitizenAnimationHelper.SpecialMoveStyle.None;
				}
			}
		}
	}

	void RotateBody()
	{
		if ( Body is null ) return;

		var targetAngle = new Angles( 0, HeadRotation.Yaw(), 0 ).ToRotation();
		float rotateDifference = Body.Transform.Rotation.Distance( targetAngle );

		// Lerp body rotation if we're moving or rotating far enough https://carsonk.net/S&box-Tutorial-How-to-create-a-Player-Controller/
		if ( rotateDifference > 50f || characterController.Velocity.Length > 10f )
		{
			Body.Transform.Rotation = Rotation.Lerp( Body.Transform.Rotation, targetAngle, Time.Delta * 2f );
		}
	}

	void UpdateCrouch()
	{
		if ( characterController is null ) return;

		if ( Input.Pressed( "Duck" ) && !IsCrouching )
		{
			IsCrouching = true;

			var height = characterController.Height / 2f;
			characterController.Height = height; // Reduce the height of our character controller

			if ( !characterController.IsOnGround )
			{
				characterController.Transform.Position = characterController.Transform.Position + Vector3.Up * height;
				Camera.InstantCrouchCamera();
			}
		}

		if ( Input.Released( "Duck" ) && IsCrouching )
		{
			IsCrouching = false;

			var height = characterController.Height;
			characterController.Height *= 2f; // Return the height of our character controller to normal

			if ( !characterController.IsOnGround )
			{

				var camTrace = Scene.Trace.Size( characterController.BoundingBox ).FromTo( Body.Transform.Position, Body.Transform.Position + Vector3.Down * height )
							.WithoutTags( "player", "trigger" )
							.Run();

				if ( camTrace.Hit )
				{
					height = camTrace.Fraction * height - 0.1f;
				}

				characterController.Transform.Position = characterController.Transform.Position - Vector3.Up * height;
				Camera.InstantUnCrouchCamera( height );
			}
		}
	}
}
