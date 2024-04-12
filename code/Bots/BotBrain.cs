using Grubs.Pawn.Controller;
using Sandbox;
using System.Threading.Tasks;

namespace Grubs.Bots;

public partial class BotBrain : Component
{
	[Property] public Func<Task>? BrainAction { get; set; }

	[Property] public GameObject? ActiveGrub { get; set; }

	protected override void OnStart()
	{
		//ActiveGrub = Scene.GetAllComponents<GrubPlayerController>().First().GameObject;
		//BrainAction.Invoke();
	}
}
