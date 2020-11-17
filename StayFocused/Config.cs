using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StayFocused
{
    enum Modes
    {
        Blacklist,
        Whitelist
    }

    class Config
    {
        private static readonly string[] DEFAULT_CONFIG = {
            "# valid modes: blacklist, whitelist",
            "[config]",
            "mode = whitelist",
            "",
            "# hook ONLY these programs, when mode = whitelist",
            "[whitelist]",
            "Photoshop.exe",
            "",
            "# hook ALL programs except the following, when mode = blacklist",
            "[blacklist]",
            "explorer.exe",
            "svchost.exe",
            "taskhostw.exe",
            "RuntimeBroker.exe",
            "SearchUI.exe",
            "ShellExperienceHost.exe",
            "chrome.exe",
            "conhost.exe",
        };

        public Modes mode = Modes.Whitelist;
        public HashSet<string> whitelist;
        public HashSet<string> blacklist;

        public Config() {
            var filename = GetConfigPath();
            if (!File.Exists(filename)) {
                Directory.CreateDirectory(Path.GetDirectoryName(filename));
                File.WriteAllLines(filename, DEFAULT_CONFIG);
            }

            var config = File.ReadAllLines(filename);
            whitelist = new HashSet<string>();
            blacklist = new HashSet<string>();
            ParseConfig(config);
        }

        public string GetConfigPath() {
            return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StayFocused.ini");
        }

        void ParseConfig(string[] lines) {
            string section = "config";

            foreach (var lineRaw in lines) {
                string line = lineRaw.Trim().ToLowerInvariant();

                if (line.StartsWith("#") || line.Length == 0) {
                    // comment line - skip
                    continue;
                }

                if (line.StartsWith("[")) {
                    // section line - change section
                    section = line.Substring(1, line.Length - 2);
                    continue;
                }

                if (section == "config") {
                    if (line.StartsWith("mode = ")) {
                        var parsedMode = line.Substring(7);
                        if (parsedMode == "blacklist") mode = Modes.Blacklist;
                    }
                    continue;
                }

                if (section == "whitelist") {
                    whitelist.Add(line);
                    continue;
                }

                if (section == "blacklist") {
                    blacklist.Add(line);
                    continue;
                }
            }

            if (mode == Modes.Blacklist) {
                Form1.Log("Config loaded • mode = blacklist • blacklisted exes:");
                Form1.Log(String.Join(", ", blacklist));
            } else {
                Form1.Log("Config loaded • mode = whitelist • whitelisted exes:");
                Form1.Log(String.Join(", ", whitelist));
            }
            Form1.Log("---");
        }
    }
}
