using System.Collections.Generic;

namespace DarkestDungeonEditor.Models
{
    public class CharacterData
    {
        public string CharacterName { get; set; }
        public string FilePath { get; set; }

        public List<Weapon> Weapons { get; set; } = new();
        public List<Armour> Armours { get; set; } = new();

        // Reference copies (originals)
        public List<Weapon> OriginalWeapons { get; set; } = new();
        public List<Armour> OriginalArmours { get; set; } = new();
    }
}
