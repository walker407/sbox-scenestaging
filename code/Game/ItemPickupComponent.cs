using Sandbox;

namespace Wizards;

public partial class ItemPickupComponent : BaseComponent
{
	[Property] public float RespawnTime { get; set; } = 5f;

	private TimeSince TimeSinceRespawned { get; set; }

	public bool Available { get; private set; } = true;

	public override void Update()
	{
		base.Update();

		if(TimeSinceRespawned > RespawnTime)
		{
			Available = true;
		}
		else if(Available == false)
		{
			//Log.Info( "Respawning..." );
		}

		

		if( GameObject.GetComponent<AnimatedModelComponent>().SceneObject is SceneObject model)
		{
			model.RenderingEnabled = Available;
		}
	}

	public void Pickup()
	{
		if ( Available )
		{
			//Log.Info( "Picking up" );
			Available = false;
			TimeSinceRespawned = 0;
		} 
	}
}
