namespace TerryForm
{
	public partial class Game : Sandbox.Game
	{
		public static Game Instance
		{
			get => Current as Game;
		}

		public Game()
		{

		}
	}
}
