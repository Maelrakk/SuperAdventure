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
        public int Level { get; set; }
        public Location CurrentLocation { get; set; }
        public List<InventoryItem> Inventory { get; set; }
        public List<PlayerQuest> Quests { get; set; }

        public Player(int currentHP, int maximumHP, int gold, int xp, int level)
            : base(currentHP, maximumHP)
        {
            Gold = gold;
            XP = xp;
            Level = level;

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

            foreach (InventoryItem ii in Inventory)
            {
                if (ii.Details.ID == location.ItemRequiredToEnter.ID)
                {
                    //We found the require item, so return "true"
                    return true;
                }
            }
            //didn't find require item, so return "false"
            return false;
        }

        public bool HasThisQuest(Quest quest)
        {
            foreach (PlayerQuest pq in Quests)
            {
                if(pq.Details.ID == quest.ID)
                {
                    return true;
                }
            }

            return false;
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
            //Compare Location List<Quest> Items to Player List<PlayerQuests> Items
            foreach (QuestCompletionItem qci in quest.QuestCompletionItems)
            {
                bool foundItemInPlayersInventory = false;

                //Check each item in player inventory for matching ID
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID == qci.Details.ID)
                    {
                        foundItemInPlayersInventory = true;

                        //if item.ID is in player Inventory, then check to see quantity matches quest requirement
                        if (ii.Quantity < qci.Quantity)
                        {
                            //Player doesn't have enough of the item to complete
                            return false;
                        }
                    }
                }

                //If we didn't find the required item, we set variable and stop looking
                if (!foundItemInPlayersInventory)
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
                foreach (InventoryItem ii in Inventory)
                {
                    if (ii.Details.ID == qci.Details.ID)
                    {
                        //Subtract quantity required by quest completion from player inventory
                        ii.Quantity -= qci.Quantity;
                        break;
                    }
                }
            }
        }

        public void AddItemToInventory(Item itemToAdd)
        {
            foreach (InventoryItem ii in Inventory)
            {
                //check for the item in the inventory already
                if (ii.Details.ID == itemToAdd.ID)
                {
                    //item in inventory already, so just add quantity
                    ii.Quantity++;
                    
                    return; //we added the item, so we leave the function before the next block runs
                }
            }
            //They didn't have the item, so add it to their inventory
            Inventory.Add(new InventoryItem(itemToAdd, 1));
        }

        public void MarkQuestCompleted(Quest quest)
        {
            //Find quest in player List<PlayerQuests> Quests
            foreach (PlayerQuest pq in Quests)
            {
                if (pq.Details.ID == quest.ID)
                {
                    //mark completed in _player.Quests
                    pq.IsCompleted = true;

                    return;
                }
            }
        }
    }
}
