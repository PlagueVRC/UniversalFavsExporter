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
        public override void OnApplicationStart()
        {
            MelonCoroutines.Start(VRChat_OnUiManagerInit());
        }

        public IEnumerator VRChat_OnUiManagerInit()
        {
            while (VRCUiManager.prop_VRCUiManager_0 == null)
            {
                yield return new WaitForSeconds(1f);
            }

            MelonCoroutines.Start(DelayedUIInit());

            yield break;
        }

        public IEnumerator DelayedUIInit()
        {
            MelonLogger.Msg("Init!");

            while (true)
            {
                //Get All Fav Lists
                var AvatarFavsArea =
                    GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/");

                while (AvatarFavsArea == null || !AvatarFavsArea.active)
                {
                    yield return new WaitForSeconds(1f);

                    AvatarFavsArea = GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Vertical Scroll View/Viewport/Content/");
                }

                var Lists = Resources.FindObjectsOfTypeAll<UiAvatarList>().Select(o => o.transform).ToList();

                for (var i = 0; i < Lists.Count; i++)
                {
                    var Child = Lists[i];

                    if (Child.GetComponent<UiAvatarList>() != null && Child.Find("Button/TitleText/FavsExporter") == null) // Is A Avi List
                    {
                        //Make Button
                        var Dupe = UnityEngine.Object.Instantiate(GameObject.Find("UserInterface/MenuContent/Screens/Avatar/Change Button"), Child.Find("Button"));

                        Dupe.name = "FavsExporter";
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
                                                   MakeValidFileName(Child.Find("Button/TitleText").GetComponent<Text>().text) + ".json";

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

                        Dupe.SetActive(true);
                    }
                }

                yield return new WaitForSeconds(5f);
            }
        }

        static char[] _invalids;

        /// <summary>Replaces characters in <c>text</c> that are not allowed in 
        /// file names with the specified replacement character.</summary>
        /// <param name="text">Text to make into a valid filename. The same string is returned if it is valid already.</param>
        /// <param name="replacement">Replacement character, or null to simply remove bad characters.</param>
        /// <param name="fancy">Whether to replace quotes and slashes with the non-ASCII characters ” and ⁄.</param>
        /// <returns>A string that can be used as a filename. If the output string would otherwise be empty, returns "_".</returns>
        public static string MakeValidFileName(string text, char? replacement = '_', bool fancy = true)
        {
            StringBuilder sb = new StringBuilder(text.Length);
            var invalids = _invalids ?? (_invalids = Path.GetInvalidFileNameChars());
            bool changed = false;
            for (int i = 0; i < text.Length; i++)
            {
                char c = text[i];
                if (invalids.Contains(c))
                {
                    changed = true;
                    var repl = replacement ?? '\0';
                    if (fancy)
                    {
                        if (c == '"') repl = '”'; // U+201D right double quotation mark
                        else if (c == '\'') repl = '’'; // U+2019 right single quotation mark
                        else if (c == '/') repl = '⁄'; // U+2044 fraction slash
                    }
                    if (repl != '\0')
                        sb.Append(repl);
                }
                else
                    sb.Append(c);
            }
            if (sb.Length == 0)
                return "_";
            return changed ? sb.ToString() : text;
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
