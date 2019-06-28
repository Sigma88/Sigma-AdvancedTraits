using System.Collections.Generic;
using CommNet;


namespace SigmaAdvancedTraitsPlugin
{
    internal class ModuleKerbalCommand : ModuleCommand
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["commNetFirstHopDistance"].guiActive =
            Fields["commNetSignal"].guiActive =
            Fields["controlSrcStatusText"].guiActive =
            Fields["hibernateOnWarp"].guiActive =
            Fields["hibernation"].guiActive =
            Events["ChangeControlPoint"].guiActive =
            Events["MakeReference"].guiActive =
            Events["RenameVessel"].guiActive = false;
        }
    }

    internal class ModuleKerbalAntenna : ModuleDataTransmitter
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            Fields["statusText"].guiActive =
            Fields["powerText"].guiActive =
            Events["StartTransmission"].guiActive =
            Events["TransmitIncompleteToggle"].guiActive =
            Events["StopTransmission"].guiActive = false;

            if (vessel?.isEVA == true)
            {
                List<ProtoCrewMember> crew = part?.protoModuleCrew;

                if (crew?.Count == 1)
                {
                    ProtoCrewMember kerbal = crew[0];

                    if (kerbal.careerLog.HasEntry("Training1", AdvancedTraitsButtons.AdvTraits["Pilot"][1]))
                    {
                        if (kerbal.experienceLevel == 2)
                        {
                            antennaPower = 1;
                            upgradesApplied = new List<string> { "Level2" };
                        }

                        else

                        if (kerbal.experienceLevel == 3)
                        {
                            antennaPower = 100;
                            upgradesApplied = new List<string> { "Level3" };
                        }

                        else

                        if (kerbal.experienceLevel > 3)
                        {
                            antennaPower = 10000;
                            upgradesApplied = new List<string> { "Level4" };
                        }
                    }
                }
            }
        }

        public override void ApplyUpgradeNode(List<string> appliedUps, ConfigNode node, bool doLoad)
        {
            if (appliedUps?.Count > 0)
            {
                string upgrade = appliedUps[0];

                if (upgrade == "Level2")
                {
                    node.AddValue("antennaPower", "1");
                }

                else

                if (upgrade == "Level3")
                {
                    node.AddValue("antennaPower", "100");
                }

                else

                if (upgrade == "Level4")
                {
                    node.AddValue("antennaPower", "10000");
                }
            }
        }
    }

    internal class ModuleKerbalRemote : ModuleProbeControlPoint
    {
        public override void OnStart(StartState state)
        {
            base.OnStart(state);

            if (vessel?.isEVA == true)
            {
                List<ProtoCrewMember> crew = part?.protoModuleCrew;

                if (crew?.Count == 1)
                {
                    ProtoCrewMember kerbal = crew[0];

                    if (kerbal.careerLog.HasEntry("Training1", AdvancedTraitsButtons.AdvTraits["Pilot"][1]))
                    {
                        multiHop = kerbal.experienceLevel < 5;

                        if (multiHop)
                        {
                            upgradesApplied.Add("Level5");
                        }
                    }
                }
            }
        }

        public override void ApplyUpgradeNode(List<string> appliedUps, ConfigNode node, bool doLoad)
        {
            if (appliedUps?.Count > 0)
            {
                string upgrade = appliedUps[0];

                if (upgrade == "Level5")
                {
                    node.AddValue("multiHop", "true");
                }
            }
        }
    }
}
