﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using dsweb_electron6.Models;
using dsweb_electron6;

namespace dsweb_electron6.Data
{
    public class DSdata 
    {
        public Dictionary<string, KeyValuePair<double, int>> WINRATE { get; private set; } = new Dictionary<string, KeyValuePair<double, int>>();
        public Dictionary<string, Dictionary<string, KeyValuePair<double, int>>> WINRATEVS { get; private set; } = new Dictionary<string, Dictionary<string, KeyValuePair<double, int>>>();
        public Dictionary<string, KeyValuePair<double, int>> WINRATE_PLAYER { get; private set; } = new Dictionary<string, KeyValuePair<double, int>>();
        public Dictionary<string, Dictionary<string, KeyValuePair<double, int>>> WINRATEVS_PLAYER { get; private set; } = new Dictionary<string, Dictionary<string, KeyValuePair<double, int>>>();

        public Dictionary<string, Dictionary<string, double>> SYNERGY { get; private set; } = new Dictionary<string, Dictionary<string, double>>();

        // mode, startdate, enddate, data

        public static string[] s_races { get; } = new string[]
        {
                "Abathur",
                 "Alarak",
                 "Artanis",
                 "Dehaka",
                 "Fenix",
                 "Horner",
                 "Karax",
                 "Kerrigan",
                 "Nova",
                 "Raynor",
                 "Stetmann",
                 "Stukov",
                 "Swann",
                 "Tychus",
                 "Vorazun",
                 "Zagara",
                 "Protoss",
                 "Terran",
                 "Zerg"
        };

        public static string[] s_races_cmdr { get; } = new string[]
        {
                "Abathur",
                 "Alarak",
                 "Artanis",
                 "Dehaka",
                 "Fenix",
                 "Horner",
                 "Karax",
                 "Kerrigan",
                 "Nova",
                 "Raynor",
                 "Stetmann",
                 "Stukov",
                 "Swann",
                 "Tychus",
                 "Vorazun",
                 "Zagara",
                 "Protoss",
                 "Terran",
                 "Zerg"
        };

        public static string[] s_gamemodes { get; } = new string[]
        {
            "GameModeBrawlCommanders",
            "GameModeBrawlStandard",
            "GameModeCommanders",
            "GameModeCommandersHeroic",
            "GameModeGear",
            "GameModeSabotage",
            "GameModeStandard",
            "GameModeSwitch"
        };

        public static string[] s_breakpoints { get; } = new string[]
        {
                 "MIN5",
                 "MIN10",
                 "MIN15",
                 "ALL",
        };

        public static string[] s_builds { get; } = new string[]
        {
            "PAX",
            "Feralan",
            "Panzerfaust"
        };

        public static string[] s_players { get; } = new string[]
        {
            "player",
            "player1",
            "player2",
            "player3",
            "player4",
            "player5",
            "player6"
        };

        public static Dictionary<string, string> INFO { get; } = new Dictionary<string, string>() {
            { "Winrate", "Winrate: Shows the winrate for each commander. When selecting a commander on the left it shows the winrate of the selected commander when matched vs the other commanders." },
            { "MVP", "MVP: Shows the % for the most ingame damage for each commander based on mineral value killed. When selecting a commander on the left it shows the mvp of the selected commander when matched vs the other commanders." },
            { "DPS", "DPS: Shows the damage delt for each commander based on mineral value killed / game duration (or army value, or minerals collected). When selecting a commander on the left it shows the damage of the selected commander when matched vs the other commanders." },
            { "Synergy", "Synergy: Shows the winrate for the selected commander when played together with the other commanders"},
            { "AntiSynergy", "Antisynergy: Shows the winrate for the selected commander when played vs the other commanders (at any position)"},
            { "Builds", "Builds: Shows the average unit count for the selected commander at the selected game duration. When selecting a vs commander it shows the average unit count of the selected commander when matched vs the other commanders."},
            { "Timeline", "Timeline: Shows the winrate development for the selected commander over the given time period."},
        };

        public static string color_max1 = "Crimson";
        public static string color_max2 = "OrangeRed";
        public static string color_max3 = "Chocolate";
        public static string color_def = "#FFCC00";
        public static string color_info = "#46a2c9";
        public static string color_diff1 = "Crimson";
        public static string color_diff2 = color_info;
        public static string color_null = color_diff2;
        public static string color_bg = "#0e0e24";
        public static string color_plbg_def = "#D8D8D8";
        public static string color_plbg_player = "#2E9AFE";
        public static string color_plbg_mvp = "#FFBF00";


        public static Dictionary<string, string> CMDRcolor { get; } = new Dictionary<string, string>()
        {
            {     "global", "#0000ff"  },
            {     "Abathur", "#266a1b" },
            {     "Alarak", "#ab0f0f" },
            {     "Artanis", "#edae0c" },
            {     "Dehaka", "#d52a38" },
            {     "Fenix", "#fcf32c" },
            {     "Horner", "#ba0d97" },
            {     "Karax", "#1565c7" },
            {     "Kerrigan", "#b021a1" },
            {     "Nova", "#f6f673" },
            {     "Raynor", "#dd7336" },
            {     "Stetmann", "#ebeae8" },
            {     "Stukov", "#663b35" },
            {     "Swann", "#ab4f21" },
            {     "Tychus", "#150d9f" },
            {     "Vorazun", "#07c543" },
            {     "Zagara", "#b01c48" },
            {     "Protoss", "#fcc828"   },
            {     "Terran", "#242331"   },
            {     "Zerg", "#440e5f"   }
        };

        public static string Enddate { get; set; } = DateTime.Today.AddDays(1).ToString("yyyyMMdd");

        public DSdata()
        {
            CultureInfo.CurrentCulture = CultureInfo.CreateSpecificCulture("en-US");


        }

        public static string GetIcon(string race)
        {
            string r = race.ToLower();
            //r = "~/images/btn-unit-hero-" + r + ".png";
            r = "images/btn-unit-hero-" + r + ".png";
            return r;
        }

    }
}



