using Harmony;
using Microsoft.Xna.Framework;
using StudioForge.Engine.GUI;
using StudioForge.Engine.Integration;
using StudioForge.TotalMiner;
using StudioForge.TotalMiner.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ShopSearch.Patch
{
    [HarmonyPatch]
    public class ShopMenuPatch
    {
        public const string TypeName = "StudioForge.TotalMiner.Screens2.ShopMenu";
        public const string MethodName = "InitMainContainer";

        static NewGuiMenu shopMenuInstance;
        struct SearchMenuData
        {
            public string SearchText;
        }
        static SearchMenuData data;

        [HarmonyPostfix]
        static void Postfix(object __instance)
        {
            try
            {
                if (data.SearchText == null)
                    data.SearchText = "";
                var menu = __instance as NewGuiMenu;
                if (menu == null)
                    return;
                shopMenuInstance = menu;

                Canvas canvas = shopMenuInstance.canvas;
                Window win;
                TextBox tbox;
                DataField df;

                int g = 4;
                var shopInventoryText = canvas.FindChild("Shop Inventory");
                var shopTraverse = Traverse.Create(__instance);
                var invTabPane = shopTraverse.Field("tabsPane");
                var mainWin = invTabPane.Field("mainWin").GetValue<Window>();
                var itemsTab = mainWin.FindChild("itemsTab");
                var itemx = (int)itemsTab.Position.X;
                var itemy = (int)itemsTab.Position.Y;
                int x = itemx + itemsTab.Size.X + g;
                int y = itemy;
                int w = shopInventoryText.Size.X - itemsTab.Size.X * 2 - g;
                int h = itemsTab.Size.Y;
                float scale = 0.6f;

                win = tbox = new TextBox("Search:", x, y, w / 4, h, scale)
                {
                    Name = "searchLabel"
                };
                win.Colors = Colors.LabelLowAlphaColors;
                mainWin.AddChild(win);
                win = tbox = df = new DataField(data.SearchText, x + (w / 4) + 1, y, w / 4 * 3, h, scale)
                {
                    Name = "searchField",
                    TextAlignX = WinTextAlignX.Left
                };
                ((ITextInputWindow)df).OnValidateInput = TextChanged;
                mainWin.AddChild(win);
            } catch (Exception e)
            {
                Logger.Error(e.ToString());
            }
        }

        [HarmonyTargetMethod]
        static MethodInfo CalculateMethod(HarmonyInstance harmony)
        {
            return AccessTools.Method(AccessTools.TypeByName(TypeName), MethodName);
        }

        static void TextChanged(ITextInputWindow win)
        {
            data.SearchText = win.Text;
            PopulateItems(data.SearchText);
        }

        static void PopulateItems(string name)
        {
            var shopMenuTraverse = Traverse.Create(shopMenuInstance);
            var invTabPaneTraverse = shopMenuTraverse.Field("tabsPane");
            var mainWin = invTabPaneTraverse.Field("mainWin").GetValue<Window>();
            var itemsTab = mainWin.FindChild("itemsTab");
            var blocksTab = mainWin.FindChild("blocksTab");
            var tabHighLight = invTabPaneTraverse.Field("tabHighLight").GetValue<Window>();
            var invPaneTraverse = invTabPaneTraverse.Field("invPane");
            var inv = invTabPaneTraverse.Field("inventory").GetValue<ITMInventory>();
            var slotID = invTabPaneTraverse.Field("slotID");
            slotID.SetValue(0);
            invTabPaneTraverse.Field("currentPage").SetValue(0);
            invTabPaneTraverse.Field("pageCountWin").Property("Text").SetValue("1");
            invTabPaneTraverse.Field("morePages").SetValue(false);
            invTabPaneTraverse.Field("pageCountContainerWin").GetValue<Window>().IsVisible = false;
            inv.Clear();
            ItemInvType invType;
            int startIndex;
            if((tabHighLight.Parent as Window).Name == "itemsTab")
            {
                startIndex = Globals1.BlockData.Length;
                invType = (ItemInvType)((byte)(invTabPaneTraverse.Field("itemsTabID").GetValue<int>() + 8));
            } else
            {
                startIndex = 0;
                invType = (ItemInvType)((byte)(invTabPaneTraverse.Field("blocksTabID").GetValue<int>() + 1));
            }
            for (int i = startIndex; i < (int)Globals1.ItemData.Length && slotID.GetValue<int>() < inv.PackSize; i++)
            {
                ItemDataXML itemData = Globals1.ItemData[i];
                if (PopulateItem(itemData) && Globals1.ItemTypeData[i].Inv == invType)
                {
                    if (data.SearchText == "" || itemData.Name.ToLowerInvariant().Contains(data.SearchText.ToLowerInvariant()) || itemData.Name.ToLowerInvariant() == data.SearchText.ToLowerInvariant())
                    {
                        InventoryItem inventoryItem = new InventoryItem(itemData.ItemID, 1);
                        slotID.SetValue(slotID.GetValue<int>() + 1);
                        inv.Items.Add(inventoryItem);
                    }
                }
            }
            invPaneTraverse.Method("RefreshInventoryWindowItems").GetValue();
        }

        static bool PopulateItem(ItemDataXML data)
        {
            if(data.IsValid && data.IsEnabled && !data.HasItemProxy && data.MinCSPrice > 0 && data.ItemID != Item.MobSpawn)
            {
                return true;
            }
            return false;
        } 
    }
}
