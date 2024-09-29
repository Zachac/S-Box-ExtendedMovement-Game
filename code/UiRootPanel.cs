using Microsoft.VisualBasic;
using Sandbox;
using Sandbox.UI;

public sealed class UiRootPanel : PanelComponent
{
	UiCrossHair CrossHair;
	UiScoreBoard ScoreBoard;
	UiMenu Menu;

	protected override void OnTreeFirstBuilt()
	{
		base.OnTreeFirstBuilt();
		
		Panel.Style.Width = Length.ViewWidth(100);
		Panel.Style.Height = Length.ViewHeight(100);
		Panel.Style.Margin = Length.Pixels(0);
		Panel.Style.Padding = Length.Pixels(0);
		Panel.Style.FontSize = Length.Rem(1);

		CrossHair = new UiCrossHair();
		CrossHair.Parent = Panel;

		Menu = new UiMenu();
		Menu.Parent = Panel;
		
		ScoreBoard = new UiScoreBoard();
		ScoreBoard.Parent = Panel;

	}
}
