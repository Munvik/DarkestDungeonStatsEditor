using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace DDStatsMod
{
    public partial class MainWindow : Window
    {
        private class Weapon
        {
            public string Name { get; set; }
            public int DmgMin { get; set; }
            public int DmgMax { get; set; }
            public int Crit { get; set; }
            public int Spd { get; set; }
            public string RawLine { get; set; }
        }

        private class Armour
        {
            public string Name { get; set; }
            public int Def { get; set; }
            public int Prot { get; set; }
            public int Hp { get; set; }
            public int Spd { get; set; }
            public string RawLine { get; set; }
        }

        private class HeroFile
        {
            public string Path { get; set; }
            public string Name => System.IO.Path.GetFileNameWithoutExtension(Path);
            public string[] Lines { get; set; }
            public List<Weapon> Weapons { get; set; } = new();
            public List<Armour> Armours { get; set; } = new();
        }

        private readonly List<HeroFile> loadedHeroes = new();

        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Checks if a file contains hero data by looking for weapon or armour lines.
        /// Returns true if the file is a hero file, false if it's a monster or other file.
        /// </summary>
        private bool IsHeroFile(string[] lines)
        {
            foreach (var line in lines)
            {
                var trimmed = line.TrimStart();
                if (trimmed.StartsWith("weapon:", StringComparison.OrdinalIgnoreCase) ||
                    trimmed.StartsWith("armour:", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        private void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new System.Windows.Forms.OpenFileDialog
            {
                Multiselect = true,
                Filter = "Darkest Dungeon Info|*.info.darkest"
            };

            if (dlg.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                int loadedCount = 0;
                int skippedCount = 0;
                int errorCount = 0;

                foreach (var path in dlg.FileNames)
                {
                    try
                    {
                        var lines = File.ReadAllLines(path);

                        // Check if this is a hero file (has weapon or armour data)
                        if (!IsHeroFile(lines))
                        {
                            skippedCount++;
                            continue;
                        }

                        string backupPath = path + ".original";
                        if (!File.Exists(backupPath))
                            File.Copy(path, backupPath);

                        var hero = new HeroFile
                        {
                            Path = path,
                            Lines = lines
                        };

                        hero.Weapons = ParseWeapons(hero.Lines.ToList());
                        hero.Armours = ParseArmours(hero.Lines.ToList());

                        loadedHeroes.Add(hero);
                        loadedCount++;
                    }
                    catch (Exception ex)
                    {
                        errorCount++;
                        System.Windows.MessageBox.Show(
                            $"Error loading file:\n{System.IO.Path.GetFileName(path)}\n\n{ex.Message}",
                            "File Load Error",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }

                HeroesList.ItemsSource = null;
                HeroesList.ItemsSource = loadedHeroes.Select(h => h.Name);

                if (skippedCount > 0 || errorCount > 0)
                {
                    var message = $"Loaded {loadedCount} hero file(s).";
                    if (skippedCount > 0)
                        message += $"\nSkipped {skippedCount} non-hero file(s) (monsters or other data).";
                    if (errorCount > 0)
                        message += $"\nFailed to load {errorCount} file(s) due to errors.";

                    System.Windows.MessageBox.Show(
                        message,
                        "Load Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }

        private void LoadFolderButton_Click(object sender, RoutedEventArgs e)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                dialog.Description = "Select folder with .info.darkest files";
                dialog.UseDescriptionForTitle = true;

                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    string selectedFolder = dialog.SelectedPath;
                    string[] files = Directory.GetFiles(selectedFolder, "*.info.darkest", SearchOption.AllDirectories);

                    int loadedCount = 0;
                    int skippedCount = 0;
                    int errorCount = 0;

                    foreach (var path in files)
                    {
                        try
                        {
                            var lines = File.ReadAllLines(path);

                            // Check if this is a hero file (has weapon or armour data)
                            if (!IsHeroFile(lines))
                            {
                                skippedCount++;
                                continue;
                            }

                            string backupPath = path + ".original";
                            if (!File.Exists(backupPath))
                                File.Copy(path, backupPath);

                            var hero = new HeroFile
                            {
                                Path = path,
                                Lines = lines
                            };

                            hero.Weapons = ParseWeapons(hero.Lines.ToList());
                            hero.Armours = ParseArmours(hero.Lines.ToList());

                            loadedHeroes.Add(hero);
                            loadedCount++;
                        }
                        catch (Exception ex)
                        {
                            errorCount++;
                            // Don't show individual error messages in folder mode to avoid spam
                            // Aggregate errors will be shown in the summary
                        }
                    }

                    HeroesList.ItemsSource = null;
                    HeroesList.ItemsSource = loadedHeroes.Select(h => h.Name);

                    var message = $"Loaded {loadedCount} hero file(s) from folder:\n{selectedFolder}";
                    if (skippedCount > 0)
                        message += $"\n\nSkipped {skippedCount} non-hero file(s) (monsters or other data).";
                    if (errorCount > 0)
                        message += $"\nFailed to load {errorCount} file(s) due to errors.";

                    System.Windows.MessageBox.Show(
                        message,
                        "Load Complete",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
        }


        private void HeroesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0) return;

            var hero = loadedHeroes[HeroesList.SelectedIndex];
            WeaponsGrid.ItemsSource = hero.Weapons;
            ArmoursGrid.ItemsSource = hero.Armours;
        }

        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            if (loadedHeroes.Count == 0)
            {
                System.Windows.MessageBox.Show("No characters loaded to save.");
                return;
            }

            int savedCount = 0;

            foreach (var hero in loadedHeroes)
            {
                SaveHeroToFile(hero);
                savedCount++;
            }

            System.Windows.MessageBox.Show(
                $"Saved changes for all ({savedCount}) characters.",
                "Save completed",
                MessageBoxButton.OK,
                MessageBoxImage.Information
            );
        }


        private void ApplyPercentAll_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(PercentBox.Text.Replace("%", ""), out double percent))
            {
                System.Windows.MessageBox.Show("Invalid percentage value!");
                return;
            }

            bool applyAll = ApplyAllHeroesCheck.IsChecked == true;
            bool fromOriginal = ChkGlobalFromOriginal != null && ChkGlobalFromOriginal.IsChecked == true;

            var targets = applyAll ? loadedHeroes : new List<HeroFile>();
            if (!applyAll && HeroesList.SelectedIndex >= 0)
                targets.Add(loadedHeroes[HeroesList.SelectedIndex]);

            foreach (var hero in targets)
            {
                var baseHero = fromOriginal ? LoadOriginalHero(hero) : hero;

                if (ApplyWeaponsCheck.IsChecked == true)
                {
                    var updatedWeapons = new List<Weapon>();

                    foreach (var baseW in baseHero.Weapons)
                    {
                        var newW = new Weapon
                        {
                            Name = baseW.Name,
                            DmgMin = ApplyPercent(baseW.DmgMin, percent),
                            DmgMax = ApplyPercent(baseW.DmgMax, percent),
                            Crit = ApplyPercent(baseW.Crit, percent),
                            Spd = ApplyPercent(baseW.Spd, percent),
                            RawLine = baseW.RawLine
                        };

                        updatedWeapons.Add(newW);
                    }

                    hero.Weapons = updatedWeapons;
                }

                if (ApplyArmoursCheck.IsChecked == true)
                {
                    var updatedArmours = new List<Armour>();

                    foreach (var baseA in baseHero.Armours)
                    {
                        var newA = new Armour
                        {
                            Name = baseA.Name,
                            Def = ApplyPercent(baseA.Def, percent),
                            Prot = ApplyPercent(baseA.Prot, percent),
                            Hp = ApplyPercent(baseA.Hp, percent),
                            Spd = ApplyPercent(baseA.Spd, percent),
                            RawLine = baseA.RawLine
                        };

                        updatedArmours.Add(newA);
                    }

                    hero.Armours = updatedArmours;
                }
            }

            // Refresh currently displayed hero in DataGrids
            if (HeroesList.SelectedIndex >= 0)
            {
                var currentHero = loadedHeroes[HeroesList.SelectedIndex];
                WeaponsGrid.ItemsSource = null;
                WeaponsGrid.ItemsSource = currentHero.Weapons;
                ArmoursGrid.ItemsSource = null;
                ArmoursGrid.ItemsSource = currentHero.Armours;
            }

            System.Windows.MessageBox.Show(
                "Applied percentage modification (without saving to files).\nTo save changes, click 'Save changes'.",
                "Changes applied",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private static int ApplyPercent(int val, double percent)
        {
            return (int)Math.Round(val + val * (percent / 100.0));
        }

        private static double ApplyPercent(double val, double percent)
        {
            return Math.Round(val + val * (percent / 100.0), 1);
        }

        private void SaveHeroToFile(HeroFile hero)
        {
            // Read current file lines (to leave the rest unchanged)
            var lines = File.ReadAllLines(hero.Path).ToList();

            // Update weapon lines
            foreach (var w in hero.Weapons)
            {
                // Find the line index that contains weapon: and the name in quotes
                int idx = lines.FindIndex(l => l.TrimStart().StartsWith("weapon:", StringComparison.OrdinalIgnoreCase)
                                              && l.Contains($"\"{w.Name}\""));
                if (idx >= 0)
                {
                    var old = lines[idx];

                    // Update individual fields
                    string updated = old;

                    if (Regex.IsMatch(updated, @"\.dmg\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.dmg\s+(-?\d+\.?\d*)\s+(-?\d+\.?\d*)", $".dmg {w.DmgMin} {w.DmgMax}");
                    else
                        updated += $" .dmg {w.DmgMin} {w.DmgMax}";

                    if (Regex.IsMatch(updated, @"\.crit\s+(-?\d+\.?\d*)%?", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.crit\s+(-?\d+\.?\d*)%?", $".crit {w.Crit}%");
                    else
                        updated += $" .crit {w.Crit}%";

                    if (Regex.IsMatch(updated, @"\.spd\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.spd\s+(-?\d+\.?\d*)", $".spd {w.Spd}");
                    else
                        updated += $" .spd {w.Spd}";

                    lines[idx] = updated;
                    w.RawLine = updated;
                }
            }

            // Update armour lines
            foreach (var a in hero.Armours)
            {
                int idx = lines.FindIndex(l => l.TrimStart().StartsWith("armour:", StringComparison.OrdinalIgnoreCase)
                                              && l.Contains($"\"{a.Name}\""));
                if (idx >= 0)
                {
                    var old = lines[idx];
                    string updated = old;

                    if (Regex.IsMatch(updated, @"\.def\s+(-?\d+\.?\d*)%?", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.def\s+(-?\d+\.?\d*)%?", $".def {a.Def:0.#}%");
                    else
                        updated += $" .def {a.Def:0.#}%";

                    if (Regex.IsMatch(updated, @"\.prot\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.prot\s+(-?\d+\.?\d*)", $".prot {a.Prot}");
                    else
                        updated += $" .prot {a.Prot}";

                    if (Regex.IsMatch(updated, @"\.hp\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.hp\s+(-?\d+\.?\d*)", $".hp {a.Hp}");
                    else
                        updated += $" .hp {a.Hp}";

                    if (Regex.IsMatch(updated, @"\.spd\s+(-?\d+\.?\d*)", RegexOptions.IgnoreCase))
                        updated = Regex.Replace(updated, @"\.spd\s+(-?\d+\.?\d*)", $".spd {a.Spd}");
                    else
                        updated += $" .spd {a.Spd}";

                    lines[idx] = updated;
                    a.RawLine = updated;
                }
            }

            File.WriteAllLines(hero.Path, lines);
            hero.Lines = lines.ToArray();
        }



        private List<Weapon> ParseWeapons(List<string> lines)
        {
            var result = new List<Weapon>();
            var lineRegex = new Regex(@"^\s*weapon:\s", RegexOptions.IgnoreCase);

            var nameRegex = new Regex(@"\.name\s+""([^""]+)""", RegexOptions.IgnoreCase);
            var dmgRegex = new Regex(@"\.dmg\s+([-\d\.]+)\s+([-\d\.]+)", RegexOptions.IgnoreCase);
            var critRegex = new Regex(@"\.crit\s+([-\d\.]+)%?", RegexOptions.IgnoreCase);
            var spdRegex = new Regex(@"\.spd\s+([-\d\.]+)", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                if (!lineRegex.IsMatch(line)) continue;

                var w = new Weapon { RawLine = line };

                var nameMatch = nameRegex.Match(line);
                if (nameMatch.Success)
                    w.Name = nameMatch.Groups[1].Value.Trim();

                var dmgMatch = dmgRegex.Match(line);
                if (dmgMatch.Success)
                {
                    w.DmgMin = ParseInt(dmgMatch.Groups[1].Value);
                    w.DmgMax = ParseInt(dmgMatch.Groups[2].Value);
                }

                var critMatch = critRegex.Match(line);
                if (critMatch.Success)
                    w.Crit = ParseInt(critMatch.Groups[1].Value);

                var spdMatch = spdRegex.Match(line);
                if (spdMatch.Success)
                    w.Spd = ParseInt(spdMatch.Groups[1].Value);

                // Skip if no name
                if (!string.IsNullOrEmpty(w.Name))
                    result.Add(w);
            }

            return result;
        }



        private List<Armour> ParseArmours(List<string> lines)
        {
            var result = new List<Armour>();
            var lineRegex = new Regex(@"^\s*armour:\s", RegexOptions.IgnoreCase);

            var nameRegex = new Regex(@"\.name\s+""(?<name>[^""]+)""", RegexOptions.IgnoreCase);
            var defRegex = new Regex(@"\.def\s+(?<def>-?\d+\.?\d*)%?", RegexOptions.IgnoreCase);
            var protRegex = new Regex(@"\.prot\s+(?<prot>-?\d+\.?\d*)", RegexOptions.IgnoreCase);
            var hpRegex = new Regex(@"\.hp\s+(?<hp>-?\d+\.?\d*)", RegexOptions.IgnoreCase);
            var spdRegex = new Regex(@"\.spd\s+(?<spd>-?\d+\.?\d*)", RegexOptions.IgnoreCase);

            foreach (var line in lines)
            {
                if (!lineRegex.IsMatch(line)) continue;

                var a = new Armour { RawLine = line };

                var mName = nameRegex.Match(line);
                if (mName.Success) a.Name = mName.Groups["name"].Value;

                var mDef = defRegex.Match(line);
                if (mDef.Success) a.Def = ParseInt(mDef.Groups["def"].Value);

                var mProt = protRegex.Match(line);
                if (mProt.Success) a.Prot = ParseInt(mProt.Groups["prot"].Value);

                var mHp = hpRegex.Match(line);
                if (mHp.Success) a.Hp = ParseInt(mHp.Groups["hp"].Value);

                var mSpd = spdRegex.Match(line);
                if (mSpd.Success) a.Spd = ParseInt(mSpd.Groups["spd"].Value);

                if (string.IsNullOrEmpty(a.Name)) continue;

                result.Add(a);
            }

            return result;
        }


        private int ParseInt(string input)
        {
            if (int.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out int val))
                return val;
            if (double.TryParse(input, NumberStyles.Any, CultureInfo.InvariantCulture, out double d))
                return (int)Math.Round(d);
            return 0;
        }


        private static double ParseDouble(string s)
        {
            return double.TryParse(s, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double val) ? val : 0.0;
        }

        private void ApplyWeaponTabPercent_Click(object sender, RoutedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0)
            {
                System.Windows.MessageBox.Show("Select a character from the list.");
                return;
            }

            var hero = loadedHeroes[HeroesList.SelectedIndex];

            if (!double.TryParse(WeaponPercentBox.Text.Replace("%", ""), out double percent))
            {
                System.Windows.MessageBox.Show("Invalid percentage value!");
                return;
            }

            bool fromOriginal = ChkWeaponsFromOriginal.IsChecked == true;
            List<Weapon> baseWeapons;

            // If "From original value" checkbox is checked
            if (fromOriginal)
            {
                string originalPath = hero.Path + ".original";
                if (File.Exists(originalPath))
                {
                    var originalLines = File.ReadAllLines(originalPath).ToList();
                    baseWeapons = ParseWeapons(originalLines);
                }
                else
                {
                    System.Windows.MessageBox.Show("Original file (.original) not found. Using current values.");
                    baseWeapons = hero.Weapons.Select(w => CloneWeapon(w)).ToList();
                }
            }
            else
            {
                baseWeapons = hero.Weapons.Select(w => CloneWeapon(w)).ToList();
            }

            // Create new result list
            var updatedWeapons = new List<Weapon>();

            foreach (var baseW in baseWeapons)
            {
                var newW = CloneWeapon(baseW);

                if (ApplyDamageCheck.IsChecked == true)
                {
                    newW.DmgMin = ApplyPercent(baseW.DmgMin, percent);
                    newW.DmgMax = ApplyPercent(baseW.DmgMax, percent);
                }
                if (ApplyCritCheck.IsChecked == true)
                    newW.Crit = ApplyPercent(baseW.Crit, percent);
                if (ApplySpeedCheck.IsChecked == true)
                    newW.Spd = ApplyPercent(baseW.Spd, percent);

                updatedWeapons.Add(newW);
            }

            // Overwrite weapons in current hero
            hero.Weapons = updatedWeapons;

            // Refresh UI
            WeaponsGrid.ItemsSource = null;
            WeaponsGrid.ItemsSource = hero.Weapons;
            WeaponsGrid.Items.Refresh();
        }

        private Weapon CloneWeapon(Weapon w)
        {
            return new Weapon
            {
                Name = w.Name,
                DmgMin = w.DmgMin,
                DmgMax = w.DmgMax,
                Crit = w.Crit,
                Spd = w.Spd,
                RawLine = w.RawLine
            };
        }


        private void ApplyArmourTabPercent_Click(object sender, RoutedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0)
            {
                System.Windows.MessageBox.Show("Select a character from the list.");
                return;
            }

            var hero = loadedHeroes[HeroesList.SelectedIndex];

            if (!double.TryParse(ArmourPercentBox.Text.Replace("%", ""), out double percent))
            {
                System.Windows.MessageBox.Show("Invalid percentage value!");
                return;
            }

            bool fromOriginal = ChkArmoursFromOriginal.IsChecked == true;
            List<Armour> baseArmours;

            // If "From original value" is checked
            if (fromOriginal)
            {
                string originalPath = hero.Path + ".original";
                if (File.Exists(originalPath))
                {
                    var originalLines = File.ReadAllLines(originalPath).ToList();
                    baseArmours = ParseArmours(originalLines);
                }
                else
                {
                    System.Windows.MessageBox.Show("Original file (.original) not found. Using current values.");
                    baseArmours = hero.Armours.Select(a => CloneArmour(a)).ToList();
                }
            }
            else
            {
                baseArmours = hero.Armours.Select(a => CloneArmour(a)).ToList();
            }

            // Create new list with calculated values
            var updatedArmours = new List<Armour>();

            foreach (var baseA in baseArmours)
            {
                var newA = CloneArmour(baseA);

                if (ApplyHpCheck.IsChecked == true)
                    newA.Hp = ApplyPercent(baseA.Hp, percent);
                if (ApplyDefCheck.IsChecked == true)
                    newA.Def = ApplyPercent(baseA.Def, percent);

                updatedArmours.Add(newA);
            }

            // Overwrite armours in current hero
            hero.Armours = updatedArmours;

            // Refresh UI
            ArmoursGrid.ItemsSource = null;
            ArmoursGrid.ItemsSource = hero.Armours;
            ArmoursGrid.Items.Refresh();
        }

        private Armour CloneArmour(Armour a)
        {
            return new Armour
            {
                Name = a.Name,
                Def = a.Def,
                Prot = a.Prot,
                Hp = a.Hp,
                Spd = a.Spd,
                RawLine = a.RawLine
            };
        }

        private HeroFile LoadOriginalHero(HeroFile hero)
        {
            string originalPath = hero.Path + ".original";
            if (!File.Exists(originalPath))
                return hero;

            var copy = new HeroFile
            {
                Path = originalPath,
                Lines = File.ReadAllLines(originalPath)
            };

            copy.Weapons = ParseWeapons(copy.Lines.ToList());
            copy.Armours = ParseArmours(copy.Lines.ToList());

            return copy;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            if (loadedHeroes.Count == 0)
            {
                System.Windows.MessageBox.Show("No characters loaded to reset.");
                return;
            }

            bool resetAll = ResetAllCheck != null && ResetAllCheck.IsChecked == true;

            if (resetAll)
            {
                foreach (var hero in loadedHeroes)
                {
                    ResetHeroInMemory(hero);
                }

                RefreshView();
                System.Windows.MessageBox.Show(
                    "All characters have been restored to original values in memory.",
                    "Reset completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }
            else
            {
                if (HeroesList.SelectedIndex < 0)
                {
                    System.Windows.MessageBox.Show(
                        "No character selected for reset.",
                        "No selection",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                var hero = loadedHeroes[HeroesList.SelectedIndex];
                ResetHeroInMemory(hero);
                RefreshView();
                System.Windows.MessageBox.Show(
                    $"Restored original values for character: {hero.Name} in memory.",
                    "Reset completed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            }

            void RefreshView()
            {
                // Refresh view after reset
                if (HeroesList.SelectedIndex >= 0)
                {
                    var currentHero = loadedHeroes[HeroesList.SelectedIndex];
                    WeaponsGrid.ItemsSource = null;
                    WeaponsGrid.ItemsSource = currentHero.Weapons;
                    ArmoursGrid.ItemsSource = null;
                    ArmoursGrid.ItemsSource = currentHero.Armours;
                    WeaponsGrid.Items.Refresh();
                    ArmoursGrid.Items.Refresh();
                }
                else
                {
                    WeaponsGrid.ItemsSource = null;
                    ArmoursGrid.ItemsSource = null;
                }
            }
        }

        // Method to reset only in memory
        private void ResetHeroInMemory(HeroFile hero)
        {
            string originalPath = hero.Path + ".original";
            if (!File.Exists(originalPath))
            {
                System.Windows.MessageBox.Show($"No original backup found for {hero.Name} ({originalPath})");
                return;
            }

            // Read original data
            var originalLines = File.ReadAllLines(originalPath).ToList();
            hero.Lines = originalLines.ToArray();
            hero.Weapons = ParseWeapons(originalLines);
            hero.Armours = ParseArmours(originalLines);

            // Do NOT save to file, changes are only in memory
        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            var helpWindow = new HelpWindow
            {
                Owner = this
            };
            helpWindow.ShowDialog();
        }

    }
}
