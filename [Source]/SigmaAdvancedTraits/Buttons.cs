using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using KSP.UI;
using KSP.UI.TooltipTypes;


namespace SigmaAdvancedTraitsPlugin
{
    [KSPAddon(KSPAddon.Startup.SpaceCentre, false)]
    internal class SpaceCenterTrigger : MonoBehaviour
    {
        void Start()
        {
            CrewListItem[] items = Resources.FindObjectsOfTypeAll<CrewListItem>();
            for (int i = 0; i < items?.Length; i++)
            {
                if (items[i].name == "ListItem_enlisted")
                {
                    items[i].gameObject.AddOrGetComponent<AdvancedTraitsButtons>();
                }
            }
        }
    }

    internal class AdvancedTraitsButtons : MonoBehaviour
    {
        GameObject[] advTraitButtons = new GameObject[2];

        internal static Dictionary<string, string[]> AdvTraits = new Dictionary<string, string[]>
        {
            { "Engineer", new[] { "Engineer0", "Engineer1" } },
            { "Pilot", new[] { "ManeuverExecution", "RemoteControl" } },
            { "Scientist", new[] { "Scientist0", "Scientist1" } }
        };

        static Dictionary<string, string[]> AdvTooltips = new Dictionary<string, string[]>
        {
            {
                "Engineer",
                new[]
                {
                    // Engineer Trait 0 Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#989898>AdvTraits[\"Engineer\"][0]</color>",
                    // Engineer Trait 1 Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#989898>AdvTraits[\"Engineer\"][1]</color>"
                }
            },
            {
                "Pilot",
                new[]
                {
                    // Maneuver Execution Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#BADA55>Maneuver Execution</color>",
                    // Remote Control Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#BADA55>Remote Control</color>"
                }
            },
            {
                "Scientist",
                new[]
                {
                    // Scientist Trait 0 Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#55BADA>AdvTraits[\"Scientist\"][0]</color>",
                    // Scientist Trait 1 Tooltip
                    "<size=10><color=#FFFFFF>Advanced Training in</size></color>\n" +
                    "<color=#55BADA>AdvTraits[\"Scientist\"][1]</color>"
                }
            }
        };

        static Dictionary<string, string[]> AdvDescriptions = new Dictionary<string, string[]>
        {
            {
                "Engineer",
                new[]
                {
                    // Engineer Trait0 Description
                    "After this training, the engineer will be able\n" +
                    "to do something.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> \n" +
                    "<color=#FDD901>Level 3:</color> \n" +
                    "<color=#FDD901>Level 4:</color> \n" +
                    "<color=#FDD901>Level 5:</color> ",
                    // Engineer Trait1 Description
                    "After this training, the engineer will be able\n" +
                    "to do something.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> \n" +
                    "<color=#FDD901>Level 3:</color> \n" +
                    "<color=#FDD901>Level 4:</color> \n" +
                    "<color=#FDD901>Level 5:</color> "
                }
            },
            {
                "Pilot",
                new[]
                {
                    // Maneuvre Execution Desctiption
                    "After this training, the pilot will be able\n" +
                    "to execute burns without assistance.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> Burn Only (might overshoot)\n" +
                    "<color=#FDD901>Level 3:</color> Point toward maneuver\n" +
                    "<color=#FDD901>Level 4:</color> Precise Burn\n" +
                    "<color=#FDD901>Level 5:</color> Execute multiple maneuvers",
                    // Remote Control Desctiption
                    "After this training, the pilot will be able\n" +
                    "to act as a probe control point.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> Short range, direct link only\n" +
                    "<color=#FDD901>Level 3:</color> Medium range, direct link only\n" +
                    "<color=#FDD901>Level 4:</color> Long range, direct link only\n" +
                    "<color=#FDD901>Level 5:</color> Long range, can use relays"
                }
            },
            {
                "Scientist",
                new[]
                {
                    // Scientist Trait0 Description
                    "After this training, the scientist will be able\n" +
                    "to do something.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> \n" +
                    "<color=#FDD901>Level 3:</color> \n" +
                    "<color=#FDD901>Level 4:</color> \n" +
                    "<color=#FDD901>Level 5:</color> ",
                    // Scientist Trait1 Description
                    "After this training, the scientist will be able\n" +
                    "to do something.\n\n" +
                    "<color=#FDD901>Features unlocked by level:</color>\n" +
                    "<color=#FDD901>Level 2:</color> \n" +
                    "<color=#FDD901>Level 3:</color> \n" +
                    "<color=#FDD901>Level 4:</color> \n" +
                    "<color=#FDD901>Level 5:</color> "
                }
            }
        };

        void Start()
        {
            ProtoCrewMember kerbal = GetComponent<CrewListItem>()?.GetCrewRef();
            Debug.Log("SigmaLog: kerbal = " + kerbal);

            Debug.Log("SigmaLog: experienceLevel = " + kerbal?.experienceLevel);
            if (kerbal?.experienceLevel > 1)
            {
                Debug.Log("SigmaLog: kerbal.rosterStatus = " + kerbal.rosterStatus);
                if (kerbal.rosterStatus == ProtoCrewMember.RosterStatus.Available)
                {
                    Debug.Log("SigmaLog: kerbal.trait = " + kerbal.trait);
                    if (AdvTraits.ContainsKey(kerbal.trait))
                    {
                        if (!kerbal.careerLog.HasEntry("Training1", AdvTraits[kerbal.trait][0]) && !kerbal.careerLog.HasEntry("Training1", AdvTraits[kerbal.trait][1]))
                        {
                            Debug.Log("SigmaLog: kerbal.careerLog.HasEntry(\"AdvancedTraining\") = false");
                            SetupButtons(kerbal, 0);
                            SetupButtons(kerbal, 1);
                        }
                        else
                        {
                            Debug.Log("SigmaLog: kerbal.careerLog.HasEntry(\"AdvancedTraining\") = true");
                        }
                    }
                }
            }
        }

