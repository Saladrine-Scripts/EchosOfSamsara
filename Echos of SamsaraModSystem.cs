using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Config;
using Vintagestory.API.Datastructures;
using Vintagestory.API.Util;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;
using System.Collections;
using System.Security.AccessControl;
using System.Drawing;
using System.Reflection;
using System.Text.RegularExpressions;
using Vintagestory.API.Common.Entities;
//using Vintagestory.GameContent.Mechanics;

namespace Echos_of_Samsara;

public class Echos_of_SamsaraModSystem : ModSystem
{
    /*
    RoadMap:
    v0.0 Set up modding environment.
    v1.0 Save list of items in player inventory by command.
    v1.1 Make a death count variable that persists between worlds.
    v1.2 Increment death count by command.
    v1.3 Have different file names based on death number.
    v1.4 Increment death count on death and refactor code.
    v1.5 Save list of items in player inventory on death.
    
    v2.0 Add a single empty chest to structure list
    v2.1 Spawn items from dead drop list in chest on spawn.
    v2.2 Limit dead drop chest to one spawn per chest.
    v3.0 Make save file with player placed items.
    v3.1 Spawn player made base with loot
    v4.0 Spawn player made bases with structure errosion
    v4.1 Spawn player made bases with Loot errosion
    v5.0 Expand system to work with a multiplayer server.
    v6.0 Add karma system
    v7.0 Add vengful ghost system.
    */
    public String PersistantFile = "EchoData.json";


    //Fixed Items syntax { "Code": "game:knife-generic-tinbronze", "Type": "item", "MinQuantity": 1, "MaxQuantity": 1 },
    public class EchodItem
    {
        public string Code;
        public string Type;
        public int MaxQuantity;
        public int MinQuantity;

    }

    public class DropDeadChestData
    {
        public string ChestID;
        public List<EchodItem> Items = new List<EchodItem>();
    }

    public class EchosOfSamsaraData
    {
        public int DeathCount;
    }

    private ICoreServerAPI _sapi = null!;

    //public override bool ShouldLoad(EnumAppSide forSide) => forSide == EnumAppSide.Server;


    private void OnEntityDeath(Entity entity, DamageSource damageSource)
    {
        if (entity is EntityPlayer entityPlayer)
        {
            OnPlayerDeath((IServerPlayer)entityPlayer.Player);
        }
    }

    private void OnPlayerDeath(IServerPlayer byplayer)
    {
        //Create chest file for the current life
        CreatePlayerEcho(byplayer);
        //Increment death count
        VerifyEchoData();
        EchosOfSamsaraData EchoData = _sapi.LoadModConfig<EchosOfSamsaraData>(PersistantFile);
        EchoData.DeathCount ++;

        _sapi.StoreModConfig(EchoData, PersistantFile);

    }

    public override void StartServerSide(ICoreServerAPI api)
    {
        _sapi = api;
        _sapi.Event.OnEntityDeath += OnEntityDeath;
        Mod.Logger.Notification("Welcome back to the cycle. Leave your mark.");
        _sapi.ChatCommands.Create("CreateEcho")
            .WithDescription("Export inventory into a DeadDropChest.")
            .RequiresPrivilege(Privilege.chat)
            //.WithArgs(api.ChatCommands.Parsers.OptionalWord("arg1"))
            .HandleWith(OnExportEcho);
        _sapi.ChatCommands.Create("CountEchos")
            .WithDescription("Get Deathcount")
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(OnExportDeathCount);
        _sapi.ChatCommands.Create("IncrementDeathCount")
            .WithDescription("Increase Deathcount by 1")
            .RequiresPrivilege(Privilege.chat)
            .HandleWith(OnIncrementDeathCount);
    }

    //"WARNING: PlayerInventoryLib is installed. It replaces the character and backpack inventories, 
    // which Inventory Transfer reads directly — transfers can silently miss or duplicate items with 
    // NO errors in the logs. Remove PlayerInventoryLib before exporting or importing.",
    
    private static void WarnIfPlayerInventoryLib(ICoreAPI api, IServerPlayer player)
    {
        if (!api.ModLoader.IsModEnabled("playerinventorylib")) return;
        player.SendMessage(GlobalConstants.GeneralChatGroup,
            "WARNING: PlayerInventoryLib is installed. It replaces the character and backpack inventories.",
            EnumChatType.Notification);
    }

    private TextCommandResult OnIncrementDeathCount(TextCommandCallingArgs args)
    {
        VerifyEchoData();
        EchosOfSamsaraData EchoData = _sapi.LoadModConfig<EchosOfSamsaraData>(PersistantFile);
        EchoData.DeathCount ++;

        _sapi.StoreModConfig(EchoData, PersistantFile);
        
        return TextCommandResult.Success(
            $"Echo Complete.\n" +
            $"You have an echo count of {EchoData.DeathCount}."
        );
    }

