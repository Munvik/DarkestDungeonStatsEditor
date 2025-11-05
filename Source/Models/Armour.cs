namespace DarkestDungeonEditor.Models
{
    public class Armour
    {
        public string Name { get; set; }
        public int Def { get; set; }
        public int Prot { get; set; }
        public int Hp { get; set; }
        public int Spd { get; set; }
        public int? UpgradeRequirementCode { get; set; }
    }
}

