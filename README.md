# ⚔️ Darkest Dungeon Stats Editor

> *From a Darkest Dungeon enjoyer, for Darkest Dungeon enjoyers*

A powerful and intuitive Windows application designed to help you quickly modify hero statistics in Darkest Dungeon. Whether you want to make the game more challenging, easier, or just experiment with different stat configurations, this tool makes it fast and simple to edit all your heroes at once!

## 📸 Preview

![Main Interface](docs/screenshots/main-interface.png)
*The main interface showing the hero list, weapons tab, and global modification options*

![Armor Editing](docs/screenshots/armor-editing.png)
*Armor tab with defense, protection, and HP statistics*

> **Note:** Screenshots showcase the dark-themed interface with Darkest Dungeon-inspired styling, organized data grids for easy stat viewing, and intuitive controls for batch modifications.

## ✨ Features

- 🎯 **Batch Editing** - Modify all heroes simultaneously with a single percentage modifier
- ⚔️ **Weapon Stats** - Edit damage (min/max), critical chance, and speed
- 🛡️ **Armor Stats** - Adjust defense, protection, HP, and speed
- 📁 **Flexible Loading** - Load individual files or entire hero folders
- 💾 **Safe Modifications** - Automatic backup creation before any changes
- 🔄 **Easy Reset** - Restore original values anytime from automatic backups
- 🎨 **Dark Theme UI** - Beautiful Darkest Dungeon-inspired interface
- 📊 **Visual Editing** - See all stats in organized data grids
- ⚡ **Percentage Modifiers** - Apply percentage-based changes globally or per-tab
- 🎯 **Selective Application** - Choose which stats to modify with checkboxes

## 🚀 Getting Started

### Prerequisites

- **Windows OS** (Windows 10/11 recommended)
- **.NET 8.0 Runtime** or later ([Download here](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Darkest Dungeon** game installed

### Installation

1. **Download the latest release** from the [Releases page](https://github.com/Munvik/DarkestDungeonStatsEditor/releases)
2. **Extract the files** to a folder of your choice
3. **Run `DDStatsMod.exe`** to launch the application

### Building from Source

If you want to build the application yourself:

```bash
# Clone the repository
git clone https://github.com/Munvik/DarkestDungeonStatsEditor.git

# Navigate to the project directory
cd DarkestDungeonStatsEditor

# Build the project
dotnet build

# Run the application
dotnet run
```

## 📖 How to Use

### Step 1: Locate Your Hero Files

Your Darkest Dungeon hero files are typically located at:
```
Steam\steamapps\common\DarkestDungeon\heroes\[hero_name]\[hero_name].info.darkest
```

**Example paths:**
- `...\heroes\abomination\abomination.info.darkest`
- `...\heroes\crusader\crusader.info.darkest`
- `...\heroes\highwayman\highwayman.info.darkest`

### Step 2: Load Hero Files

Choose one of two methods:

**Method A: Load Individual Files**
1. Click **"📂 Wczytaj pliki"** (Load Files)
2. Select one or more `.info.darkest` files
3. The application automatically creates `.original` backup files

**Method B: Load Entire Folder**
1. Click **"Wczytaj folder postaci"** (Load Hero Folder)
2. Select your heroes folder (e.g., `...\DarkestDungeon\heroes\`)
3. All hero files will be loaded automatically

### Step 3: Modify Stats

**Individual Hero Editing:**
1. Select a hero from the list on the left
2. Switch between **Weapons** and **Armour** tabs
3. Directly edit values in the data grids
4. Or use tab-specific percentage modifiers

**Global Modifications:**
1. Enter a percentage in the **"Modyfikacja globalna (%)"** box (e.g., 25 for +25%)
2. Check which categories to modify:
   - ✅ Zastosuj do broni (Apply to weapons)
   - ✅ Zastosuj do zbroi (Apply to armor)
   - ✅ Zastosuj do wszystkich postaci (Apply to all heroes)
3. Choose whether to apply from original values or current values
4. Click **"Zastosuj %"** (Apply %)

### Step 4: Save Changes

1. Click **"💾 Zapisz zmiany"** (Save Changes)
2. All modifications are written to the game files
3. Original values are safely stored in `.original` files

### Step 5: Reset if Needed

If you want to undo changes:
1. Click **"🔄 Resetuj"** (Reset)
2. Choose to reset current hero or all heroes
3. Original values are restored in memory
4. Click Save to write the reset to files

## 💡 Tips & Best Practices

- **Always keep backups** - The tool creates `.original` files automatically, but keep additional backups of your `heroes` folder
- **Test changes gradually** - Start with small percentage modifications (10-25%) before making drastic changes
- **Use "From Original Values"** - Check this option to ensure consistent results when applying multiple modifications
- **Reset before major changes** - Use the reset feature to start fresh if you're experimenting
- **Close the game** - Make sure Darkest Dungeon is closed before modifying files
- **Verify changes** - After saving, load a game to verify your modifications work as expected

## 🎮 Common Use Cases

### Making Heroes Stronger
Set global modifier to +25% or +50% and apply to all heroes for an easier gameplay experience.

### Balancing Challenge
Reduce hero stats by -10% or -20% for a more challenging run.

### Custom Builds
Manually edit specific heroes to create unique party compositions (e.g., glass cannon damage dealers, tanky supports).

### Speed Runs
Increase speed stats across all heroes to control turn order more effectively.

## 🛠️ Technical Details

- **Framework:** .NET 8.0 with WPF
- **Language:** C#
- **UI:** Windows Presentation Foundation (WPF)
- **File Format:** `.info.darkest` text-based configuration files
- **Backup Strategy:** Automatic `.original` file creation on first load

### Supported Stats

**Weapons:**
- Damage Min/Max
- Critical Hit Chance
- Speed

**Armour:**
- Defense (Dodge)
- Protection
- Hit Points
- Speed

## ⚠️ Disclaimer

This is a modding tool for single-player use. Using modified game files in any online or competitive mode may violate terms of service. Always keep backups and use responsibly!

## 🤝 Contributing

Contributions are welcome! Feel free to:
- Report bugs or issues
- Suggest new features
- Submit pull requests
- Share your feedback

## 📝 License

This project is provided as-is for the Darkest Dungeon community.

## 🌟 Acknowledgments

Created with passion by a Darkest Dungeon fan for the community. May the Light guide your path through the darkness!

---

*"Many fall in the face of chaos, but not this one... not today."*
