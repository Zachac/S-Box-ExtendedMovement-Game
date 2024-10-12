using System;
using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.UI;

public class UiMenu : Panel
{

    public UiMenu()
    {

        Style.Width = Length.ViewWidth( 100 );
        Style.Height = Length.ViewHeight( 100 );
        Style.Margin = Length.Pixels( 0 );
        Style.Padding = Length.Pixels( 0 );
        Style.FontColor = Color.White;
        Style.Position = PositionMode.Absolute;
        Style.Display = DisplayMode.None;
        // Style.JustifyContent = Justify.Center;
        // Style.AlignItems = Align.Center;
        Style.BackdropFilterBlur = Length.Percent( 70 );
        Style.PointerEvents = PointerEvents.All;

        var panel = new Panel();
        panel.Parent = this;
        panel.Style.Display = DisplayMode.Flex;
        panel.Style.FlexDirection = FlexDirection.Column;
        // panel.Style.MarginTop = Length.Auto;
        // panel.Style.MarginLeft = Length.Auto;
        panel.Style.Width = Length.ViewWidth( 15 );
        panel.Style.Height = Length.Auto;
        panel.Style.Margin = Length.Pixels( 0 );
        panel.Style.Padding = Length.Pixels( 10 );
        panel.Style.PaddingTop = Length.Pixels( 25 );
        panel.Style.PaddingBottom = Length.Pixels( 25 );
        panel.Style.JustifyContent = Justify.FlexStart;
        panel.Style.AlignItems = Align.FlexStart;
        panel.Style.Position = PositionMode.Absolute;
        panel.Style.Bottom = Length.ViewHeight( 10 );
        panel.Style.Left = Length.ViewHeight( 15 );
        // panel.Style.BackgroundColor = Color.Black;
        // panel.Style.Opacity = 0.7f;

        var label1 = new Label();
        label1.Text = "Physics Settings";
        label1.Style.BackgroundColor = Color.Black;
        label1.Style.Padding = Length.Pixels( 12 );
        label1.Style.Width = Length.Percent( 100 );
        label1.Style.Opacity = 0.95f;
        label1.Parent = panel;

        new ToggleLabel( "Low Gravity", Settings.LowGravity, b => Settings.LowGravity = b)
            .Parent = panel;

        new ToggleLabel( "High Air Acceleration", Settings.HighAirAcceleration, b => Settings.HighAirAcceleration = b )
            .Parent = panel;

        var header2 = new Label();
        header2.Text = "Other Settings";
        header2.Style.BackgroundColor = Color.Black;
        header2.Style.Padding = Length.Pixels( 12 );
        header2.Style.Width = Length.Percent( 100 );
        header2.Style.MarginTop = Length.Pixels(24);
        header2.Style.Opacity = 0.95f;
        header2.Parent = panel;
        header2.AddEventListener("OnClick", e => {
            MapLoaderTest.LoadMap("softsplit.gm_bigcity");
        });

        new ToggleLabel( "Wall Jump", Settings.WallJump, b => Settings.WallJump = b )
            .Parent = panel;
        new ToggleLabel( "Wall Slide", Settings.WallSlide, b => Settings.WallSlide = b )
            .Parent = panel;

        new ToggleLabel( "Auto Hop", Settings.AutoHop, b => Settings.AutoHop = b )
            .Parent = panel;

        new CustomButton("Respawn", () => {
            var players = Scene.GetAllComponents<PlayerMovement>();
            foreach (var p in players) {
                if (!p.IsProxy) {
                    var spawnPoint = NetworkHelper2.FindSpawnLocation(Scene);
                    p.GameObject.WorldPosition = spawnPoint.Position;
                    p.GameObject.WorldRotation = spawnPoint.Rotation;
                }
            }
        }).Parent = panel;   
    }

    public override void Tick()
    {
        if ( Input.Pressed( "Menu" ) )
        {
            Mouse.Visible = true;

            Style.Display = null;
        }
        else if ( Input.Released( "Menu" ) )
        {
            Mouse.Visible = false;
            Style.Display = DisplayMode.None;
        }
    }

	public class ToggleLabel : Label
    {

        bool Checked;
        Action<bool> UpdateAction;

        public ToggleLabel( string text, bool defaultValue, Action<bool> action)
        {

            Checked = defaultValue;
            Style.BackgroundColor = Color.Black;
            Style.Opacity = Checked ? 0.95f : 0.75f;
            Style.Width = Length.Percent( 100 );
            Style.Padding = Length.Pixels( 8 );
            Style.MarginTop = Length.Pixels( 2 );

            Text = text;
            UpdateAction = action;
        }

        protected override void OnClick( MousePanelEvent e )
        {
            Checked = !Checked;
            Style.Opacity = Checked ? 0.95f : 0.75f;
            UpdateAction.Invoke(Checked);
        }
    }

	public class CustomButton : Label
    {
        Action UpdateAction;

        public CustomButton( string text, Action action)
        {
            Style.BackgroundColor = Color.Black;
            Style.Opacity = 0.95f;
            Style.Width = Length.Percent( 100 );
            Style.Padding = Length.Pixels( 8 );
            Style.MarginTop = Length.Pixels( 2 );

            Text = text;
            UpdateAction = action;
        }

        protected override void OnClick( MousePanelEvent e )
        {
            UpdateAction.Invoke();
        }

		protected override void OnMouseDown( MousePanelEvent e )
		{  
            Style.Opacity = 0.75f;
		}

		protected override void OnMouseUp( MousePanelEvent e )
		{  
            Style.Opacity = 0.95f;
		}
	}


}