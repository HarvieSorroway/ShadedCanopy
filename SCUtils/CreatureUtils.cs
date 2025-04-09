using SCUtils.CreatureUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SCUtils
{
    public static class Test
    {
        static CreatureTemplate.Type FlashFalcon = new CreatureTemplate.Type("FlashFalcon", true);
        public static void TestFunc()
        {
            CreatureTemplateBuilder creatureTemplateBuilder = new CreatureTemplateBuilder(FlashFalcon, null);

            var creatureTemplate =
            creatureTemplateBuilder
                .SetTileResistance(AItile.Accessibility.Floor, 1f, PathCost.Legality.Unwanted)
                .SetTileResistance(AItile.Accessibility.Wall, 100f, PathCost.Legality.IllegalTile)

                .SetTileConnectionResistance(MovementConnection.MovementType.ReachDown, 20f, PathCost.Legality.Unwanted)

                .SetResistance(null, 20f, 5f)
                .SetResistance(Creature.DamageType.Explosion, 4f, 5f)

                .SetCreatureRelationShip(null, CreatureTemplate.Relationship.Type.Attacks, 1f)
                .SetCreatureRelationShip(CreatureTemplate.Type.LanternMouse, CreatureTemplate.Relationship.Type.Ignores, 1f)

                .BuildTemplate();
        }
    }
}
