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
                    var Dupe = UnityEngine.Object.Instantiate(GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button"), Child.transform);

                    Dupe.transform.localPosition = new Vector3(-25f, 219f, 0f);
                    Dupe.GetComponentInChildren<Text>(true).text = "Export";
                    Dupe.GetComponent<RectTransform>().sizeDelta = new Vector2(150f, 80f);
                    Dupe.GetComponent<Button>().onClick = new Button.ButtonClickedEvent();
                    Dupe.GetComponent<Button>().onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(
                        new Action(() =>
                        {
                            var FavsInList = Child.GetComponentsInChildren<VRCUiContentButton>(true).Select(o => o.field_Public_String_0).Where(p => p != null);

                            var Json = JsonConvert.SerializeObject(FavsInList);

                            if (!Directory.Exists(Environment.CurrentDirectory + "\\ExportedFavs"))
                            {
                                Directory.CreateDirectory(Environment.CurrentDirectory + "\\ExportedFavs");
                            }

                            var FilePath = Environment.CurrentDirectory + "\\ExportedFavs\\" +
                                           Child.Find("Button/TitleText").GetComponent<Text>().text + ".json";

                            File.WriteAllText(FilePath, Json);

                            ChillOkayPopup("Alert", "Your Fav List Was Exported To: " + FilePath + "\n\nYou Can Move It To " + Environment.CurrentDirectory + "\\UserData\\FavCatImport\\ To Import The Fav List Into Plague's Modpack.", PopupType.FullScreen);
                        })));

                    //Fix Collapsing Issue
                    var CollapseButton =
                        GameObject.Find(
                            "UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/Personal Avatar List/Button/");

                    CollapseButton.GetComponent<Button>().onClick.AddListener(DelegateSupport.ConvertDelegate<UnityAction>(
                        new Action(() =>
                        {
                            //This Should Run AFTER The Original, So It Should Be Opposite.
                            Dupe.transform.localPosition = CollapseButton.transform.Find("ToggleIcon").GetComponent<Image>().activeSprite.name ==
                                                           "collapsebutton" ? new Vector3(-125f, 219f, 0f) : new Vector3(-125f, 114f, 0f);
                        })));
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
