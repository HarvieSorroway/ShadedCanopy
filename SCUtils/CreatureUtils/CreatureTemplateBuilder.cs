using System;
using System.Collections.Generic;
using System.Text;
using static AItile;

namespace SCUtils.CreatureUtils
{
    internal partial class CreatureTemplateBuilder
    {
        readonly CreatureTemplate.Type type;
        readonly CreatureTemplate? ancestor;

        string name;

        /// <summary> 生物的基础伤害抗性，默认为1防止忘记填写 </summary>
        float baseDamageResistance = 1f;
        float baseStunResistance;

        readonly float[,] damageRestistances = new float[ExtEnum<Creature.DamageType>.values.Count, 2];


        Dictionary<AItile.Accessibility, KeyValuePair<float, PathCost.Legality>> tileResistances = new();
        Dictionary<MovementConnection.MovementType, KeyValuePair<float, PathCost.Legality>> tileConnectionResistances = new();

        CreatureTemplate.Relationship defaultRelationShip = new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 1f);
        CreatureTemplate.Relationship[] relationShips = new CreatureTemplate.Relationship[ExtEnum<CreatureTemplate.Type>.values.Count];

        public CreatureTemplate BuildTemplate()
        {
            List<TileTypeResistance> tileResistances = new List<TileTypeResistance>();
            foreach(var pair in this.tileResistances)
                tileResistances.Add(new TileTypeResistance(pair.Key, pair.Value.Key, pair.Value.Value));

            List<TileConnectionResistance> connectionResistances = new List<TileConnectionResistance>();
            foreach (var pair in tileConnectionResistances)
                connectionResistances.Add(new TileConnectionResistance(pair.Key, pair.Value.Key, pair.Value.Value));

            CreatureTemplate creatureTemplate = new(type, ancestor, tileResistances, connectionResistances, defaultRelationShip);
            for(int i = 0;i < relationShips.Length; i++)
            {
                if (relationShips[i] != default)
                {
                    creatureTemplate.relationships[i] = relationShips[i];
                }
            }
            return creatureTemplate;
        }
    }

    internal partial class CreatureTemplateBuilder
    {

        /// <param name="type">该生物模板对应的生物类型</param>
        /// <param name="ancestor">该生物模板的继承对象，留空则不继承</param>
        public CreatureTemplateBuilder(CreatureTemplate.Type type, CreatureTemplate.Type ancestor = null)
        {
            this.type = type;
            this.ancestor = StaticWorld.GetCreatureTemplate(ancestor);
        }

        public CreatureTemplateBuilder SetName(string name) { this.name = name; return this; }

        /// <summary>
        /// 设置生物的伤害和眩晕抗性
        /// </summary>
        /// <param name="damageType">伤害类型，留空则设置<see cref="CreatureTemplateBuilder.baseDamageResistance"/></param>
        /// <param name="damageResistance">伤害抗性值</param>
        /// <param name="stunResistance">眩晕抗性值</param>
        /// <exception cref="ArgumentException">当设置<see cref="CreatureTemplateBuilder.baseDamageResistance"/>为0时会有此报错，该值不应当设置为0 </exception>
        public CreatureTemplateBuilder SetResistance(Creature.DamageType? damageType, float damageResistance, float stunResistance)
        {
            if (damageType == null)
            {
                if (damageResistance == 0f)
                    throw new ArgumentException("baseDamageResistance can not be ZERO");
                baseDamageResistance = damageResistance;
                baseStunResistance = stunResistance;
            }
            else
            {
                damageRestistances[damageType.Index, 0] = damageResistance;
                damageRestistances[damageType.Index, 1] = stunResistance;
            }
            return this;
        }


        /// <summary> 用于配置<see cref="CreatureTemplate.pathingPreferencesTiles"/> </summary>
        public CreatureTemplateBuilder SetTileResistance(AItile.Accessibility accessibility, float resistance, PathCost.Legality legality)
        {
            if (tileResistances.ContainsKey(accessibility))
                tileResistances[accessibility] = new KeyValuePair<float, PathCost.Legality>(resistance, legality);
            else
                tileResistances.Add(accessibility, new KeyValuePair<float, PathCost.Legality>(resistance, legality));

            return this;
        }

        /// <summary> 用于配置<see cref="CreatureTemplate.pathingPreferencesConnections"/> </summary>
        public CreatureTemplateBuilder SetTileConnectionResistance(MovementConnection.MovementType movementType, float resistance, PathCost.Legality legality)
        {
            if (tileConnectionResistances.ContainsKey(movementType))
                tileConnectionResistances[movementType] = new KeyValuePair<float, PathCost.Legality>(resistance, legality);
            else
                tileConnectionResistances.Add(movementType, new KeyValuePair<float, PathCost.Legality>(resistance, legality));
            return this;
        }

        /// <summary> 用于配置<see cref="CreatureTemplate.relationships"/> </summary>
        /// <param name="type">设置的生物对象类型，留空则设置<see cref="CreatureTemplate.defaultRelationShip"/></param>
        public CreatureTemplateBuilder SetCreatureRelationShip(CreatureTemplate.Type type, CreatureTemplate.Relationship.Type relationship, float intensity)
        {
            if(type == null)
                defaultRelationShip = new CreatureTemplate.Relationship(relationship, intensity);
            else
                relationShips[type.Index] = new CreatureTemplate.Relationship(relationship, intensity);
            return this;
        }
    }
}
