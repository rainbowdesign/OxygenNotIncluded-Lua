using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;
using Klei.CustomSettings;
using Harmony;

namespace Co
{
    static public class References
    {

        static public ToggleSettingConfig customtogglesetting(string toggleid, string togglelabel, string toggletooltip)
        {
            return new ToggleSettingConfig(toggleid, togglelabel, toggletooltip,
                                                                               new SettingLevel("Disabled",
                                                                                                togglelabel,
                                                                                                toggletooltip),
                                                                               new SettingLevel("Enabled",
                                                                                                togglelabel,
                                                                                                toggletooltip), "Disabled", "Disabled");
        }
        static public ListSettingConfig customlistsetting(
            string settingsidx, string settingslabelx, string settingstooltipx, List<SettingLevel> settings)
        {
            return new ListSettingConfig(settingsidx, settingslabelx, settingstooltipx, settings, "Disabled", "Disabled");
        }

        public static string getgamesettingslevel(string settingsname)
        {

            if (CustomGameSettings.Instance == null) return "Disabled";
            if (!CustomGameSettings.Instance.QualitySettings.ContainsKey(settingsname)) return "Disabled";
            {
                return CustomGameSettings.Instance.GetCurrentQualitySetting(settingsname).id;
            }
        }
    }
}
