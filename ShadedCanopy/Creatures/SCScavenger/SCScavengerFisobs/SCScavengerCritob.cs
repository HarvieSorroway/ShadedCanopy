using Fisobs.Core;
using Fisobs.Creatures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace ShadedCanopy.Creatures.Scintivenger.ScintivengerCritobs
{
    internal class SCScavengerCritob : Critob
    {
        public SCScavengerCritob() : base(SCEnums.CreatureTemplateType.SCScavenger)
        {
            LoadedPerformanceCost = 20f;
            SandboxPerformanceCost = new(linear: 0.6f, exponential: 0.1f);
            ShelterDanger = ShelterDanger.Safe;
            CreatureName = "SCScavenger";
        }

        public override AbstractCreatureAI CreateAbstractAI(AbstractCreature acrit)
        {
            return new ScavengerAbstractAI(acrit.world, acrit);
        }
        public override ArtificialIntelligence CreateRealizedAI(AbstractCreature acrit)
        {
            return new ScavengerAI(acrit, acrit.world);  
        }

        public override Creature CreateRealizedCreature(AbstractCreature acrit)
        {
            return new Scavenger(acrit, acrit.world);
        }

        public override CreatureTemplate CreateTemplate()
        {
            var scavengerTemplate = StaticWorld.GetCreatureTemplate(CreatureTemplate.Type.Scavenger);
            var template = new CreatureTemplate(SCEnums.CreatureTemplateType.SCScavenger, scavengerTemplate, new List<TileTypeResistance>(), new List<TileConnectionResistance>(), new CreatureTemplate.Relationship(CreatureTemplate.Relationship.Type.Ignores, 0.1f));
            template.BlizzardWanderer = true;
            template.baseDamageResistance = 2.2f;
            template.baseStunResistance = 1.3f;
            template.instantDeathDamageLimit = 3.5f;
            template.offScreenSpeed = 0.75f;
            template.grasps = 5;
            template.AI = true;
            template.requireAImap = true;
            template.abstractedLaziness = 50;
            template.bodySize = 1.2f;
            template.doPreBakedPathing = false;
            template.preBakedPathingAncestor = scavengerTemplate;
            template.stowFoodInDen = false;
            template.shortcutSegments = 2;
            template.visualRadius = 1200f;
            template.movementBasedVision = 0.3f;
            template.canSwim = true;
            template.waterRelationship = CreatureTemplate.WaterRelationship.AirAndSurface;
            template.hibernateOffScreen = true;
            template.roamBetweenRoomsChance = -1f;
            template.roamInRoomChance = -1f;
            template.socialMemory = true;
            template.communityID = CreatureCommunities.CommunityID.Scavengers;
            template.communityInfluence = 1f;
            template.dangerousToPlayer = 0.5f;
            template.meatPoints = 4;
            template.usesNPCTransportation = true;
            template.usesRegionTransportation = true;
            template.usesCreatureHoles = false;
            template.jumpAction = "Jump";
            template.pickupAction = "Pick Up";
            template.throwAction = "Throw";

            return template;
        }

        public override void EstablishRelationships()
        {
            //todo
        }

        public override string DevtoolsMapName(AbstractCreature acrit)
        {
            return "Sc(SC)";
        }

        public override Color DevtoolsMapColor(AbstractCreature acrit)
        {
            return new Color(0.32f, 0.14f, 0.99f);

        }

        public override void LoadResources(RainWorld rainWorld)
        {
            string spriteName = "illustrations/Icons/Kill_Scintivenger";
            var atlas = Futile.atlasManager.LoadImage(spriteName);

            Icon = new SimpleIcon(atlas.elements[0].name, Ext.MenuGrey);
        }

    }
}
