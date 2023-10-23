using Sandbox;
using System;

public sealed class BobAndSpinComponent : BaseComponent, BaseComponent.ExecuteInEditor
{
	[Property] public Angles SpinAngles { get; set; }
	[Property] public float Amplitude { get; set; }

	public override void Update()
	{
		Transform.LocalRotation *= (SpinAngles * RealTime.Delta).ToRotation();
		Transform.LocalPosition += new Vector3(0,0,((float)Math.Sin( Time.Now ) * Amplitude) );
	}
}
