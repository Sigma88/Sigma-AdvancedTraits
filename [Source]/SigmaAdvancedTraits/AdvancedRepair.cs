using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
using ModuleWheels;


namespace SigmaAdvancedTraitsPlugin
{
    internal class ModuleVesselRepair : PartModule
    {
        [KSPEvent(guiActiveUnfocused = false, guiActive = false, guiActiveEditor = false, guiName = "Repair Vessel")]
        public void RepairAll()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                Debug.Log("RepairAll", "Vessel =  " + vessel);
                if (vessel != null)
                {
                    ModuleAdvancedRepair[] MAR = vessel.Parts.Where(p => p?.GetComponent<ModuleAdvancedRepair>() != null).Select(p => p?.GetComponent<ModuleAdvancedRepair>()).ToArray();
                    Debug.Log("RepairAll", "Found " + MAR?.Length + " modules of type <ModuleAdvancedRepair>");

                    for (int i = 0; i < MAR?.Length; i++)
                    {
                        MAR[i].Repair();
                    }

                    ModuleWheelDamage[] MWB = vessel.Parts.Where(p => p?.GetComponent<ModuleWheelDamage>() != null).Select(p => p?.GetComponent<ModuleWheelDamage>()).ToArray();
                    Debug.Log("RepairAll", "Found " + MWB?.Length + " modules of type <ModuleWheelDamage>");

                    for (int i = 0; i < MWB?.Length; i++)
                    {
                        if (MWB[i].isRepairable && MWB[i].isDamaged)
                        {
                            MWB[i].EventRepairExternal();
                        }
                    }

                    ModuleParachute[] MP = vessel.Parts.Where(p => p?.GetComponent<ModuleParachute>() != null).Select(p => p?.GetComponent<ModuleParachute>()).ToArray();
                    Debug.Log("RepairAll", "Found " + MP?.Length + " modules of type <ModuleParachute>");

                    for (int i = 0; i < MP?.Length; i++)
                    {
                        if (MP[i].deploymentState == ModuleParachute.deploymentStates.CUT)
                        {
                            MP[i].Repack();
                        }
                    }
                }
            }
        }
    }

    internal class ModuleAdvancedRepair : PartModule
    {
        internal List<Collider> newCollidersList = new List<Collider>();

        int retractIn = 0;

        [KSPEvent(guiActiveUnfocused = false, guiActive = false, guiActiveEditor = false, guiName = "Retract Manually")]
        public void RetractManually()
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                ModuleDeployableSolarPanel MDSP = part?.GetComponent<ModuleDeployableSolarPanel>();

                Debug.Log("RetractManually", "ModuleDeployableSolarPanel = " + MDSP);
                if (MDSP != null)
                {
                    Debug.Log("RetractManually", "retractable = " + MDSP.retractable);
                    if (!MDSP.retractable)
                    {
                        MDSP.deployState = ModuleDeployablePart.DeployState.BROKEN;
                        Repair(0);
                    }
                }
            }
        }

        [KSPEvent(guiActiveUnfocused = false, guiActive = false, guiActiveEditor = false, guiName = "Repair Part")]
        public void Repair()
        {
            Repair(1);
        }

        void Repair(int cost = 1)
        {
            if (HighLogic.LoadedSceneIsFlight)
            {
                ModuleDeployablePart MDP = part?.GetComponent<ModuleDeployablePart>();

                Debug.Log("Repair", "ModuleDeployablePart = " + MDP);
                if (MDP != null)
                {
                    Debug.Log("Repair", "DeployState = " + MDP?.deployState);
                    if (MDP?.deployState == ModuleDeployablePart.DeployState.BROKEN)
                    {
                        Debug.Log("Repair", "hasPivot = " + MDP?.hasPivot);
                        if (MDP.hasPivot)
                        {
                            GameObject broken = MDP?.panelRotationTransform?.gameObject;
                            Debug.Log("Repair", "broken = " + broken);
                            GameObject prefab = part?.partInfo?.partPrefab?.FindModelTransform(MDP.pivotName)?.gameObject;
                            Debug.Log("Repair", "prefab = " + prefab);

                            if (broken != null && prefab != null)
                            {
                                // Delete breakObjects
                                FieldInfo MDP_breakObjects = typeof(ModuleDeployablePart).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Skip(3).FirstOrDefault();
                                Debug.Log("Repair", "MDP_breakObjects.Name = " + MDP_breakObjects?.Name + ", should be = breakObjects");

                                List<GameObject> oldBreakObjects = (List<GameObject>)MDP_breakObjects?.GetValue(MDP);
                                Debug.Log("Repair", "Destroying " + oldBreakObjects?.Count + " breakObjects");
                                for (int i = 0; i < oldBreakObjects?.Count; i++)
                                {
                                    DestroyImmediate(oldBreakObjects[i]);
                                }

                                Debug.Log("Repair", "Replace breakObject with empty list");
                                MDP_breakObjects.SetValue(MDP, new List<GameObject>());


                                // Replace Part model
                                Debug.Log("Repair", "Clone the prefab GameObject");
                                GameObject clone = Instantiate(prefab);
                                clone.name = broken.name;
                                clone.transform.SetParent(broken.transform.parent);
                                clone.transform.localPosition = prefab.transform.localPosition;
                                clone.transform.localScale = prefab.transform.localScale;
                                clone.transform.localRotation = prefab.transform.localRotation;
                                DestroyImmediate(broken);
                                clone.SetActive(true);
                                Debug.Log("Repair", "clone = " + clone);


                                // Reset panelRotationTransfrom
                                MDP.panelRotationTransform = clone.transform;
                                MDP.originalRotation = MDP.panelRotationTransform.localRotation;
                                MDP.currentRotation = MDP.originalRotation;
                                Debug.Log("Repair", "new panelRotationTransform = " + MDP.panelRotationTransform);


                                // Reset secondaryTransform
                                FieldInfo MDP_secondaryTransform = typeof(ModuleDeployablePart).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).FirstOrDefault();
                                Debug.Log("Repair", "MDP_secondaryTransform.Name = " + MDP_secondaryTransform?.Name + ", should be = secondaryTransform");

                                Transform secondaryTransform = part?.FindModelTransform(MDP.secondaryTransformName);
                                MDP_secondaryTransform.SetValue(MDP, secondaryTransform);
                                Debug.Log("Repair", "new secondaryTransform = " + secondaryTransform);


                                // Reset ModuleDeployableSolarPanel.trackingDotTransform
                                ModuleDeployableSolarPanel MDSP = MDP as ModuleDeployableSolarPanel;
                                Debug.Log("Repair", "ModuleDeployablePart as ModuleDeployableSolarPanel = " + MDSP);
                                if (MDSP != null)
                                {
                                    Debug.Log("Repair", "MDSP.useRaycastForTrackingDot = " + MDSP.useRaycastForTrackingDot);
                                    if (MDSP.useRaycastForTrackingDot)
                                    {
                                        MDSP.trackingDotTransform = secondaryTransform;
                                    }
                                    else
                                    {
                                        MDSP.trackingDotTransform = MDSP.panelRotationTransform;
                                    }
                                    Debug.Log("Repair", "MDSP.trackingDotTransform = " + MDSP.trackingDotTransform);
                                }


                                // Destroy new colliders
                                for (int i = 0; i < newCollidersList?.Count; i++)
                                {
                                    Debug.Log("Repair", "Destroying newCollider[" + i + "] = " + newCollidersList[i]);
                                    DestroyImmediate(newCollidersList[i]);
                                }
                                Debug.Log("Repair", "Clearing newCollidersList(" + newCollidersList?.Count + ")");
                                newCollidersList?.Clear();


                                // Reset ModuleDeployablePart state
                                Debug.Log("Repair", "MDP.GetTrackingBodyTransforms()");
                                MDP.GetTrackingBodyTransforms();

                                Debug.Log("Repair", "MDP.useAnimation = " + MDP.useAnimation);
                                if (MDP.useAnimation)
                                {
                                    FieldInfo MDP_anim = typeof(ModuleDeployablePart).GetFields(BindingFlags.NonPublic | BindingFlags.Instance).Skip(1).FirstOrDefault();
                                    Debug.Log("Repair", "MDP_anim.Name = " + MDP_anim?.Name + ", should be = anim");
                                    MDP_anim.SetValue(MDP, part.GetComponentInChildren<Animation>(true));

                                    Debug.Log("Repair", "MDP.retractable = " + MDP.retractable);
                                    if (MDP.retractable)
                                    {
                                        MDP.deployState = ModuleDeployablePart.DeployState.EXTENDED;
                                        retractIn = 2;
                                        MDP.startFSM();
                                        Debug.Log("Repair", "after MDP.startFSM()");
                                    }
                                    else
                                    {
                                        MDP.deployState = ModuleDeployablePart.DeployState.RETRACTED;
                                        MDP.startFSM();
                                        Debug.Log("Repair", "after MDP.startFSM()");
                                    }

                                    Debug.Log("Repair", "MDP.deployState = " + MDP.deployState + ", retractIn = " + retractIn);
                                }
                                else
                                {
                                    MDP.deployState = ModuleDeployablePart.DeployState.EXTENDED;
                                    MDP.startFSM();
                                    Debug.Log("Repair", "after MDP.startFSM()");
                                }
                            }
                        }
                    }
                }
            }
        }

        public override void OnUpdate()
        {
            if (retractIn > 0)
            {
                retractIn--;

                if (retractIn == 0)
                {
                    part.GetComponent<ModuleDeployablePart>().Retract();
                }
            }
        }
    }
}
