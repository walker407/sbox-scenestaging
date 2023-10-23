using Sandbox;
using System.Linq;
using System.Runtime.CompilerServices;
using System;

namespace Wizards;

public partial class PickupComponent : BaseComponent
{
	public override void Update()
	{
		base.Update();

		var pickup = GameObject.Scene.GetAllObjects( true )
						.Where<GameObject>( x => x.GetComponent<ItemPickupComponent>() != null && (double)Vector3.DistanceBetween( Transform.Position, x.Transform.Position ) < 64.0f )
						.FirstOrDefault();
		
		if(pickup != null)
		{
			var itemPickup = pickup.GetComponent<ItemPickupComponent>();

			if(itemPickup.Available)
			{
				itemPickup.Pickup();
			}
		}
		
	}

}
