using System.Collections.Generic;


namespace SigmaAdvancedTraitsPlugin
{
    internal class AutoPilot : PartModule
    {
        // Features
        [UI_Toggle(disabledText = "<color=#FF0000>Disabled</color>", enabledText = "<color=#00FF00>Enabled</color>")]
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "<color=#FF5555>TOGGLE DEBUG</color>")]
        bool debug = false;

        [UI_Toggle(disabledText = "<color=#FF0000>Disabled</color>", enabledText = "<color=#00FF00>Enabled</color>")]
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "<color=#FF5555>Debug: Track</color>")]
        bool track = false;

        [UI_Toggle(disabledText = "<color=#FF0000>Disabled</color>", enabledText = "<color=#00FF00>Enabled</color>")]
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "<color=#FF5555>Debug: Precise</color>")]
        bool precise = false;

        [UI_Toggle(disabledText = "<color=#FF0000>Disabled</color>", enabledText = "<color=#00FF00>Enabled</color>")]
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "<color=#FF5555>Debug: Multinode</color>")]
        bool multinode = false;


        // KSP Fields
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Pilot")]
        public string pilotName = "";

        [UI_Toggle(disabledText = "<color=#FF0000>Disabled</color>", enabledText = "<color=#00FF00>Enabled</color>")]
        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "<color=#BADA55>Execute Maneuver</color>")]
        public bool autoPilot = false;

        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Ignition")]
        [UI_FloatRange(minValue = 0, maxValue = 1, stepIncrement = 0.1f)]
        public float ignition = 0;

        [KSPField(isPersistant = false, guiActive = false, guiActiveEditor = false, guiName = "Cut off")]
        [UI_FloatRange(minValue = 0, maxValue = 1, stepIncrement = 0.1f)]
        public float cutOff = 0;


        // Parameters
        bool active = false;
        bool burn = false;
        bool slow = false;
        double throttle = 0;
        double UT = 0;
        double dV = 0;
        ProtoCrewMember pilot = null;


        // Methods
        void Reset()
        {
            Debug.Log("AutoPilot.Reset", "Start");
            if (track)
            {
                vessel.Autopilot.Enable(VesselAutopilot.AutopilotMode.StabilityAssist);
            }
            active = false;
            burn = false;
            slow = false;
            throttle = 0;
            dV = 0;
            UT = 0;
            pilot = null;
            FlightInputHandler.state.mainThrottle = 0;
            Debug.Log("AutoPilot.Reset", "End");
        }

        void CheckPilot()
        {
            Debug.Log("AutoPilot.CheckPilot", "debug = " + debug);

            if (debug)
            {
                Fields["autoPilot"].guiActive = Fields["pilotName"].guiActive = true;
                pilotName = "<color=#FF0000>DEBUG</color>";

                if (!(vessel?.patchedConicSolver?.maneuverNodes?.Count > 0))
                {
                    autoPilot = false;
                    Debug.Log("AutoPilot.CheckPilot", "No maneuver nodes, turn off autopilot");
                }

                return;
            }

            pilot = null;

            Debug.Log("AutoPilot.CheckPilot", "vessel = " + vessel + ", activeVessel = " + FlightGlobals.ActiveVessel);
            if (vessel == FlightGlobals.ActiveVessel)
            {
                Debug.Log("AutoPilot.CheckPilot", "maneuver nodes count = " + vessel?.patchedConicSolver?.maneuverNodes?.Count);
                if (vessel?.patchedConicSolver?.maneuverNodes?.Count > 0)
                {
                    List<ProtoCrewMember> crew = part?.protoModuleCrew;

                    Debug.Log("AutoPilot.CheckPilot", "crew count = " + crew?.Count);
                    if (crew != null)
                    {
                        int n = crew.Count;

                        for (int i = 0; i < n; i++)
                        {
                            Debug.Log("AutoPilot.CheckPilot", "crew[" + i + "] = " + crew[i]);
                            if (crew[i].careerLog.HasEntry("AdvancedTrait_ManualPilot"))
                            {
                                Fields["autoPilot"].guiActive = Fields["pilotName"].guiActive = true;

                                Debug.Log("AutoPilot.CheckPilot", "crew[" + i + "] level = " + crew[i].experienceLevel);
                                if (pilot == null || pilot.experienceLevel < crew[i].experienceLevel)
                                {
                                    pilot = crew[i];
                                    pilotName = pilot.name;

                                    if (pilot.experienceLevel == 5)
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            Debug.Log("AutoPilot.CheckPilot", "pilot = " + pilot);
            if (pilot == null)
            {
                Fields["autoPilot"].guiActive = Fields["pilotName"].guiActive = false;
                autoPilot = false;
                Debug.Log("AutoPilot.CheckPilot", "No valid pilots, turn off autopilot");
            }
        }

        void CheckFeatures()
        {
            Debug.Log("AutoPilot.CheckFeatures", "debug = " + debug);
            if (!debug)
            {
                Debug.Log("AutoPilot.CheckFeatures", "pilot level = " + pilot?.experienceLevel);
                track = pilot?.experienceLevel > 2;
                precise = pilot?.experienceLevel > 3;
                multinode = pilot?.experienceLevel > 4;
            }

            Debug.Log("AutoPilot.CheckFeatures", "precise = " + precise);
            if (precise)
            {
                Fields["ignition"].guiActive = Fields["cutOff"].guiActive = true;
            }
            else
            {
                ignition = 0;
                cutOff = 0;
                Fields["ignition"].guiActive = Fields["cutOff"].guiActive = false;
            }

            Fields["track"].guiActive = Fields["precise"].guiActive = Fields["multinode"].guiActive = debug;
        }

        public override void OnStart(StartState state)
        {
            Debug.Log("AutoPilot.OnStart", "Debug.debug = " + Debug.debug);
            Fields["debug"].guiActive = debug = Debug.debug;

            base.OnStart(state);
        }

        public override void OnUpdate()
        {
            CheckPilot();
            CheckFeatures();

            Debug.Log("AutoPilot.OnUpdate", "autoPilot = " + autoPilot);
            if (autoPilot)
            {
                Debug.Log("AutoPilot.OnUpdate", "active = " + active);
                if (!active)
                {
                    active = true;

                    Debug.Log("AutoPilot.OnUpdate", "track = " + track);
                    if (track)
                    {
                        vessel.ActionGroups.SetGroup(KSPActionGroup.SAS, true);
                        vessel.Autopilot.Enable(VesselAutopilot.AutopilotMode.Maneuver);
                    }
                }
                else
                {
                    var maneuver = vessel.patchedConicSolver.maneuverNodes[0];

                    Debug.Log("AutoPilot.OnUpdate", "burn = " + burn + ", startBurnIn = " + maneuver?.startBurnIn + ", ignition = " + ignition);
                    if (!burn && maneuver.startBurnIn < ignition)
                    {
                        burn = true;
                        dV = maneuver.GetPartialDv().magnitude;
                        UT = (float)Planetarium.GetUniversalTime();
                    }

                    Debug.Log("AutoPilot.OnUpdate", "burn = " + burn + ", dV = " + dV + ", UT = " + UT);
                    if (burn)
                    {
                        double dT = Planetarium.GetUniversalTime() - UT;

                        Debug.Log("AutoPilot.OnUpdate", "old UT = " + UT);
                        Debug.Log("AutoPilot.OnUpdate", "new UT = " + (UT + dT));
                        Debug.Log("AutoPilot.OnUpdate", "dT = " + dT);
                        if (dT > 0)
                        {
                            double ddV = maneuver.GetPartialDv().magnitude - dV;

                            Debug.Log("AutoPilot.OnUpdate", "old dV = " + dV);
                            Debug.Log("AutoPilot.OnUpdate", "new dV = " + (dV + ddV));
                            Debug.Log("AutoPilot.OnUpdate", "ddV = " + ddV);
                            if (ddV > 1e-9)
                            {
                                Debug.Log("AutoPilot.OnUpdate", "ddV increased, current maneuver node is completed.");
                                Debug.Log("AutoPilot.OnUpdate", "multinode = " + multinode + ", maneuver nodes count = " + vessel?.patchedConicSolver?.maneuverNodes?.Count);
                                if (multinode && vessel.patchedConicSolver.maneuverNodes.Count > 1)
                                {
                                    vessel.patchedConicSolver.RemoveManeuverNode(maneuver);
                                    Debug.Log("AutoPilot.OnUpdate", "Removed maneuver node.");
                                }
                                else
                                {
                                    autoPilot = false;
                                    Debug.Log("AutoPilot.OnUpdate", "Last maneuver node, turn off autopilot.");
                                }
                                Reset();
                                return;
                            }

                            dV += ddV;
                            UT += dT;

                            Debug.Log("AutoPilot.OnUpdate", "slow = " + slow);
                            if (!slow)
                            {
                                Debug.Log("AutoPilot.OnUpdate", "ddV = " + ddV + ", [(dV + ddV) / -ddV * dT] = " + ((dV + ddV) / -ddV * dT) + ", cutOff = " + cutOff + ", throttle = " + throttle);
                                if (ddV < 0 && (dV + ddV) / -ddV * dT < cutOff)
                                {
                                    slow = true;
                                    Debug.Log("AutoPilot.OnUpdate", "Start slowing down.");
                                }

                                else

                                if (throttle < 1)
                                {
                                    Debug.Log("AutoPilot.OnUpdate", "Increase throttle by " + (dT / ignition));
                                    throttle += dT / ignition;
                                    if (throttle > 1)
                                        throttle = 1;
                                    Debug.Log("AutoPilot.OnUpdate", "new throttle = " + throttle);
                                }
                            }

                            Debug.Log("AutoPilot.OnUpdate", "precise = " + precise);
                            if (precise)
                            {
                                Debug.Log("AutoPilot.OnUpdate", "slow = " + slow + ", throttle = " + throttle);
                                if (slow && throttle > 0.05)
                                {
                                    Debug.Log("AutoPilot.OnUpdate", "Decrease throttle by " + (dT / (cutOff * 2)));
                                    throttle -= dT / (cutOff * 2);
                                    if (throttle < 0.05)
                                        throttle = 0.05;
                                    Debug.Log("AutoPilot.OnUpdate", "new throttle = " + throttle);
                                }
                            }

                            FlightInputHandler.state.mainThrottle = (float)throttle;
                        }
                    }
                }
            }
            else
            {
                Debug.Log("AutoPilot.OnUpdate", "active = " + active);
                if (active)
                {
                    Reset();
                }
            }
        }
    }
}
