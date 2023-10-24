using Sandbox;
using System;

public sealed class LiquidComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property] public float WobbleX { get; set; } = 0;
	[Property] public float WobbleY { get; set; } = 0;

	[Property] public float BobTime { get; set; } = 0.75f;

	float bobAmountX;
	float bobAmountY;
	float bobAmountAddX;
	float bobAmountAddY;
	float pulse;

	Vector3 lastPos;
	Vector3 lastRot;
	Vector3 velocity;
	Vector3 angularVelocity;

	public override void Update()
	{
		if(GameObject.GetComponent<AnimatedModelComponent>().SceneObject is SceneObject model)
		{
			BobTime += Time.Delta;

			bobAmountAddX = MathX.Lerp( bobAmountAddX, 0, Time.Delta * 1 );
			bobAmountAddY = MathX.Lerp( bobAmountAddY, 0, Time.Delta * 1 );

			pulse = 2f * (float)Math.PI * 1f;

			bobAmountX = bobAmountAddX * (float)Math.Sin( pulse * BobTime );
			bobAmountY = bobAmountAddY * (float)Math.Sin( pulse * BobTime );

			model.Attributes.Set( "WobbleX", bobAmountX );
			model.Attributes.Set( "WobbleY", bobAmountY );

			velocity = (lastPos - Transform.Position) / Time.Delta;
			angularVelocity = Transform.Rotation.Angles().AsVector3() - lastRot;

			bobAmountAddX += MathX.Clamp( (velocity.x + (angularVelocity.y * 0.2f)) * 0.03f, -0.03f, 0.03f );
			bobAmountAddY += MathX.Clamp( (velocity.y + (angularVelocity.x * 0.2f)) * 0.03f, -0.03f, 0.03f );

			lastPos = Transform.Position;
			lastRot = Transform.Rotation.Angles().AsVector3();

		}
	}
}



