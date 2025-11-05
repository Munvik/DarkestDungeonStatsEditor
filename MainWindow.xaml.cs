using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

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
            public double Def { get; set; }
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

        private void LoadFiles_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Multiselect = true,
                Filter = "Darkest Dungeon Info|*.info.darkest"
            };

            if (dlg.ShowDialog() == true)
            {
                foreach (var path in dlg.FileNames)
                {
                    string backupPath = path + ".original";
                    if (!File.Exists(backupPath))
                        File.Copy(path, backupPath);

                    var hero = new HeroFile
                    {
                        Path = path,
                        Lines = File.ReadAllLines(path)
                    };

                    hero.Weapons = ParseWeapons(hero.Lines.ToList());
                    hero.Armours = ParseArmours(hero.Lines.ToList());

                    loadedHeroes.Add(hero);
                }

                HeroesList.ItemsSource = null;
                HeroesList.ItemsSource = loadedHeroes.Select(h => h.Name);
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
            if (HeroesList.SelectedIndex < 0)
            {
                MessageBox.Show("Wybierz postać z listy.");
                return;
            }

            var hero = loadedHeroes[HeroesList.SelectedIndex];
            SaveHeroToFile(hero);
            MessageBox.Show($"Zapisano zmiany dla {hero.Name}");
        }

        private void ApplyPercentAll_Click(object sender, RoutedEventArgs e)
        {
            if (!double.TryParse(PercentBox.Text.Replace("%", ""), out double percent))
            {
                MessageBox.Show("Niepoprawna wartość procentowa!");
                return;
            }

            var targets = ApplyAllHeroesCheck.IsChecked == true ? loadedHeroes : new List<HeroFile>();
            if (ApplyAllHeroesCheck.IsChecked == false && HeroesList.SelectedIndex >= 0)
                targets.Add(loadedHeroes[HeroesList.SelectedIndex]);

            foreach (var hero in targets)
            {
                if (ApplyWeaponsCheck.IsChecked == true)
                {
                    foreach (var w in hero.Weapons)
                    {
                        w.DmgMin = ApplyPercent(w.DmgMin, percent);
                        w.DmgMax = ApplyPercent(w.DmgMax, percent);
                        w.Crit = ApplyPercent(w.Crit, percent);
                        w.Spd = ApplyPercent(w.Spd, percent);
                    }
                }

                if (ApplyArmoursCheck.IsChecked == true)
                {
                    foreach (var a in hero.Armours)
                    {
                        a.Def = ApplyPercent(a.Def, percent);
                        a.Hp = ApplyPercent(a.Hp, percent);
                        a.Spd = ApplyPercent(a.Spd, percent);
                    }
                }

                SaveHeroToFile(hero);
            }

            WeaponsGrid.Items.Refresh();
            ArmoursGrid.Items.Refresh();
            MessageBox.Show("Zastosowano modyfikację procentową.");
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
            // wczytaj aktualne linie pliku (żeby zostawić resztę nietkniętą)
            var lines = File.ReadAllLines(hero.Path).ToList();

            // Zaktualizuj linie weapon
            foreach (var w in hero.Weapons)
            {
                // znajdź indeks linii, która zawiera weapon: oraz nazwe w cudzysłowie
                int idx = lines.FindIndex(l => l.TrimStart().StartsWith("weapon:", StringComparison.OrdinalIgnoreCase)
                                              && l.Contains($"\"{w.Name}\""));
                if (idx >= 0)
                {
                    // zbuduj nową wersję linii - zachowaj inne pola poza tymi co nadpisujemy
                    var old = lines[idx];

                    // aktualizacje poszczególnych pól (jeżeli pole nie istnieje - dodamy je na końcu linii)
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

            // Zaktualizuj linie armour
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

                // jeśli nie ma nazwy, pomijamy
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
                if (mDef.Success) a.Def = ParseDouble(mDef.Groups["def"].Value);

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
            if (HeroesList.SelectedIndex < 0) return;
            var hero = loadedHeroes[HeroesList.SelectedIndex];

            if (!double.TryParse(WeaponPercentBox.Text.Replace("%", ""), out double percent))
            {
                MessageBox.Show("Niepoprawna wartość procentowa!");
                return;
            }

            foreach (var w in hero.Weapons)
            {
                if (ApplyDamageCheck.IsChecked == true)
                {
                    w.DmgMin = ApplyPercent(w.DmgMin, percent);
                    w.DmgMax = ApplyPercent(w.DmgMax, percent);
                }
                if (ApplyCritCheck.IsChecked == true)
                    w.Crit = ApplyPercent(w.Crit, percent);
                if (ApplySpeedCheck.IsChecked == true)
                    w.Spd = ApplyPercent(w.Spd, percent);
            }

            WeaponsGrid.Items.Refresh();
        }

        private void ApplyArmourTabPercent_Click(object sender, RoutedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0) return;
            var hero = loadedHeroes[HeroesList.SelectedIndex];

            if (!double.TryParse(ArmourPercentBox.Text.Replace("%", ""), out double percent))
            {
                MessageBox.Show("Niepoprawna wartość procentowa!");
                return;
            }

            foreach (var a in hero.Armours)
            {
                if (ApplyHpCheck.IsChecked == true)
                    a.Hp = ApplyPercent(a.Hp, percent);
                if (ApplyDefCheck.IsChecked == true)
                    a.Def = ApplyPercent(a.Def, percent);
            }

            ArmoursGrid.Items.Refresh();
        }

        private void ApplyWeaponsPercent_Click(object sender, RoutedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0)
            {
                MessageBox.Show("Wybierz postać z listy.");
                return;
            }

            var hero = loadedHeroes[HeroesList.SelectedIndex];
            if (!double.TryParse(WeaponPercentBox.Text.Replace("%", ""), out double percent))
            {
                MessageBox.Show("Niepoprawna wartość procentowa!");
                return;
            }

            bool fromOriginal = ChkWeaponsFromOriginal.IsChecked == true;
            var baseHero = fromOriginal ? LoadOriginalHero(hero) : hero;

            for (int i = 0; i < hero.Weapons.Count; i++)
            {
                var w = hero.Weapons[i];
                var baseW = baseHero.Weapons.FirstOrDefault(x => x.Name == w.Name);
                if (baseW == null) continue;

                if (ApplyDamageCheck.IsChecked == true)
                {
                    w.DmgMin = ApplyPercent(baseW.DmgMin, percent);
                    w.DmgMax = ApplyPercent(baseW.DmgMax, percent);
                }
                if (ApplyCritCheck.IsChecked == true)
                    w.Crit = ApplyPercent(baseW.Crit, percent);
                if (ApplySpeedCheck.IsChecked == true)
                    w.Spd = ApplyPercent(baseW.Spd, percent);
            }

            WeaponsGrid.Items.Refresh();
        }

        private void ApplyArmoursPercent_Click(object sender, RoutedEventArgs e)
        {
            if (HeroesList.SelectedIndex < 0)
            {
                MessageBox.Show("Wybierz postać z listy.");
                return;
            }

            var hero = loadedHeroes[HeroesList.SelectedIndex];
            if (!double.TryParse(ArmourPercentBox.Text.Replace("%", ""), out double percent))
            {
                MessageBox.Show("Niepoprawna wartość procentowa!");
                return;
            }

            bool fromOriginal = ChkArmoursFromOriginal.IsChecked == true;
            var baseHero = fromOriginal ? LoadOriginalHero(hero) : hero;

            for (int i = 0; i < hero.Armours.Count; i++)
            {
                var a = hero.Armours[i];
                var baseA = baseHero.Armours.FirstOrDefault(x => x.Name == a.Name);
                if (baseA == null) continue;

                if (ApplyHpCheck.IsChecked == true)
                    a.Hp = ApplyPercent(baseA.Hp, percent);
                if (ApplyDefCheck.IsChecked == true)
                    a.Def = ApplyPercent(baseA.Def, percent);
            }

            ArmoursGrid.Items.Refresh();
        }

        private HeroFile LoadOriginalHero(HeroFile hero)
        {
            string originalPath = hero.Path + ".original";
            if (!File.Exists(originalPath))
                return hero; // jeśli nie ma backupu, używamy bieżących danych

            var copy = new HeroFile
            {
                Path = originalPath,
                Lines = File.ReadAllLines(originalPath)
            };

            copy.Weapons = ParseWeapons(copy.Lines.ToList());
            copy.Armours = ParseArmours(copy.Lines.ToList());

            return copy;
        }
    }
}
