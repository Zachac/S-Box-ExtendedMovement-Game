using Sandbox;
using Sandbox.UI;

public class UiCrossHair : Panel
{

    Label SpeedLabel;

    public UiCrossHair()
    {

        Style.Width = Length.ViewWidth( 100 );
        Style.Height = Length.ViewHeight( 100 );
        Style.Margin = Length.Pixels( 0 );
        Style.Padding = Length.Pixels( 0 );
        Style.FontColor = Color.White;
        Style.Display = DisplayMode.None;

        var panel = new Panel();
        panel.Parent = this;
        panel.Style.Display = DisplayMode.Flex;
        panel.Style.Width = Length.ViewWidth( 100 );
        panel.Style.Height = Length.ViewHeight( 100 );
        panel.Style.Margin = Length.Pixels( 0 );
        panel.Style.Padding = Length.Pixels( 0 );
        panel.Style.JustifyContent = Justify.Center;
        panel.Style.AlignItems = Align.Center;

        var label = new Label();
        label.Parent = panel;
        label.Style.FontSize = Length.Pixels( 60 );
        label.Style.MarginRight = Length.Pixels( 20 );
        label.Text = "（";


        label = new Label();
        label.Parent = panel;
        label.Style.FontSize = Length.Pixels( 60 );
        label.Style.MarginLeft = Length.Pixels( 20 );
        label.Text = $"）";

        label = new Label();
        label.Parent = this;
        label.Style.FontSize = Length.Pixels(30);
        label.Style.TextAlign = TextAlign.Left;
        label.Style.Position = PositionMode.Absolute;
        label.Style.PaddingLeft = Length.Pixels( 50 );
        label.Style.PaddingTop = Length.Pixels( 10 );
        label.Style.Left = Length.Percent( 50 );
        label.Style.Transform.GetValueOrDefault().AddTranslateX( Length.Percent( 50 ) );
        label.Style.Top = Length.Percent( 50 );
        label.Style.Transform.GetValueOrDefault().AddTranslateY( Length.Percent( 50 ) );
        label.Text = $"0";
        SpeedLabel = label;
    }

    public override void Tick()
    {
        if (PlayerInfo.IsFirstPerson()) {
            Style.Display = null;
            SpeedLabel.Text = $"{System.Math.Round( PlayerInfo.Velocity().WithZ( 0 ).Length / 100 ) * 10}";
        } else {
            Style.Display = DisplayMode.None;
        }
    }
}