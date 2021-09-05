using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MelonLoader;
using Newtonsoft.Json;
using UIExpansionKit.API;
using UIExpansionKit.Components;
using UnhollowerRuntimeLib;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[assembly: MelonInfo(typeof(UniversalFavsExporter.FavsExporterByPlague), "Universal Favs Exporter", "1.0", "Plague")]
[assembly: MelonGame("VRChat", "VRChat")]

namespace UniversalFavsExporter
{
    public class FavsExporterByPlague : MelonMod
    {
        public override void VRChat_OnUiManagerInit()
        {
            MelonCoroutines.Start(DelayedUIInit());
        }

        public IEnumerator DelayedUIInit()
        {
            //Get All Fav Lists
            var AvatarFavsArea =
                GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/");

            while (AvatarFavsArea == null || !AvatarFavsArea.active)
            {
                yield return new WaitForSeconds(1f);

                AvatarFavsArea = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/");
            }

            for (var i = 0; i < AvatarFavsArea.transform.childCount; i++)
            {
                var Child = AvatarFavsArea.transform.GetChild(i);

                if (Child.GetComponent<UiAvatarList>() != null) // Is A Avi List
                {
                    //Make Button
                    var Dupe = UnityEngine.Object.Instantiate(GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button"), Child.Find("Button"));

                    Dupe.GetComponent<RectTransform>().sizeDelta = new Vector2(30f, 80f);

                    //This Is Done To Fix Positioning
                    Dupe.transform.localPosition = new Vector3(115f, 0f, 0f);
                    Dupe.transform.SetParent(Child.Find("Button/TitleText"));

                    Dupe.GetComponentInChildren<Text>(true).text = "E";
                    Dupe.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    Dupe.GetComponent<Button>().onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(
                        new Action(() =>
                        {
                            var FavsInList = Child.GetComponentsInChildren<VRCUiContentButton>(true)
                                .Select(o => o.field_Public_String_0).Where(p => p != null).ToList();

                            if (FavsInList.Count > 0)
                            {
                                var Json = JsonConvert.SerializeObject(FavsInList);

                                if (!Directory.Exists(Environment.CurrentDirectory + "\\ExportedFavs"))
                                {
                                    Directory.CreateDirectory(Environment.CurrentDirectory + "\\ExportedFavs");
                                }

                                var FilePath = Environment.CurrentDirectory + "\\ExportedFavs\\" +
                                               Child.Find("Button/TitleText").GetComponent<Text>().text + ".json";

                                File.WriteAllText(FilePath, Json);

                                ChillOkayPopup("Alert",
                                    "Your Fav List Was Exported To: " + FilePath + "\n\nYou Can Move It To " +
                                    Environment.CurrentDirectory +
                                    "\\UserData\\FavCatImport\\ To Import The Fav List Into Plague's Modpack.\n\nModpack Discord Invite: https://plague.cx",
                                    PopupType.FullScreen);
                            }
                            else
                            {
                                ChillOkayPopup("Error",
                                    "No Favs In List To Export!",
                                    PopupType.FullScreen);
                            }
                        })));

                    Dupe.SetActive(Child.gameObject.active);

                    EnableDisableListener Listener = null;

                    Listener = Child.gameObject.GetComponent<EnableDisableListener>() == null ? Child.gameObject.AddComponent<EnableDisableListener>() : Child.gameObject.GetComponent<EnableDisableListener>();
                        
                    Listener.OnEnabled += () =>
                    {
                        Dupe.SetActive(true);
                    };

                    Listener.OnDisabled += () =>
                    {
                        Dupe.SetActive(false);
                    };
                }
            }

            MelonLogger.Msg("Init!");

            yield break;
        }

        internal enum PopupType
        {
            FullScreen,
            QuickMenu
        }

        internal static void ChillOkayPopup(string Title, string Content, PopupType type, string OkayText = "Okay", Action OkayAction = null)
        {
            ICustomShowableLayoutedMenu Popup = null;

            if (type == PopupType.FullScreen)
            {
                Popup = ExpansionKitApi.CreateCustomFullMenuPopup(LayoutDescription.WideSlimList);
            }
            else
            {
                Popup = ExpansionKitApi.CreateCustomQuickMenuPage(LayoutDescription.WideSlimList);
            }

            Popup.AddSimpleButton(Title, delegate () { });
            Popup.AddLabel(Content);
            Popup.AddSpacer();
            Popup.AddSpacer();
            Popup.AddSpacer();
            Popup.AddSpacer();
            Popup.AddSpacer();
            Popup.AddSimpleButton(OkayText, () =>
            {
                Popup.Hide();
                OkayAction?.Invoke();
            });

            Popup.Show();
        }
    }
}
