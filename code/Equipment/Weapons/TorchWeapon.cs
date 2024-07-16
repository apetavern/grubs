using Grubs.Common;
using Grubs.Equipment.Weapons;
using Grubs.Terrain;

namespace Grubs;

[Title( "Grubs - Torch Weapon" ), Category( "Equipment" )]
public sealed class TorchWeapon : Weapon
{
	[Property] private float TorchSize { get; set; } = 10f;
	[Property] private GameObject TorchFlame { get; set; }

	[Sync] private bool TorchFlameEnabled { get; set; }

	private TimeSince _timeSinceLastTorchSound;

	protected override void OnUpdate()
	{
		var pc = Equipment.Grub?.PlayerController;

		if ( TorchFlame is not null )
		{
			TorchFlame.Enabled = TorchFlameEnabled;

			if ( pc is not null )
			{
				TorchFlame.Transform.Position = GetStartPosition();
				TorchFlame.Transform.Rotation = Rotation.LookAt( pc.Facing * pc.EyeRotation.Forward );
			}
		}

		base.OnUpdate();
	}

	protected override void HandleComplexFiringInput()
	{
		base.HandleComplexFiringInput();

		var pc = Equipment.Grub.PlayerController;
		var startPos = GetStartPosition();

		IsFiring = Input.Down( "fire" );

		if ( !IsProxy )
			TorchFlameEnabled = IsFiring && Equipment.Deployed;

		if ( IsFiring )
		{
			TimesUsed += Time.Delta * (Input.Pressed( "fire" ) ? StartMultiplier : 1f);

			if ( TimesUsed >= MaxUses )
			{
				TorchFlame.Enabled = false;
				FireFinished();
				return;
			}

			var endPos = startPos + pc.Facing * pc.EyeRotation.Forward * 2f;

			if ( _timeSinceLastTorchSound > 0.2f )
			{
				FireEffects( startPos, endPos );
				_timeSinceLastTorchSound = 0f;
			}

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

	[Broadcast]
	public void FireEffects( Vector3 startPos, Vector3 endPos )
	{
		if ( Equipment is not null && Equipment.Grub is not null )
			Sound.Play( UseSound, Equipment.Grub.Transform.Position );

		var tr = Scene.Trace.Ray( startPos, endPos )
						.WithAnyTags( "solid", "player", "pickup" )
						.WithoutTags( "dead" )
						.IgnoreGameObjectHierarchy( Equipment.Grub.GameObject )
						.Run();

		if ( !tr.Hit )
			return;

		if ( tr.GameObject.Components.TryGet<Health>( out var health, FindMode.EverythingInSelfAndAncestors ) )
		{
			health.TakeDamage( GrubsDamageInfo.FromFire( 1, Equipment.Grub.Id, Equipment.Grub.Name, tr.HitPosition ) );
		}
	}

	protected override void FireFinished()
	{
		TorchFlameEnabled = false;

		base.FireFinished();
	}

	public override void OnHolster()
	{
		TorchFlameEnabled = false;

		base.OnHolster();
	}
}
