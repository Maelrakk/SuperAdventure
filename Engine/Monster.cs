using System.Collections.Generic;

namespace Engine
{
    public class Monster : LivingCreature
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public int MaximumDamage { get; set; }
        public int RewardXP { get; set; }
        public int RewardGold { get; set; }
        public List<LootItem> LootTable { get; set; }

        public Monster(int id, string name, int maximumDamage, int rewardXP, int rewardGold, int currentHP, int maximumHP)
            : base(currentHP, maximumHP)
        {
            ID = id;
            Name = name;
            MaximumDamage = maximumDamage;
            RewardXP = rewardXP;
            RewardGold = rewardGold;

            LootTable = new List<LootItem>();
        }
    }
}
