#if UNITY_2019_3_OR_NEWER
using System.Collections.Generic;
using System.Text;

#if UNITY_EDITOR
using UnityEditor;
using System.IO;
using System.Xml;
using System.Xml.XPath;
#endif
using UnityEngine;

namespace CoinMode
{
    internal class CoinModeDeepLinkSetup
    {
#if UNITY_EDITOR
        public enum DeepLinkPlatform
        {
            Android,
            iOS,
        }

        private static string androidPath { get { return Application.dataPath + "/Plugins/Android"; } }
        private static string androidManifestPath { get { return Application.dataPath + "/Plugins/Android/AndroidManifest.xml"; } }

        private const string defaultAndroidManifest =
        "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" +
        "<manifest xmlns:android=\"http://schemas.android.com/apk/res/android\" xmlns:tools=\"http://schemas.android.com/tools\">\n" +
            "\t<application/>\n" +
        "</manifest>";

        private static void AlignIosAndAndroidUrlSchemes()
        {
            List<string> androidSchemes = AndroidSchemes();
            List<string> iosSchemes = IosSchemes();

            for (int i = 0; i < iosSchemes.Count; i++)
            {
                if (!androidSchemes.Contains(iosSchemes[i]))
                {
                    AddDeepLinkUrlScheme(DeepLinkPlatform.Android, iosSchemes[i]);
                }
            }

            for (int i = 0; i < androidSchemes.Count; i++)
            {
                if (!iosSchemes.Contains(androidSchemes[i]))
                {
                    AddDeepLinkUrlScheme(DeepLinkPlatform.iOS, androidSchemes[i]);
                }
            }
        }

        private static List<string> AndroidSchemes()
        {
            List<string> androidSchemes = new List<string>();

            return androidSchemes;
        }

        private static List<string> IosSchemes()
        {
            return new List<string>(PlayerSettings.iOS.iOSUrlSchemes);
        }

        internal static bool AddDeepLinkUrlScheme(DeepLinkPlatform platform, string urlScheme)
        {
            switch (platform)
            {
                case DeepLinkPlatform.Android:
                    return AddAndroidDeepLinkUrlScheme(urlScheme);
                case DeepLinkPlatform.iOS:
                    {
                        List<string> iosUrls = new List<string>(PlayerSettings.iOS.iOSUrlSchemes);
                        if(!iosUrls.Contains(urlScheme))
                        {
                            iosUrls.Add(urlScheme);
                            PlayerSettings.iOS.iOSUrlSchemes = iosUrls.ToArray();
                            AssetDatabase.SaveAssets();
                            return true;
                        }
                        return false;
                    }
            }
            return false;
        }

        internal static bool RemoveDeepLinkUrlScheme(DeepLinkPlatform platform, string urlScheme)
        {
            switch (platform)
            {
                case DeepLinkPlatform.Android:
                    return RemoveAndroidDeepLinkUrlScheme(urlScheme);
                case DeepLinkPlatform.iOS:
                    {
                        List<string> iosUrls = new List<string>(PlayerSettings.iOS.iOSUrlSchemes);
                        for (int i = iosUrls.Count - 1; i >= 0; i--)
                        {
                            if (iosUrls[i] == urlScheme)
                            {
                                iosUrls.RemoveAt(i);
                            }
                        }
                        PlayerSettings.iOS.iOSUrlSchemes = iosUrls.ToArray();
                        AssetDatabase.SaveAssets();
                    }
                    break;
            }
            return false;
        }

