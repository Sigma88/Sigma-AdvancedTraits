namespace SigmaAdvancedTraitsPlugin
{
    internal class ModuleAdvancedTraining : PartModule
    {
        [KSPField(guiName = "<color=#FDD901><b>AdvancedTraining</b></color>", guiActive = false, guiActiveUnfocused = false, unfocusedRange = 2000)]
        string advancedTrainingLabel = "";

        public override void OnStart(StartState state)
        {
            Debug.Log("AdvancedTraining.OnStart", "vessel?.isEVA = " + vessel?.isEVA);
            if (vessel?.isEVA == true)
            {
                ProtoCrewMember kerbal = vessel?.Parts?[0]?.protoModuleCrew?[0];

                Debug.Log("AdvancedTraining.OnStart", "kerbal = " + kerbal);
                if (kerbal != null)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        Debug.Log("AdvancedTraining.OnStart", "kerbal.careerLog?.HasEntry(\"Training1\", AdvancedTraitsButtons.AdvTraits[\"" + kerbal.trait + "\"][" + i + "]) = " + kerbal.careerLog?.HasEntry("Training1", AdvancedTraitsButtons.AdvTraits[kerbal.trait][i]));
                        if (kerbal.careerLog?.HasEntry("Training1", AdvancedTraitsButtons.AdvTraits[kerbal.trait][i]) == true)
                        {
                            advancedTrainingLabel = AdvancedTraitsButtons.AdvTraits[kerbal.trait][i];
                            Debug.Log("AdvancedTraining.OnStart", "advancedTrainingLabel = " + advancedTrainingLabel);
                            Fields["advancedTrainingLabel"].guiActive =
                            Fields["advancedTrainingLabel"].guiActiveUnfocused = true;
                            return;
                        }
                    }
                }
            }
        }
    }
}
