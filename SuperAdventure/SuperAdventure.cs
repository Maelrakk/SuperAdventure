using Engine;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace SuperAdventure
{
    public partial class SuperAdventure : Form
    {
        private Player _player;
        private Monster _currentMonster;
        private const string PLAYER_DATA_FILE_NAME = "PlayerData.xml";

        public SuperAdventure()
        {
            InitializeComponent();

            if (File.Exists(PLAYER_DATA_FILE_NAME))
            {
                _player = Player.CreatePlayerFromXmlString(File.ReadAllText(PLAYER_DATA_FILE_NAME));
            }
            else
            {
                _player = Player.CreateDefaultPlayer();
            }

            MoveTo(_player.CurrentLocation);
            UpdatePlayerStats();
        }

        private void btnNorth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToNorth);
        }

        private void btnSouth_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToSouth);
        }

        private void btnEast_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToEast);
        }

        private void btnWest_Click(object sender, EventArgs e)
        {
            MoveTo(_player.CurrentLocation.LocationToWest);
        }

        private void MoveTo(Location newLocation)
        {
            //Does location have required Items
            if (!_player.HasRequiredItemToEnterThisLocation(newLocation))
            {
                rtbMessages.Text += "You must have a " + newLocation.ItemRequiredToEnter.Name + " to enter this location." + Environment.NewLine;
                ScrollToBottom();

                return;
            }

            //Update Player location
            _player.CurrentLocation = newLocation;

            //Show available movements
            btnNorth.Visible = (newLocation.LocationToNorth != null);
            btnSouth.Visible = (newLocation.LocationToSouth != null);
            btnEast.Visible = (newLocation.LocationToEast != null);
            btnWest.Visible = (newLocation.LocationToWest != null);

            //Describe player's current location
            rtbLocation.Text = newLocation.Name + Environment.NewLine;
            rtbLocation.Text += newLocation.Description + Environment.NewLine;

            //Heal Player
            _player.CurrentHP = _player.MaximumHP;

            UpdatePlayerStats();

            //Does location have quest
            if (newLocation.QuestAvailableHere != null)
            {
                //Location has quest
                bool playerAlreadyHasQuest = _player.HasThisQuest(newLocation.QuestAvailableHere);
                bool playerAlreadyCompletedQuest = _player.CompletedThisQuest(newLocation.QuestAvailableHere);

                //if player has quest already
                if (playerAlreadyHasQuest)
                {
                    //If player has not completed quest upon entering location, check to see if quest can be completed.
                    if(!playerAlreadyCompletedQuest)
                    {
                        //Check if player has all items needed for completion
                        bool playerHasAllItemsToCompleteQuest = _player.HasAllQuestCompletionItems(newLocation.QuestAvailableHere);
                        
                        //Player has all items required
                        if (playerHasAllItemsToCompleteQuest)
                        {
                            //Display Message
                            rtbMessages.Text += Environment.NewLine;
                            rtbMessages.Text += "You complete the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                            ScrollToBottom();

                            //Remove quest items from inventory
                            _player.RemoveQuestCompletionItems(newLocation.QuestAvailableHere);

                            //Give quest rewards
                            rtbMessages.Text += "You receive: " + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardXP.ToString() + " XP" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardGold.ToString() + " Gold" + Environment.NewLine;
                            rtbMessages.Text += newLocation.QuestAvailableHere.RewardItem.Name + Environment.NewLine;
                            rtbMessages.Text += Environment.NewLine;
                            ScrollToBottom();

                            _player.XP += newLocation.QuestAvailableHere.RewardXP;
                            _player.Gold += newLocation.QuestAvailableHere.RewardGold;

                            //Add Reward Item to inventory
                            _player.AddItemToInventory(newLocation.QuestAvailableHere.RewardItem);

                            //Mark Quest as completed
                            _player.MarkQuestCompleted(newLocation.QuestAvailableHere);

                            UpdatePlayerStats();
                        }
                    }
                }

                //Player does not have quest
                else
                {
                    //Diplay messages
                    rtbMessages.Text += "You receive the " + newLocation.QuestAvailableHere.Name + " quest." + Environment.NewLine;
                    rtbMessages.Text += newLocation.QuestAvailableHere.Description + Environment.NewLine;
                    rtbMessages.Text += "To complete the quest, return here with: " + Environment.NewLine;
                    
                    foreach(QuestCompletionItem qci in newLocation.QuestAvailableHere.QuestCompletionItems)
                    {
                        if (qci.Quantity == 1)
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.Name + Environment.NewLine;
                        }
                        else
                        {
                            rtbMessages.Text += qci.Quantity.ToString() + " " + qci.Details.NamePlural + Environment.NewLine;
                        }
                    }

                    rtbMessages.Text += Environment.NewLine;
                    ScrollToBottom();

                    //Add quest to _player.Quests
                    _player.Quests.Add(new PlayerQuest(newLocation.QuestAvailableHere));
                }
            }

            //Does Location have a monster?
            if (newLocation.MonsterLivingHere != null)
            {
                rtbMessages.Text += "You see a " + newLocation.MonsterLivingHere.Name + Environment.NewLine;
                ScrollToBottom();

                //Make a new monster, using values from standard monster in World.cs
                Monster standardMonster = World.MonsterByID(newLocation.MonsterLivingHere.ID);

                _currentMonster = new Monster(standardMonster.ID, standardMonster.Name, standardMonster.MaximumDamage, standardMonster.RewardXP, standardMonster.RewardGold, standardMonster.CurrentHP, standardMonster.MaximumHP);

                foreach (LootItem lootItem in standardMonster.LootTable)
                {
                    _currentMonster.LootTable.Add(lootItem);
                }

                cboWeapons.Visible = true;
                cboPotions.Visible = true;
                btnUseWeapon.Visible = true;
                btnUsePotion.Visible = true;
            }
            else
            {
                _currentMonster = null;

                cboWeapons.Visible = false;
                cboPotions.Visible = false;
                btnUseWeapon.Visible = false;
                btnUsePotion.Visible = false;
            }

            //Refresh Player's inventory list
            UpdateInventoryListInUI();

            //Refresh Player's quest list
            UpdateQuestListInUI();

            //Refresh Player's weapons comboBox
            UpdateWeaponListInUI();

            //Refresh Player's potions comboBox
            UpdatePotionListInUI();
        }

        private void UpdatePlayerStats()
        {
            lblHP.Text = _player.CurrentHP.ToString() + "/" + _player.MaximumHP.ToString();
            lblGold.Text = _player.Gold.ToString();
            lblXP.Text = _player.XP.ToString();
            lblLevel.Text = _player.Level.ToString();
        }

        private void UpdateInventoryListInUI()
        {
            dgvInventory.RowHeadersVisible = false;

            dgvInventory.ColumnCount = 2;
            dgvInventory.Columns[0].Name = "Name";
            dgvInventory.Columns[0].Width = 197;
            dgvInventory.Columns[1].Name = "Quantity";

            dgvInventory.Rows.Clear();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Quantity > 0)
                {
                    dgvInventory.Rows.Add(new[] { 
                        ii.Details.Name, 
                        ii.Quantity.ToString() });
                }
            }
        }

        private void UpdateQuestListInUI()
        {
            dgvQuests.RowHeadersVisible = false;

            dgvQuests.ColumnCount = 2;
            dgvQuests.Columns[0].Name = "Name";
            dgvQuests.Columns[0].Width = 197;
            dgvQuests.Columns[1].Name = "Done?";

            dgvQuests.Rows.Clear();

            foreach (PlayerQuest pq in _player.Quests)
            {
                dgvQuests.Rows.Add(new[] { 
                    pq.Details.Name, 
                    pq.IsCompleted.ToString() });
            }
        }

        private void UpdateWeaponListInUI()
        {
            List<Weapon> weapons = new List<Weapon>();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details is Weapon)
                {
                    if (ii.Quantity > 0)
                    {
                        weapons.Add((Weapon)ii.Details);
                    }
                }
            }

            if (weapons.Count == 0)
            {
                //Player has no weapons, so we should hide the combobox and Use button
                cboWeapons.Visible = false;
                btnUseWeapon.Visible = false;
            }
            else
            {
                cboWeapons.DataSource = weapons;
                cboWeapons.DisplayMember = "Name";
                cboWeapons.ValueMember = "ID";

                cboWeapons.SelectedIndex = 0;
            }
        }

        private void UpdatePotionListInUI()
        {
            List<HealingPotion> healingPotions = new List<HealingPotion>();

            foreach (InventoryItem ii in _player.Inventory)
            {
                if (ii.Details is HealingPotion)
                {
                    if (ii.Quantity > 0)
                    {
                        healingPotions.Add((HealingPotion)ii.Details);
                    }
                }
            }

            if (healingPotions.Count == 0)
            {
                //Player has no potions, hide combobox and use button
                cboPotions.Visible = false;
                btnUsePotion.Visible = false;
            }
            else
            {
                cboPotions.DataSource = healingPotions;
                cboPotions.DisplayMember = "Name";
                cboPotions.ValueMember = "ID";

                cboPotions.SelectedIndex = 0;
            }
        }

        private void WeaponDamageCalc()
        {
            //Get Current weapon from combobox
            Weapon currentWeapon = (Weapon)cboWeapons.SelectedItem;

            //Calculate Damage
            int damageToMonster = ComplexRNG.NumberBetween(currentWeapon.MinimumDamage, currentWeapon.MaximumDamage);

            //Apply Damage to Monster's current HP
            _currentMonster.CurrentHP -= damageToMonster;

            //Display message
            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "You hit the " + _currentMonster.Name + " for " + damageToMonster.ToString() + " points." + Environment.NewLine;
            ScrollToBottom();

        }

        private void MonsterDamageCalc(Monster monster)
        {
            //Determine monster Damage
            int damageToPlayer = ComplexRNG.NumberBetween(0, monster.MaximumDamage);

            //Display message
            rtbMessages.Text += monster.Name + " dealt " + damageToPlayer.ToString() + " points of damage." + Environment.NewLine;
            ScrollToBottom();

            //Subtract damage from player
            _player.CurrentHP -= damageToPlayer;

            //Refresh player data UI
            UpdatePlayerStats();
        }

        private void MonsterRewards(Monster monster)
        {
            //Monster is dead
            rtbMessages.Text += Environment.NewLine;
            rtbMessages.Text += "You defeated the " + monster.Name + Environment.NewLine;

            //RewardXP
            _player.XP += monster.RewardXP;
            rtbMessages.Text += "You receive " + monster.RewardXP.ToString() + " XP." + Environment.NewLine;

            //RewardGold
            _player.Gold += monster.RewardGold;
            rtbMessages.Text += "You receive " + monster.RewardGold.ToString() + " gold." + Environment.NewLine;
            ScrollToBottom();


            //Get random loot from monster - create list to hold new items
            List<InventoryItem> lootedItems = new List<InventoryItem>();

            //Add lootTable items to list
            foreach (LootItem loot in monster.LootTable)
            {
                if (ComplexRNG.NumberBetween(1, 100) <= loot.DropPercentage)
                {
                    lootedItems.Add(new InventoryItem(loot.Details, 1));
                }
            }

            //no items randomly selected
            if (lootedItems.Count == 0)
            {
                foreach (LootItem loot in monster.LootTable)
                {
                    if (loot.IsDefaultItem)
                    {
                        lootedItems.Add(new InventoryItem(loot.Details, 1));
                    }
                }
            }

            foreach (InventoryItem ii in lootedItems)
            {
                _player.AddItemToInventory(ii.Details);

                if (ii.Quantity == 1)
                {
                    rtbMessages.Text += "You loot " + ii.Quantity.ToString() + " " + ii.Details.Name + Environment.NewLine;
                    ScrollToBottom();

                }
                else
                {
                    rtbMessages.Text += "You loot " + ii.Quantity.ToString() + " " + ii.Details.NamePlural + Environment.NewLine;
                    ScrollToBottom();

                }
            }
        }

        private void btnUseWeapon_Click(object sender, EventArgs e)
        {
            WeaponDamageCalc();

            //Check if monster died
            if (_currentMonster.CurrentHP <= 0)
            {
                MonsterRewards(_currentMonster);

                //Refresh player info and inventory
                UpdatePlayerStats();

                UpdateInventoryListInUI();
                UpdateWeaponListInUI();
                UpdatePotionListInUI();

                //add a blank line to messages box
                rtbMessages.Text += Environment.NewLine;
                ScrollToBottom();

                //move player to current location (i.e. refresh current location)
                MoveTo(_player.CurrentLocation);
            }
            else
            {
                //Monster still alive
                MonsterDamageCalc(_currentMonster);
                
                if (_player.CurrentHP <= 0)
                {
                    //Display message
                    rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;
                    rtbMessages.Text += Environment.NewLine;
                    ScrollToBottom();

                    //MoveTo(home)
                    MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
                }
            }

        }


        private void btnUsePotion_Click(object sender, EventArgs e)
        {
            //Get currently selected potion from combobox
            HealingPotion potion = (HealingPotion)cboPotions.SelectedItem;

            //Add amount to heal to Player HP
            _player.CurrentHP += potion.AmountToHeal;

            //CurrentHP cannot exceed Max
            if (_player.CurrentHP > _player.MaximumHP)
            {
                _player.CurrentHP = _player.MaximumHP;
            }

            //Remove a potion from inventory
            foreach(InventoryItem ii in _player.Inventory)
            {
                if(ii.Details.ID == potion.ID)
                {
                    ii.Quantity--;
                    break;
                }
            }

            //Display Message
            rtbMessages.Text += "You drink a " + potion.Name + Environment.NewLine;
            ScrollToBottom();

            //Monster Turn
            MonsterDamageCalc(_currentMonster);
            
            if (_player.CurrentHP <= 0)
            {
                //Display message
                rtbMessages.Text += "The " + _currentMonster.Name + " killed you." + Environment.NewLine;
                rtbMessages.Text += Environment.NewLine;
                ScrollToBottom();

                //MoveTo(home)
                MoveTo(World.LocationByID(World.LOCATION_ID_HOME));
            }

            //Refresh player info and inventory
            UpdatePlayerStats();
            UpdateInventoryListInUI();
            UpdatePotionListInUI();
        }

        private void ScrollToBottom()
        {
            rtbMessages.SelectionStart = rtbMessages.Text.Length;
            rtbMessages.ScrollToCaret();
        }

        private void SuperAdventure_FormClosing(object sender, FormClosingEventArgs e)
        {
            File.WriteAllText(PLAYER_DATA_FILE_NAME, _player.ToXmlString());
        }
    }
}