    private void VerifyEchoData()
    {
        //ICoreServerAPI api = args.Caller.Player.Entity.Api as ICoreServerAPI;
        
        //EchosOfSamsaraData loadedData = api.LoadModConfig<EchosOfSamsaraData>(PersistantFile);
        EchosOfSamsaraData loadedData = _sapi.LoadModConfig<EchosOfSamsaraData>(PersistantFile);

        if (loadedData == null)
        {
            //Make Deathcount var
            EchosOfSamsaraData StarterData = new EchosOfSamsaraData();
            //Add deathcount to data
            StarterData.DeathCount = 0;
            //args.Caller.Player.Entity.Api.StoreModConfig(StarterData, PersistantFile);
            _sapi.StoreModConfig(StarterData, PersistantFile);
        }
    }
    private TextCommandResult OnExportDeathCount(TextCommandCallingArgs args)// Update death count
    {
        //ICoreServerAPI api = args.Caller.Player.Entity.Api as ICoreServerAPI;

        VerifyEchoData();
        
        
        EchosOfSamsaraData EchoData = _sapi.LoadModConfig<EchosOfSamsaraData>(PersistantFile);
        
        _sapi.StoreModConfig(EchoData, PersistantFile);
        
        return TextCommandResult.Success(
            $"Echo Complete.\n" +
            $"You have an echo count of {EchoData.DeathCount}."
        );
    }
    private TextCommandResult OnExportEcho(TextCommandCallingArgs args) //Exports inventory save file
    {
        IServerPlayer player = args.Caller.Player as IServerPlayer;
        return CreatePlayerEcho(player);
    }
    private TextCommandResult CreatePlayerEcho(IServerPlayer player)
    {
        
        if(player.WorldData.CurrentGameMode == EnumGameMode.Creative)
        {
            return TextCommandResult.Error("Echo Fades: Creative mode surpasses the limits of reincarnation try again in survival.");
        }

        WarnIfPlayerInventoryLib(player.Entity.Api, player);

        DropDeadChestData DDData = new DropDeadChestData();
                
        int savedCount = 0;

        Stack<ItemSlot> allSlots = new Stack<ItemSlot>();

        // Extract hotbar character and mouse inventories

        string[] targetInvIds = { "hotbar", "character", "mouse"};
        foreach (string Id in targetInvIds)
        {
            IInventory inv = player.InventoryManager.GetOwnInventory(Id);
            if (inv != null)
            {
                foreach(var slot in inv)
                {
                    allSlots.Push(slot);
                }
            }
        }
        // Extract backpacks themselves and not contents.
        IInventory bagHolder = player.InventoryManager.GetOwnInventory("backpack");
        if (bagHolder != null)
        {
            foreach (var slot in bagHolder)
            {
                if (slot.Empty) continue; // Skips empty slots
                if (slot is ItemSlotBagContent) continue; //skips bag contents
                allSlots.Push(slot);
            }
        }

        // Extract bag contents
        int bagContentsExtracted = 0;
        Queue<ItemStack> bagQueue = new Queue<ItemStack>();
        foreach (var slot in allSlots)
        {
            if (!slot.Empty) bagQueue.Enqueue(slot.Itemstack);
        }
        int bagLimit = 20000;
        while (bagQueue.Count > 0 && bagLimit-- > 0)
        {
            ItemStack bagStack = bagQueue.Dequeue();
            IHeldBag heldBag = bagStack?.Collectible?.GetCollectibleInterface<IHeldBag>();
            if (heldBag != null && !heldBag.IsEmpty(bagStack))
            {
                ItemStack[] contents;
                try
                {
                    contents = heldBag.GetContents(bagStack, player.Entity.World);
                }
                catch {continue;}
                if (contents == null) continue;
                foreach (ItemStack contentStack in contents)
                {
                    if (contentStack == null)
                    {
                        continue;
                    }
                    contentStack.ResolveBlockOrItem(player.Entity.World);
                    if (contentStack.Collectible == null)
                    {
                        continue;
                    }
                    allSlots.Push(new DummySlot(contentStack));
                    bagContentsExtracted++;
                    bagQueue.Enqueue(contentStack);//Gets contents of bags in bags.

                }
            }
        }

        //Process Items list
        foreach (var slot in allSlots)
        {
            if (slot.Empty) continue; //Skip empty
            ItemStack stack = slot.Itemstack;
            // remove contents of items like buckets and bowls as the buckets and bowls themselves are kept.
            if (stack.Collectible.Code.Path.Contains("portion"))
            {
                //filteredCount += stack.StackSize;
                continue;
            }
            EchodItem EItem = new EchodItem();
            EItem.Code = stack.Collectible.Code.ToString();
            EItem.Type = (stack.Class == EnumItemClass.Block) ? "Block" : "Item";
            EItem.MaxQuantity = stack.StackSize;
            EItem.MinQuantity = stack.StackSize;

            //Save Item
            DDData.Items.Add(EItem);
            savedCount++;

        }
        //Sort Save Data
        DDData.Items = DDData.Items
            .OrderBy(it => it.Code, StringComparer.Ordinal)
            .ToList();
        
        DDData.ChestID = Guid.NewGuid().ToString("N");


        VerifyEchoData();
        EchosOfSamsaraData EchoData = _sapi.LoadModConfig<EchosOfSamsaraData>(PersistantFile);
        
        string ExportPathPrefix = "DeadDropChest";
        string ExportPathSuffix = ".json";
        string exportFile = ExportPathPrefix + EchoData.DeathCount.ToString() + ExportPathSuffix;
        _sapi.StoreModConfig(DDData, exportFile);
        
        return TextCommandResult.Success(
            $"Echo Complete.\n" +
            $"Saved {savedCount} items."
        );
    }
}
