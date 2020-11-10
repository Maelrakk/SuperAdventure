using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;

namespace Engine
{
    public class Player : LivingCreature
    {
        public int Gold { get; set; }
        public int XP { get; set; }
        public int Level 
        {
            get { return ((XP / 100) + 1); }
        }
        public Location CurrentLocation { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        public Player(int currentHP, int maximumHP, int gold, int xp)
            : base(currentHP, maximumHP)
        {
            Gold = gold;
            XP = xp;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }

        public bool HasRequiredItemToEnterThisLocation(Location location)
        {
            if (location.ItemRequiredToEnter == null)
            {
                //There is no required item for this location, so return "true"
                return true;
            }

            return Inventory.Exists(ii => ii.Details.ID == location.ItemRequiredToEnter.ID);
        }

        public bool HasThisQuest(Quest quest)
        {
            return Quests.Exists(pq => pq.Details.ID == quest.ID);
        }

        public bool CompletedThisQuest(Quest quest)
        {
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    return pq.IsCompleted;
                }
            }

            return false;
        }

        public bool HasAllQuestCompletionItems(Quest quest)
        {
            //Query list of QuestCompletionItems and compare items to condition below
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                //Cheack that the quest completion item currently looping is in the Player Inventory and whether they have enough of it
                if(!Inventory.Exists(ii => ii.Details.ID == qci.Details.ID && ii.Quantity >= qci.Quantity))
                {
                    return false;
                }
            
            }
            
            //If we got here, then the player must have all the required items, and enough of them, to complete the quest
            return true;
        }

        public void RemoveQuestCompletionItems(Quest quest)
        {
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == qci.Details.ID);

                if (item != null)
                {
                    //Subtract quantity required by quest completion from player inventory
                    item.Quantity -= qci.Quantity;
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            InventoryItem item = Inventory.SingleOrDefault(ii => ii.Details.ID == itemToAdd.ID);

            if (item == null)
            {
                //Inventory.SingleOrDefault returns null, so there is no InventoryItem that == itemToAdd.ID
                //Add Item to Inventory by storing to new InventoryItem in list
                Inventory.Add(new InventoryItem(itemToAdd, 1));
            }
            else
            {
                //The have the item in the inventory, so we just add quantity
                item.Quantity++;
            }
        }

        public void MarkQuestCompleted(Quest quest)
        {
            //Find quest in player List<PlayerQuests> Quests
            PlayerQuest playerQuest = Quests.SingleOrDefault(pq => pq.Details.ID == quest.ID);

            if (playerQuest != null)
            {
                playerQuest.IsCompleted = true;
            }
        }
    }
}
