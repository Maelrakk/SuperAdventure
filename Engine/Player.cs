﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

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

        private Player(int currentHP, int maximumHP, int gold, int xp)
            : base(currentHP, maximumHP)
        {
            Gold = gold;
            XP = xp;

            Inventory = new List<InventoryItem>();
            Quests = new List<PlayerQuest>();
        }
        
        public static Player CreateDefaultPlayer()
        {
            Player player = new Player(10, 10, 20, 0);
            player.Inventory.Add(new InventoryItem(World.ItemByID(World.ITEM_ID_RUSTY_SWORD), 1));
            player.CurrentLocation = World.LocationByID(World.LOCATION_ID_HOME);

            return player;
        }

        public static Player CreatePlayerFromXmlString(string xmlPlayerData)
        {
            try
            {
                XmlDocument playerData = new XmlDocument();

                playerData.LoadXml(xmlPlayerData);

                int currentHP = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentHP").InnerText);
                int maximumHP = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/MaximumHP").InnerText);
                int gold = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/Gold").InnerText);
                int xp = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/XP").InnerText);

                Player player = new Player(currentHP, maximumHP, gold, xp);

                int currentLocationID = Convert.ToInt32(playerData.SelectSingleNode("/Player/Stats/CurrentLocation").InnerText);
                player.CurrentLocation = World.LocationByID(currentLocationID);

                foreach (XmlNode node in playerData.SelectNodes("/Player/InventoryItems/InventoryItem"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    int quantity = Convert.ToInt32(node.Attributes["Quantity"].Value);

                    for (int i = 0; i < quantity; i++)
                    {
                        player.AddItemToInventory(World.ItemByID(id));
                    }
                }

                foreach (XmlNode node in playerData.SelectNodes("/Player/PlayerQuests/PlayerQuest"))
                {
                    int id = Convert.ToInt32(node.Attributes["ID"].Value);
                    bool isCompleted = Convert.ToBoolean(node.Attributes["IsCompleted"].Value);

                    PlayerQuest playerQuest = new PlayerQuest(World.QuestByID(id));
                    playerQuest.IsCompleted = isCompleted;

                    player.Quests.Add(playerQuest);
                }

                return player;
            }
            catch (Exception)
            {
                //If there was an error with the XML Data, return a default player object
                return Player.CreateDefaultPlayer();
            }
        }

        public string ToXmlString()
        {
            XmlDocument playerData = new XmlDocument();

            //Create the top-level XML node
            XmlNode player = playerData.CreateElement("Player");
            playerData.AppendChild(player);

            //Create "stats" child node
            XmlNode stats = playerData.CreateElement("Stats");
            player.AppendChild(stats);

            //Create child nodes for "Stats" node
            XmlNode currentHP = playerData.CreateElement("CurrentHP");
            currentHP.AppendChild(playerData.CreateTextNode(this.CurrentHP.ToString()));
            stats.AppendChild(currentHP);

            XmlNode maximumHP = playerData.CreateElement("MaximumHP");
            maximumHP.AppendChild(playerData.CreateTextNode(this.MaximumHP.ToString()));
            stats.AppendChild(maximumHP);

            XmlNode gold = playerData.CreateElement("Gold");
            gold.AppendChild(playerData.CreateTextNode(this.Gold.ToString()));
            stats.AppendChild(gold);

            XmlNode XP = playerData.CreateElement("XP");
            XP.AppendChild(playerData.CreateTextNode(this.XP.ToString()));
            stats.AppendChild(XP);

            XmlNode currentLocation = playerData.CreateElement("CurrentLocation");
            currentLocation.AppendChild(playerData.CreateTextNode(this.CurrentLocation.ID.ToString()));
            stats.AppendChild(currentLocation);

            //Create the "InventoryItems" child node to hold each InventoryItem node
            XmlNode inventoryItems = playerData.CreateElement("InventoryItems");
            player.AppendChild(inventoryItems);

            //Create an "InventoryItem" node for each item in the player's inventory
            foreach (InventoryItem ii in this.Inventory)
            {
                XmlNode inventoryItem = playerData.CreateElement("InventoryItem");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = ii.Details.ID.ToString();
                inventoryItem.Attributes.Append(idAttribute);

                XmlAttribute quantityAttribute = playerData.CreateAttribute("Quantity");
                quantityAttribute.Value = ii.Quantity.ToString();
                inventoryItem.Attributes.Append(quantityAttribute);

                inventoryItems.AppendChild(inventoryItem);
            }

            //Create the "PlayerQuets" chiild node to hold individual PlayerQuest nodes
            XmlNode playerQuests = playerData.CreateElement("PlayerQuests");
            player.AppendChild(playerQuests);

            //Create a "PlayerQuest" node for each quest the player has acquired
            foreach (PlayerQuest pq in this.Quests)
            {
                XmlNode playerQuest = playerData.CreateElement("PlayerQuest");

                XmlAttribute idAttribute = playerData.CreateAttribute("ID");
                idAttribute.Value = pq.Details.ID.ToString();
                playerQuest.Attributes.Append(idAttribute);

                XmlAttribute isCompletedAttribute = playerData.CreateAttribute("IsCompleted");
                isCompletedAttribute.Value = pq.IsCompleted.ToString();
                playerQuest.Attributes.Append(isCompletedAttribute);

                playerQuests.AppendChild(playerQuest);
            }

            return playerData.InnerXml; //the XML document, as a string, so we can save data to disk
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
