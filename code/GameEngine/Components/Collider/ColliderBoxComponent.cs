﻿using Sandbox;
using Sandbox.Diagnostics;

[Title( "Collider - Box" )]
[Category( "Physics" )]
[Icon( "check_box_outline_blank", "red", "white" )]
public class ColliderBoxComponent : ColliderBaseComponent
{
	[Property] public Vector3 Scale { get; set; } = 50;
	[Property] public Surface Surface { get; set; }

	public override void DrawGizmos()
	{
		if ( !Gizmo.IsSelected && !Gizmo.IsHovered )
			return;

		Gizmo.Draw.LineThickness = 1;
		Gizmo.Draw.Color = Gizmo.Colors.Green.WithAlpha( Gizmo.IsSelected ? 1.0f : 0.2f );
		Gizmo.Draw.LineBBox( new BBox( Scale * -0.5f, Scale * 0.5f ) );
	}

	protected override PhysicsShape CreatePhysicsShape( PhysicsBody targetBody )
	{
		var tx = targetBody.Transform.ToLocal( GameObject.WorldTransform );
		var shape = targetBody.AddBoxShape( tx.Position, tx.Rotation, Scale * 0.5f * tx.Scale );

		if ( Surface is not null )
		{
			shape.SurfaceMaterial = Surface.ResourceName;
		}

		return shape;
	}
}