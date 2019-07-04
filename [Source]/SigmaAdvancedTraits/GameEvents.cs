using System.Linq;
using UnityEngine;


namespace SigmaAdvancedTraitsPlugin
{
    [KSPAddon(KSPAddon.Startup.Flight, false)]
    internal class FlightEvents : MonoBehaviour
    {
        void Start()
        {
            Debug.Log("FlightEvents", "Start");
            GameEvents.onVesselLoaded.Add(OnVesselLoaded);
            GameEvents.onVesselWasModified.Add(OnVesselWasModified);
            GameEvents.onVesselChange.Add(OnVesselChange);
        }

        void OnDestroy()
        {
            Debug.Log("FlightEvents", "OnDestroy");
            GameEvents.onVesselLoaded.Remove(OnVesselLoaded);
            GameEvents.onVesselWasModified.Remove(OnVesselWasModified);
            GameEvents.onVesselChange.Remove(OnVesselChange);
        }

        void OnVesselLoaded(Vessel vessel)
        {
            Debug.Log("FlightEvents.OnVesselLoaded", "vessel = " + vessel);
            AddColliders(vessel);
            FixPAWs(vessel);
        }

        void OnVesselWasModified(Vessel vessel)
        {
            Debug.Log("FlightEvents.OnVesselWasModified", "vessel = " + vessel);
            AddColliders(vessel);
            FixPAWs(vessel);
        }

        void OnVesselChange(Vessel vessel)
        {
            Debug.Log("FlightEvents.OnVesselChange", "vessel = " + vessel);
            FixPAWs(vessel);
        }

        void AddColliders(Vessel vessel)
        {
            Debug.Log("FlightEvents.AddCollidersToVessel", "Vessel = " + vessel);
            Part[] parts = vessel?.Parts?.Where(p => p?.GetComponent<ModuleDeployablePart>()?.deployState == ModuleDeployablePart.DeployState.BROKEN).ToArray();
            Debug.Log("FlightEvents.AddCollidersToVessel", "Found " + parts?.Length + " broken deployable parts.");
            for (int i = 0; i < parts?.Length; i++)
            {
                Debug.Log("FlightEvents.AddCollidersToVessel", "Part[" + i + "] = " + parts[i]);
                AddColliders(parts[i]);
            }
        }

        void AddColliders(Part part)
        {
            Debug.Log("FlightEvents.AddCollidersToPart", "Part = " + part);
            ModuleAdvancedRepair MAR = part?.GetComponent<ModuleAdvancedRepair>();
            Debug.Log("FlightEvents.AddCollidersToPart", "ModuleVesselRepair = " + MAR);

            if (MAR != null)
            {
                MeshFilter[] meshes = part?.GetComponentsInChildren<MeshFilter>()?.Where(mf => mf?.GetComponent<Collider>() == null)?.ToArray();
                Debug.Log("FlightEvents.AddCollidersToPart", "Found " + meshes?.Length + " objects without a collider.");
                for (int i = 0; i < meshes?.Length; i++)
                {
                    Debug.Log("FlightEvents.AddCollidersToPart", "GameObject[" + i + "] = " + meshes[i]?.gameObject);
                    BoxCollider bc = meshes[i].gameObject.AddOrGetComponent<BoxCollider>();
                    MAR.newCollidersList.Add(bc);
                    Debug.Log("FlightEvents.AddCollidersToPart", "BoxCollider = " + bc);
                }
            }
        }

        void FixPAWs(Vessel vessel)
        {
            Debug.Log("FlightEvents.FixPAWs", "Vessel = " + vessel);
            Part[] parts = vessel?.Parts?.Where(p => p?.GetComponent<ModuleVesselRepair>() != null).ToArray();
            Debug.Log("FlightEvents.FixPAWs", "Found " + parts?.Length + " parts with ModuleVesselRepair.");
            for (int i = 0; i < parts?.Length; i++)
            {
                Debug.Log("FlightEvents.FixPAWs", "Part[" + i + "] = " + parts[i]);
                FixPAW(parts[i]);
            }
        }