        void SetupButtons(ProtoCrewMember kerbal, int advTraitID)
        {
            // Create GameObject
            GameObject exitOBJ = GameObject.Find("ExitButton");
            GameObject buttonOBJ = advTraitButtons[advTraitID] = Instantiate(exitOBJ);
            DestroyImmediate(buttonOBJ.GetComponent<Button>());
            buttonOBJ.transform.SetParent(transform);

            // Fix Position and Scale
            Vector3[] corners = new Vector3[4];
            GameObject label_courage = gameObject.GetChild("label_courage");
            ((RectTransform)label_courage.transform).GetWorldCorners(corners);
            buttonOBJ.transform.position = corners[1];
            ((RectTransform)buttonOBJ.transform).GetWorldCorners(corners);
            buttonOBJ.transform.localScale /= corners[3].x - corners[0].x;
            ((RectTransform)exitOBJ.transform).GetWorldCorners(corners);
            buttonOBJ.transform.localScale *= corners[3].x - corners[0].x;
            buttonOBJ.transform.position += Vector3.down * (corners[1].y - corners[0].y) * 0.375f + Vector3.left * (corners[3].x - corners[0].x) * (1.365f - advTraitID * 0.865f);

            // Get Texture
            Texture2D texture = Resources.FindObjectsOfTypeAll<Texture2D>().FirstOrDefault(t => t.name == "Sigma/AdvancedTraits/Textures/" + AdvTraits[kerbal.trait][advTraitID]);

            // Create Button
            Button newButton = buttonOBJ.AddComponent<Button>();
            newButton.image = buttonOBJ.AddOrGetComponent<Image>();
            newButton.navigation = new Navigation { mode = Navigation.Mode.None };
            newButton.transition = Selectable.Transition.SpriteSwap;
            newButton.image.sprite = Sprite.Create(texture, new Rect(0, 128, 128, 128), Vector2.zero);
            newButton.spriteState = new SpriteState
            {
                highlightedSprite = Sprite.Create(texture, new Rect(128, 128, 128, 128), Vector2.zero),
                pressedSprite = Sprite.Create(texture, new Rect(0, 0, 128, 128), Vector2.zero),
                disabledSprite = Sprite.Create(texture, new Rect(0, 128, 128, 128), Vector2.zero)
            };

            // Fix Tooltip
            TooltipFixer tf = buttonOBJ.AddComponent<TooltipFixer>();
            tf.textString = AdvTooltips[kerbal.trait][advTraitID];
            tf.description = "\n\n<size=10><color=#FFFFFF>" + AdvDescriptions[kerbal.trait][advTraitID] + "</color></size>";

            // Add Listener
            newButton.onClick.AddListener(() => OnClick(kerbal, advTraitID));
            newButton.gameObject.SetActive(true);
        }

        void OnClick(ProtoCrewMember kerbal, int advTraitID)
        {
            kerbal.careerLog.AddEntry(new FlightLog.Entry(kerbal.careerLog.Flight, "Training1", AdvTraits[kerbal.trait][advTraitID]));
            kerbal.careerLog.AddFlight();
            DestroyImmediate(advTraitButtons[0]);
            DestroyImmediate(advTraitButtons[1]);
        }
    }

    internal class TooltipFixer : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        internal float wait = 1;
        internal string textString = "";
        internal string description = "";

        TooltipController_CrewAC ac;
        TooltipController_Text tc;
        bool fixTooltip = true;
        float time = 0;

        void Start()
        {
            ac = transform.parent.GetComponent<TooltipController_CrewAC>();
            tc = GetComponent<TooltipController_Text>();
            tc.textString = textString;
        }

        void IPointerEnterHandler.OnPointerEnter(PointerEventData eventData)
        {
            fixTooltip = true;
            time = 0;
            TimingManager.UpdateAdd(TimingManager.TimingStage.Normal, WhileMouseOver);
        }

        void IPointerExitHandler.OnPointerExit(PointerEventData eventData)
        {
            fixTooltip = true;
            time = 0;
            tc.textString = textString;
            TimingManager.UpdateRemove(TimingManager.TimingStage.Normal, WhileMouseOver);
            UIMasterController.Instance.SpawnTooltip(ac);
        }

        void OnDestroy()
        {
            TimingManager.UpdateRemove(TimingManager.TimingStage.Normal, WhileMouseOver);
            UIMasterController.Instance.SpawnTooltip(ac);
        }

        void WhileMouseOver()
        {
            if ((Object)UIMasterController.Instance.CurrentTooltip != tc)
            {
                UIMasterController.Instance.DespawnTooltip(ac);
                UIMasterController.Instance.SpawnTooltip(tc);
            }
            if (tc.continuousUpdate)
            {
                tc.continuousUpdate = false;
            }
            if (fixTooltip)
            {
                time += Time.unscaledDeltaTime;
                if (time > wait)
                {
                    fixTooltip = false;
                    tc.continuousUpdate = true;
                    tc.textString += description;
                }
            }
        }
    }
}