        private static bool AddAndroidDeepLinkUrlScheme(string urlScheme)
        {
            bool documentModified = false;
            bool added = false;
            XmlDocument androidManifest = LoadOrCreateManifestXml();

            XmlNodeList applicationNodes;
            XmlNode root = androidManifest.DocumentElement;

            if (root.Name != "manifest")
            {
                CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", "Root element of AndroidManifest is {0}, expected <manifest>", root.Name);
                return false;
            }

            applicationNodes = root.SelectNodes("descendant::application");

            XmlNode applicationNode = null;
            if (applicationNodes.Count > 0)
            {
                XmlNode activityNode = null;
                foreach (XmlNode appNode in applicationNodes)
                {
                    XmlNodeList activityNodes = appNode.SelectNodes("descendant::activity");
                    if (activityNodes.Count > 0)
                    {
                        foreach (XmlNode actNode in activityNodes)
                        {
                            XmlNode activityName = actNode.Attributes.GetNamedItem("name", "http://schemas.android.com/apk/res/android");
                            if (activityName != null)
                            {
                                if (activityName.Value == "com.unity3d.player.UnityPlayerActivity")
                                {
                                    activityNode = actNode;
                                    XmlNode mainIntentFilterNode = null;
                                    XmlNode viewIntentFilterNode = null;
                                    XmlNodeList intentFilterNodes = activityNode.SelectNodes("descendant::intent-filter");
                                    if (intentFilterNodes.Count > 0)
                                    {
                                        foreach (XmlNode intNode in intentFilterNodes)
                                        {
                                            XmlNode actionNode = intNode.SelectSingleNode("descendant::action");
                                            if (actionNode != null)
                                            {
                                                XmlNode actionName = actionNode.Attributes.GetNamedItem("name", "http://schemas.android.com/apk/res/android");
                                                if (actionName != null)
                                                {
                                                    if (actionName.Value == "android.intent.action.MAIN")
                                                    {
                                                        mainIntentFilterNode = intNode;
                                                    }
                                                    else if (actionName.Value == "android.intent.action.VIEW")
                                                    {
                                                        XmlNode dataNode = activityNode.SelectSingleNode("descendant::data");
                                                        if (dataNode != null)
                                                        {
                                                            XmlNode dataScheme = dataNode.Attributes.GetNamedItem("scheme", "http://schemas.android.com/apk/res/android");
                                                            XmlNode dataHost = dataNode.Attributes.GetNamedItem("host", "http://schemas.android.com/apk/res/android");
                                                            if (dataScheme != null && dataHost != null)
                                                            {
                                                                if(dataScheme.Value == urlScheme && dataHost.Value == DeepLinkUtilities.urlHost)
                                                                {
                                                                    viewIntentFilterNode = dataNode;
                                                                }
                                                            }
                                                            else
                                                            {
                                                                CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme",
                                                                "data for android.intent.action.VIEW found in com.unity3d.player.UnityPlayerActivity does not specify scheme and host in AndroidManifest.xml");
                                                            }
                                                        }
                                                        else
                                                        {
                                                            CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme",
                                                                "android.intent.action.VIEW found in com.unity3d.player.UnityPlayerActivity is missing data in AndroidManifest.xml");
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", "Unnamed action found in com.unity3d.player.UnityPlayerActivity in AndroidManifest.xml");
                                                }
                                            }
                                            else
                                            {
                                                CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", "intent-filter found with no defined action in AndroidManifest.xml");
                                            }
                                        }
                                    }

                                    if (mainIntentFilterNode == null)
                                    {
                                        AddMainIntentFilterXml(androidManifest, activityNode);
                                        documentModified = true;
                                        CoinModeLogging.LogMessage("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", 
                                            "android.intent.action.MAIN action added to com.unity3d.player.UnityPlayerActivity as one was not found in AndroidManifest.xml");
                                    }

                                    if (viewIntentFilterNode == null)
                                    {
                                        AddViewIntentFilterXml(androidManifest, activityNode, urlScheme, DeepLinkUtilities.urlHost);
                                        documentModified = true;
                                        added = true;
                                    }
                                }
                            }
                            else
                            {
                                CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", "Unnamed activity element found in AndroidManifest.xml");
                            }
                        }
                    }
                }

                if (activityNode == null)
                {
                    applicationNode = applicationNodes.Item(0);
                    AddUnityActionXml(androidManifest, applicationNode, urlScheme, DeepLinkUtilities.urlHost);
                    documentModified = true;
                    added = true;
                }
            }
            else
            {
                applicationNode = androidManifest.CreateElement("application");
                root.AppendChild(applicationNode);
                AddUnityActionXml(androidManifest, applicationNode, urlScheme, DeepLinkUtilities.urlHost);
                documentModified = true;
                added = true;
            }

            if(documentModified)
            {
                androidManifest.Save(androidManifestPath);
            }
            return added;
        }

        private static bool RemoveAndroidDeepLinkUrlScheme(string urlScheme)
        {
            bool documentModified = false;
            bool removed = false;
            XmlDocument androidManifest = LoadOrCreateManifestXml();
            
            XmlNode root = androidManifest.DocumentElement;

            if (root.Name != "manifest")
            {
                CoinModeLogging.LogWarning("CoinModeDeepLinkHelper", "AddAndroindDeepLinkUrlScheme", "Root element of AndroidManifest is {0}, expected <manifest>", root.Name);
                return false;
            }

            XmlNamespaceManager nsMgr = new XmlNamespaceManager(androidManifest.NameTable);
            nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");
            string xpath = "descendant::data[@android:scheme='" + urlScheme + "' and @android:host='" + DeepLinkUtilities.urlHost + "']";
            XmlNode viewActionDataNode = root.SelectSingleNode(xpath, nsMgr);

            if(viewActionDataNode != null)
            {
                documentModified = true;
                removed = true;
                XmlNode intentFilterNode = viewActionDataNode.ParentNode;
                XmlNode activityNode = intentFilterNode.ParentNode;
                intentFilterNode.ParentNode.RemoveChild(intentFilterNode);
                XmlNodeList intentFilterNodes = activityNode.SelectNodes("descendant::intent-filter");
                if (intentFilterNodes.Count > 0)
                {
                    bool viewNodesRemain = false;
                    foreach (XmlNode intNode in intentFilterNodes)
                    {
                        XmlNode actionNode = intNode.SelectSingleNode("descendant::action");
                        XmlNode actionName = actionNode.Attributes.GetNamedItem("name", "http://schemas.android.com/apk/res/android");
                        if (actionName != null)
                        {
                            if (actionName.Value == "android.intent.action.VIEW")
                            {
                                viewNodesRemain = true;
                                break;
                            }
                        }
                    }
                    if(!viewNodesRemain)
                    {
                        activityNode.ParentNode.RemoveChild(activityNode);
                    }
                }
                else
                {
                    activityNode.ParentNode.RemoveChild(activityNode);
                }
            }

            if (documentModified)
            {
                androidManifest.Save(androidManifestPath);
            }
            return removed;
        }

