using Grubs.Equipment.Weapons;
using Grubs.Pawn;
using Grubs.Terrain;
using Sandbox;

namespace Grubs;

[Title( "Grubs - Torch Weapon" ), Category( "Equipment" )]
public sealed class TorchWeapon : Weapon
{
	[Property] private float TorchTime { get; set; } = 5f;

	private float TorchTimeLeft { get; set; } = 5f;

	[Property] private float TorchSize { get; set; } = 10f;

	[Property] private GameObject TorchFlame { get; set; }

	protected override void OnStart()
	{
		base.OnStart();
		TorchTimeLeft = TorchTime;
	}

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		var pc = Equipment.Grub.PlayerController;
		var startPos = GetStartPosition();

		IsFiring = Input.Down("fire");

		TorchFlame.Enabled = IsFiring && Equipment.Deployed;
		TorchFlame.Transform.Position = GetStartPosition();
		TorchFlame.Transform.Rotation = Rotation.LookAt( pc.Facing * pc.EyeRotation.Forward );

		if ( IsFiring )
		{
			TorchTimeLeft -= Time.Delta;

			if ( TorchTimeLeft <= 0 )
			{
				TorchTimeLeft = TorchTime;
				IsFiring = false;
				TorchFlame.Enabled = false;
				FireFinished();
				return;
			}

			var endPos = startPos + pc.Facing * pc.EyeRotation.Forward * 2f;

			var tr = Scene.Trace.Ray( startPos, endPos )
					.WithAnyTags( "solid", "player" )
					.WithoutTags( "dead" )
					.IgnoreGameObjectHierarchy( Equipment.Grub.GameObject );

			using ( Rpc.FilterInclude( c => c.IsHost ) )
			{
				GrubsTerrain.Instance.SubtractLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ), TorchSize, 1 );
				GrubsTerrain.Instance.ScorchLine( new Vector2( startPos.x, startPos.z ),
					new Vector2( endPos.x, endPos.z ),
					TorchSize + 8f );
			}
		}
	}
}
