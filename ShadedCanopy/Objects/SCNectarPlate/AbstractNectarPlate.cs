using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShadedCanopy.Objects.SCNectarPlate
{
    public class AbstractNectarPlate : AbstractConsumable
    {
        public static readonly AbstractPhysicalObject.AbstractObjectType NectarPlate = new AbstractPhysicalObject.AbstractObjectType("NectarPlate");
        public bool dead = false;

        public AbstractNectarPlate(World world, AbstractObjectType type, PhysicalObject realizedObject, WorldCoordinate pos, EntityID ID, int originRoom, int consumableIndex, PlacedObject.ConsumableObjectData consumableData, bool dead = false) : base(world, NectarPlate, realizedObject, pos, ID, originRoom, consumableIndex, consumableData)
        {
            this.dead = dead;
            if (!dead && world.game.session is StoryGameSession && (world.game.session as StoryGameSession).saveState.ItemConsumed(world, false, originRoom, consumableIndex))
            {
                this.dead = true;
            }
        }

        public override void Realize()
        {
            if (this.realizedObject != null) return;
            this.realizedObject = new SCNectarPlate(this, UnityEngine.Random.Range(12, 16), 30f, 60f);
        }
    }
}