        private static void AddUnityActionXml(XmlDocument doc, XmlNode parent, string urlScheme, string urlHost)
        {
            XmlNode activityNode = AddUnityActivityElement(doc, parent);

            AddMainIntentFilterXml(doc, activityNode);
            AddViewIntentFilterXml(doc, activityNode, urlScheme, urlHost);
        }

        private static void AddMainIntentFilterXml(XmlDocument doc, XmlNode parent)
        {
            XmlNode mainIntentFilterNode = AddIntentFilterElement(doc, parent);
            AddAndroidActionElement(doc, mainIntentFilterNode, "MAIN");
            AddAndroidCategoryElement(doc, mainIntentFilterNode, "LAUNCHER");
        }

        private static void AddViewIntentFilterXml(XmlDocument doc, XmlNode parent, string urlScheme, string urlHost)
        {
            XmlNode viewIntentFilterNode = AddIntentFilterElement(doc, parent);
            AddAndroidActionElement(doc, viewIntentFilterNode, "VIEW");
            AddAndroidCategoryElement(doc, viewIntentFilterNode, "DEFAULT");
            AddAndroidCategoryElement(doc, viewIntentFilterNode, "BROWSABLE");
            AddUrlDataElement(doc, viewIntentFilterNode, urlScheme, urlHost);
        }

        private static XmlNode AddUnityActivityElement(XmlDocument doc, XmlNode parent)
        {
            XmlNode activityNode = doc.CreateElement("activity");

            XmlNode nameAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "name", "http://schemas.android.com/apk/res/android");
            nameAttr.Value = "com.unity3d.player.UnityPlayerActivity";
            activityNode.Attributes.SetNamedItem(nameAttr);

            XmlNode themeAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "theme", "http://schemas.android.com/apk/res/android");
            themeAttr.Value = "@style/UnityThemeSelector";
            activityNode.Attributes.SetNamedItem(themeAttr);

            parent.AppendChild(activityNode);
            return activityNode;
        }

        private static XmlNode AddIntentFilterElement(XmlDocument doc, XmlNode parent)
        {
            XmlNode intentFilter = doc.CreateElement("intent-filter");
            parent.AppendChild(intentFilter);
            return intentFilter;
        }

        private static XmlNode AddAndroidActionElement(XmlDocument doc, XmlNode parent, string action)
        {
            XmlNode actionNode = doc.CreateElement("action");

            XmlNode nameAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "name", "http://schemas.android.com/apk/res/android");
            nameAttr.Value = "android.intent.action." + action;
            actionNode.Attributes.SetNamedItem(nameAttr);
            parent.AppendChild(actionNode);
            return actionNode;
        }

        private static XmlNode AddAndroidCategoryElement(XmlDocument doc, XmlNode parent, string category)
        {
            XmlNode categoryNode = doc.CreateElement("category");

            XmlNode nameAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "name", "http://schemas.android.com/apk/res/android");
            nameAttr.Value = "android.intent.category." + category;
            categoryNode.Attributes.SetNamedItem(nameAttr);
            parent.AppendChild(categoryNode);
            return categoryNode;
        }

        private static XmlNode AddUrlDataElement(XmlDocument doc, XmlNode parent, string scheme, string host)
        {
            XmlNode dataNode = doc.CreateElement("data");

            XmlNode schemeAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "scheme", "http://schemas.android.com/apk/res/android");
            schemeAttr.Value = scheme;
            dataNode.Attributes.SetNamedItem(schemeAttr);

            XmlNode hostAttr = doc.CreateNode(XmlNodeType.Attribute, "android", "host", "http://schemas.android.com/apk/res/android");
            hostAttr.Value = host;
            dataNode.Attributes.SetNamedItem(hostAttr);

            parent.AppendChild(dataNode);
            return dataNode;
        }

        private static XmlDocument LoadOrCreateManifestXml()
        {
            XmlDocument document = new XmlDocument();
            if (!Directory.Exists(androidPath))
            {
                Directory.CreateDirectory(androidPath);
                CreateDefaultManifest();
            }
            else if(!File.Exists(androidManifestPath))
            {
                CreateDefaultManifest();
            }
            document.Load(androidManifestPath);
            return document;
        }

        private static void CreateDefaultManifest()
        {
            using (FileStream fs = File.Create(androidManifestPath))
            {
                byte[] info = new UTF8Encoding(true).GetBytes(defaultAndroidManifest);
                fs.Write(info, 0, info.Length);
            }
        }
#endif
    }
}
#endif