using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.UI;

public class UiScoreBoard : Panel
{

    Panel ScoreBoardPanel;

    public UiScoreBoard()
    {

        Style.Width = Length.ViewWidth( 100 );
        Style.Height = Length.ViewHeight( 100 );
        Style.Margin = Length.Pixels( 0 );
        Style.Padding = Length.Pixels( 0 );
        Style.FontColor = Color.White;
        Style.Position = PositionMode.Absolute;
        Style.Display = DisplayMode.None;

        var panel = new Panel();
        panel.Parent = this;
        panel.Style.Display = DisplayMode.Flex;
        panel.Style.Width = Length.ViewWidth( 100 );
        panel.Style.Height = Length.ViewHeight( 75 );
        panel.Style.Margin = Length.Pixels( 0 );
        panel.Style.Padding = Length.Pixels( 0 );
        panel.Style.PaddingTop = Length.ViewHeight( 25 );
        panel.Style.JustifyContent = Justify.FlexStart;
        panel.Style.AlignItems = Align.Center;
        panel.Style.FlexDirection = FlexDirection.Column;
        panel.Style.Position = PositionMode.Absolute;
        ScoreBoardPanel = panel;

    }

    public override void Tick()
    {
        if ( Input.Pressed( "Score" ) )
        {

            ScoreBoardPanel.DeleteChildren();

            var headerPanel = new Panel();
            headerPanel.Style.FontSize = Length.Pixels( 25 );
            headerPanel.Style.FontColor = Color.White;
            headerPanel.Style.BackgroundColor = new Color( 0, 0, 0, 1 );
            headerPanel.Style.Opacity = 0.7f;
            headerPanel.Style.Display = DisplayMode.Flex;
            headerPanel.Style.JustifyContent = Justify.SpaceBetween;
            headerPanel.Style.Width = Length.ViewWidth( 50 );
            headerPanel.Parent = ScoreBoardPanel;
            headerPanel.Style.PaddingLeft = Length.Pixels( 50 );
            headerPanel.Style.PaddingRight = Length.Pixels( 50 );
            headerPanel.Style.MarginTop = Length.Pixels( 8 );
            headerPanel.Style.PaddingTop = Length.Pixels( 3 );
            headerPanel.Style.PaddingBottom = Length.Pixels( 3 );

            var label4 = new Label();
            label4.Parent = headerPanel;
            label4.Text = "Name";
            label4.Style.Opacity = 1.0f;
            label4.Style.PaddingLeft = Length.Pixels( 50 );

            var label5 = new Label();
            label5.Style.PaddingRight = Length.Pixels( 50 );
            label5.Text = "Ping";
            label5.Parent = headerPanel;

            foreach ( var c in Connection.All )
            {

                var namePanel = new Panel();
                namePanel.Style.FontSize = Length.Pixels( 25 );
                namePanel.Style.FontColor = Color.White;
                namePanel.Style.BackgroundColor = new Color( 0, 0, 0, 1 );
                namePanel.Style.Opacity = 0.7f;
                namePanel.Style.Display = DisplayMode.Flex;
                namePanel.Style.JustifyContent = Justify.SpaceBetween;
                namePanel.Style.Width = Length.ViewWidth( 50 );
                namePanel.Parent = ScoreBoardPanel;
                namePanel.Style.PaddingLeft = Length.Pixels( 50 );
                namePanel.Style.PaddingRight = Length.Pixels( 50 );
                namePanel.Style.MarginTop = Length.Pixels( 4 );
                namePanel.Style.PaddingTop = Length.Pixels( 3 );
                namePanel.Style.PaddingBottom = Length.Pixels( 3 );

                var label2 = new Label();
                label2.Parent = namePanel;
                label2.Text = c.DisplayName;
                label2.Style.Opacity = 1.0f;
                label2.Style.PaddingLeft = Length.Pixels( 50 );

                var label3 = new Label();
                label3.Style.PaddingRight = Length.Pixels( 50 );
                label3.Text = $"{System.Math.Round( c.Ping )}";
                label3.Parent = namePanel;
            }

            Style.Display = null;
        }
        else if ( Input.Released( "Score" ) )
        {
            Style.Display = DisplayMode.None;
        }
    }

    public class PlayerSpeedText : Label
    {

        Connection Con;

        public PlayerSpeedText( Connection c )
        {
            Con = c;
        }

        public override void Tick()
        {
            Text = $"{System.Math.Round( Con.Ping )}";
        }

    }
}