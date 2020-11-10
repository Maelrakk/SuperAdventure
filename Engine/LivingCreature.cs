namespace Engine
{
    public class LivingCreature
    {
        public int CurrentHP { get; set; }
        public int MaximumHP { get; set; }

        public LivingCreature(int currentHP, int maximumHP)
        {
            CurrentHP = currentHP;
            MaximumHP = maximumHP;
        }
    }
}
