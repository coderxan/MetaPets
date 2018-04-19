using System;

using Server;
using Server.Mobiles;
using Server.Network;

namespace Server.Menus.ItemLists
{
    public class PreAOSCraftGump : ItemListMenu
    {
        private Mobile m_Mobile;            // Declare Mobile: Who Can Use The Crafting Tool
        private string m_Menu;              // Declare A Crafting Menu To A Craft Tool Item
        private ItemListEntry[] m_Entries;  // Declare Crafting Entries To Your Crafting Menu

        public PreAOSCraftGump(Mobile mobile, ItemListEntry[] entries, string menu)
            : base("  - Select An Item To Craft -  ", entries)
        {
            m_Mobile = mobile;              // Assign A Variable To Your m_Mobile Declaration
            m_Menu = menu;                  // Assing A Variable To Your m_Menu Declaration
            m_Entries = entries;            // Assign A Variable To Your m_Entries Declaration
        }

        public static ItemListEntry[] Main(Mobile from)         // 'Main' Is The Craft Gump Menu Identifier. It Is Used To Call The Craft Gump.
        {
            ItemListEntry[] entries = new ItemListEntry[7];     // How Many Item Categories Will We Have In Our Craft Gump?

            /// <summary>
            /// The text strings in quotes below denote the category type a player can 
            /// select! This text will appear at the bottom of the gump when a player 
            /// hovers the mouse cursor over the object category they wish to craft.
            /// 
            /// The numbers after the text strings are the item id's of each picture
            /// denoting the item category a player might choose to select. This is
            /// strictly for aesthetics, but is mandatory for this type of menu gump.
            /// 
            /// Notice that their are 7 entries below if you count 'entries[0]' and that
            /// number '7' is also declared above 'ItemListEntry[7]'. If you don't have 
            /// this number set correctly then your server will crash.
            /// </summary>

            /// Tested Without Scrolling Operational
            entries[0] = new ItemListEntry("Repair", 4015);     // Test Entry 1
            entries[1] = new ItemListEntry("Shields", 7026);    // Test Entry 2
            entries[2] = new ItemListEntry("Armor", 5141);      // Test Entry 3
            entries[3] = new ItemListEntry("Weapons", 5049);    // Test Entry 4
            entries[4] = new ItemListEntry("a horse", 8479);    // Test Entry 5

            /// Tested With The Scroll Bar Working
            entries[5] = new ItemListEntry("Test1", 7026);      // Test Entry 6
            entries[6] = new ItemListEntry("Test2", 5141);      // Test Entry 7

            return entries; // Makes Sure That The Entries Above Appear On The Craft Menu Gump
        }

        public override void OnResponse(NetState state, int index)
        {
            /// <summary>
            /// The 'Main' craft gump menu identifier from above is also typed below:
            /// 
            /// This identifier is what you'll call in the OnDoubleClick method of
            /// the crafting tool you wish to assign the craft menu gump to. The 
            /// entire method to be placed in the crafting tool script will look like 
            /// the following and call the craft menu gump whenever it is used:
            /// 
            /// public override void OnDoubleClick(Mobile from)
            /// {
            ///    from.SendMenu(new PreAOSCraftGump(from, PreAOSCraftGump.Main(from), "Main"));
            /// }
            /// </summary>

            if (m_Menu == "Main")
            {
                /// Test Entry 1: Function Code When Category Is Selected
                if (m_Entries[index].Name == "Repair")
                {
                    m_Mobile.SendAsciiMessage("a repair function is called from here!");
                    // Repair.Do(m_Mobile, DefBlacksmithy.CraftSystem, m_Tool);
                }

                /// Test Entry 2: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "Shields")
                {
                    m_Mobile.SendAsciiMessage("Crafting Shields!");
                }

                /// Test Entry 3: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "Armor")
                {
                    m_Mobile.SendAsciiMessage("Crafting Armor!");
                }

                /// Test Entry 4: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "Weapons")
                {
                    m_Mobile.SendAsciiMessage("Crafting Weapons!");
                }

                /// Test Entry 5: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "a horse")
                {
                    m_Mobile.SendAsciiMessage("You have spawned a Horse!");
                    //new Horse().MoveToWorld(m_Mobile.Location, Map.Felucca);
                }

                /// Test Entry 6: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "Test1")
                {
                    m_Mobile.SendAsciiMessage("Crafting Shields!");
                }

                /// Test Entry 7: Function Code When Category Is Selected
                else if (m_Entries[index].Name == "Test2")
                {
                    m_Mobile.SendAsciiMessage("Crafting Armor!");
                }
            }
        }
    }
}