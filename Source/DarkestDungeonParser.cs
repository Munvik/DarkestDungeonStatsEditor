using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using DarkestDungeonEditor.Models;

namespace DarkestDungeonEditor.Services
{
    public static class DarkestDungeonParser
    {
        private static readonly Regex WeaponRegex = new(@"weapon:\s+\.name\s+""(?<name>[^""]+)""\s+\.atk\s+(?<atk>\d+)%\s+\.dmg\s+(?<dmgMin>\d+)\s+(?<dmgMax>\d+)\s+\.crit\s+(?<crit>\d+)%\s+\.spd\s+(?<spd>\d+)(?:\s+\.upgradeRequirementCode\s+(?<upgrade>\d+))?", RegexOptions.Compiled);
        private static readonly Regex ArmourRegex = new(@"armour:\s+\.name\s+""(?<name>[^""]+)""\s+\.def\s+(?<def>\d+)%\s+\.prot\s+(?<prot>\d+)\s+\.hp\s+(?<hp>\d+)\s+\.spd\s+(?<spd>\d+)(?:\s+\.upgradeRequirementCode\s+(?<upgrade>\d+))?", RegexOptions.Compiled);

        public static List<Weapon> ParseWeapons(string text)
        {
            return WeaponRegex.Matches(text).Select(m => new Weapon
            {
                Name = m.Groups["name"].Value,
                Atk = int.Parse(m.Groups["atk"].Value),
                DmgMin = int.Parse(m.Groups["dmgMin"].Value),
                DmgMax = int.Parse(m.Groups["dmgMax"].Value),
                Crit = int.Parse(m.Groups["crit"].Value),
                Spd = int.Parse(m.Groups["spd"].Value),
                UpgradeRequirementCode = m.Groups["upgrade"].Success ? int.Parse(m.Groups["upgrade"].Value) : null
            }).ToList();
        }

        public static List<Armour> ParseArmours(string text)
        {
            return ArmourRegex.Matches(text).Select(m => new Armour
            {
                Name = m.Groups["name"].Value,
                Def = int.Parse(m.Groups["def"].Value),
                Prot = int.Parse(m.Groups["prot"].Value),
                Hp = int.Parse(m.Groups["hp"].Value),
                Spd = int.Parse(m.Groups["spd"].Value),
                UpgradeRequirementCode = m.Groups["upgrade"].Success ? int.Parse(m.Groups["upgrade"].Value) : null
            }).ToList();
        }

        public static string BuildConfig(List<Weapon> weapons, List<Armour> armours)
        {
            var sb = new StringBuilder();

            foreach (var w in weapons)
            {
                sb.AppendLine(
                    $"weapon: .name \"{w.Name}\" .atk {w.Atk}% .dmg {w.DmgMin} {w.DmgMax} .crit {w.Crit}% .spd {w.Spd}" +
                    (w.UpgradeRequirementCode.HasValue ? $" .upgradeRequirementCode {w.UpgradeRequirementCode}" : "")
                );
            }

            foreach (var a in armours)
            {
                sb.AppendLine(
                    $"armour: .name \"{a.Name}\" .def {a.Def}% .prot {a.Prot} .hp {a.Hp} .spd {a.Spd}" +
                    (a.UpgradeRequirementCode.HasValue ? $" .upgradeRequirementCode {a.UpgradeRequirementCode}" : "")
                );
            }

            return sb.ToString();
        }

        /// <summary>
        /// Updates only the weapon: and armour: sections in the file, leaving the rest unchanged.
        /// </summary>
        public static void UpdateWeaponsAndArmoursInFile(string filePath, List<Weapon> weapons, List<Armour> armours)
        {
            var lines = File.ReadAllLines(filePath).ToList();

            // Remove old weapon and armour lines
            lines.RemoveAll(l => l.TrimStart().StartsWith("weapon:") || l.TrimStart().StartsWith("armour:"));

            // Find the insertion point - preferably after resistances/crit lines
            int insertIndex = lines.FindIndex(l => l.StartsWith("resistances:") || l.StartsWith("crit:"));
            if (insertIndex != -1)
                insertIndex += 2;
            else
                insertIndex = 0;

            var weaponLines = weapons.Select(w =>
                $"weapon: .name \"{w.Name}\" .atk {w.Atk}% .dmg {w.DmgMin} {w.DmgMax} .crit {w.Crit}% .spd {w.Spd}" +
                (w.UpgradeRequirementCode.HasValue ? $" .upgradeRequirementCode {w.UpgradeRequirementCode}" : "")
            );

            var armourLines = armours.Select(a =>
                $"armour: .name \"{a.Name}\" .def {a.Def}% .prot {a.Prot} .hp {a.Hp} .spd {a.Spd}" +
                (a.UpgradeRequirementCode.HasValue ? $" .upgradeRequirementCode {a.UpgradeRequirementCode}" : "")
            );

            // Insert new lines
            lines.InsertRange(insertIndex, weaponLines.Concat(armourLines));

            File.WriteAllLines(filePath, lines);
        }
    }
}
