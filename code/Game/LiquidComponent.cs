using Sandbox;
using System;

public sealed class LiquidComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property] public Vector3 WorldPosition { get; set; } = Vector3.Zero;
	[Property] public Vector3 FillPosition { get; set; } = Vector3.Zero;
	[Property] private float FillLevel { get; set; } = 0.5f;

	RenderAttributes attributes = new RenderAttributes();

	public override void Update()
	{
		WorldPosition = Transform.Position;
		FillPosition = WorldPosition - Transform.LocalPosition - new Vector3( 0, 0, FillLevel );
		
		attributes.Set( "g_vFillPositionWs", FillPosition );
		attributes.Set( "g_flFillLevel", FillLevel );

	
	}
}