        void FixPAW(Part part)
        {
            Debug.Log("FlightEvents.FixPAW", "Part = " + part);

            ModuleAdvancedRepair MAR = part?.GetComponent<ModuleAdvancedRepair>();
            Debug.Log("FlightEvents.FixPAW", "ModuleAdvancedRepair = " + MAR);
            BaseEvent Repair = MAR?.Events?["Repair"];
            Debug.Log("FlightEvents.FixPAW", "Repair = " + Repair);
            BaseEvent Retract = MAR?.Events?["RetractManually"];
            Debug.Log("FlightEvents.FixPAW", "Retract = " + Retract);

            ModuleVesselRepair MVR = part.GetComponent<ModuleVesselRepair>();
            Debug.Log("FlightEvents.FixPAW", "ModuleVesselRepair = " + MVR);
            BaseEvent RepairAll = MVR?.Events?["RepairAll"];
            Debug.Log("FlightEvents.FixPAW", "RepairAll = " + RepairAll);

            if (Repair == null && RepairAll == null) return;

            Vessel activeVessel = FlightGlobals.ActiveVessel;
            Debug.Log("FlightEvents.FixPAW", "activeVessel = " + activeVessel);

            if (activeVessel != null)
            {
                Debug.Log("FlightEvents.FixPAW", "activeVessel.isEVA = " + activeVessel.isEVA);
                if (activeVessel.isEVA)
                {
                    ProtoCrewMember kerbal = activeVessel.Parts[0].protoModuleCrew[0];

                    Debug.Log("FlightEvents.FixPAW", "kerbal = " + kerbal);
                    if (kerbal != null)
                    {
                        Debug.Log("FlightEvents.FixPAW", "kerbal.experienceLevel = " + kerbal.experienceLevel);
                        if (kerbal.careerLog.HasEntry("Training1", AdvancedTraitsButtons.AdvTraits["Engineer"][0]))
                        {
                            Debug.Log("FlightEvents.FixPAW", kerbal.name + " is trained in Part Repair");
                            if (kerbal.experienceLevel >= 2)
                            {
                                Debug.Log("FlightEvents.FixPAW", "ModuleAdvancedRepair = " + MAR);
                                if (MAR != null)
                                {
                                    ModuleDeployablePart MDP = part.GetComponent<ModuleDeployablePart>();
                                    Debug.Log("FlightEvents.FixPAW", "ModuleDeployablePart = " + MDP);

                                    Repair.guiActive =
                                    Repair.guiActiveUnfocused =
                                    (
                                        (MDP is ModuleDeployableAntenna && kerbal.experienceLevel >= 2) ||
                                        (MDP is ModuleDeployableRadiator && kerbal.experienceLevel >= 3) ||
                                        (MDP is ModuleDeployableSolarPanel && kerbal.experienceLevel >= 4)
                                    );
                                    Debug.Log("FlightEvents.FixPAW", "Repair guiActive = " + Repair.guiActive);
                                    Debug.Log("FlightEvents.FixPAW", "Repair guiActive = " + Repair.guiActive);

                                    Retract.guiActive =
                                    Retract.guiActiveUnfocused =
                                    (
                                        Repair.guiActiveUnfocused == true &&
                                        MDP?.deployState == ModuleDeployablePart.DeployState.EXTENDED &&
                                        MDP?.retractable == false
                                    );
                                }

                                Debug.Log("FlightEvents.FixPAW", "ModuleVesselRepair = " + MVR);
                                if (MVR != null)
                                {
                                    RepairAll.guiActive =
                                    RepairAll.guiActiveUnfocused =
                                    kerbal.experienceLevel >= 5;
                                }

                                return;
                            }
                        }
                        else
                        {
                            Debug.Log("FlightEvents.FixPAW", kerbal.name + " is NOT trained in Part Repair");
                        }
                    }
                }
            }

            // Reset all to the default
            if (MAR != null)
            {
                Repair.guiActive =
                Repair.guiActiveUnfocused =
                Retract.guiActive =
                Retract.guiActiveUnfocused = false;
            }

            if (MVR != null)
            {
                RepairAll.guiActive =
                RepairAll.guiActiveUnfocused = false;
            }
        }
    }
}
