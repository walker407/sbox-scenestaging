using Sandbox;
using System;

public sealed class LiquidComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property] public float WobbleX { get; set; } = 0;
	[Property] public float WobbleY { get; set; } = 0;
	[Property] public float MaxWobble { get; set; } = 0.08f;

	float BobTime { get; set; } = 0.75f;

	float WobbleAmountAddX;
	float WobbleAmountAddY;

	Vector3 LastPosition;
	Vector3 LastRotation;

	public override void Update()
	{
		base.Update();

		if(GameObject.GetComponent<AnimatedModelComponent>().SceneObject is SceneObject model)
		{
			BobTime += Time.Delta;

			WobbleAmountAddX = MathX.Lerp( WobbleAmountAddX, 0, Time.Delta * 1 );
			WobbleAmountAddY = MathX.Lerp( WobbleAmountAddY, 0, Time.Delta * 1 );

			var pulse = 2f * (float)Math.PI * 1f;

			var wobbleAmountX = WobbleAmountAddX * (float)Math.Sin( pulse * BobTime );
			var wobbleAmountY = WobbleAmountAddY * (float)Math.Sin( pulse * BobTime );

			model.Attributes.Set( "WobbleX", wobbleAmountX );
			model.Attributes.Set( "WobbleY", wobbleAmountY );

			var velocity = (LastPosition - Transform.Position) / Time.Delta;
			var angularVelocity = Transform.Rotation.Angles().AsVector3() - LastRotation;

			WobbleAmountAddX += MathX.Clamp( (velocity.y + (angularVelocity.x * 1)) * MaxWobble, -MaxWobble, MaxWobble );
			WobbleAmountAddY += MathX.Clamp( (velocity.x + (angularVelocity.y * 1)) * MaxWobble, -MaxWobble, MaxWobble );

			LastPosition = Transform.Position;
			LastRotation = Transform.Rotation.Angles().AsVector3();

		}
	}
}



