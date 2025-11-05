namespace DarkestDungeonEditor.Models
{
    public class Weapon
    {
        public string Name { get; set; }
        public int Atk { get; set; }
        public int DmgMin { get; set; }
        public int DmgMax { get; set; }
        public int Crit { get; set; }
        public int Spd { get; set; }
        public int? UpgradeRequirementCode { get; set; }
    }
}

