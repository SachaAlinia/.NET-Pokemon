using System.Collections.Generic;

namespace Game.Core;

/// <summary>
/// Gère l'inventaire du joueur (items, potions, balls, etc.)
/// </summary>
public class PlayerInventory
{
    public class Item
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int Quantity { get; set; }
        public ItemType Type { get; set; }

        public Item(string name, string description, ItemType type, int quantity = 1)
        {
            Name = name;
            Description = description;
            Type = type;
            Quantity = quantity;
        }
    }

    public enum ItemType
    {
        Potion,      // Récupère 20 PV
        SuperPotion, // Récupère 50 PV
        HyperPotion, // Récupère 100 PV
        FullHeal,    // Récupère tous les PV
        Antidote,    // Soigne le poison
        Awakening,   // Soigne le sommeil
        BurnHeal,    // Soigne la brûlure
        IceHeal,     // Soigne la paralysie
        PokeBall,    // Attrape les Pokémon sauvages
    }

    private Dictionary<ItemType, Item> _items = new();

    public PlayerInventory()
    {
        // Inventaire initial
        AddItem(ItemType.Potion, 5);
        AddItem(ItemType.SuperPotion, 2);
        AddItem(ItemType.PokeBall, 10);
    }

    public void AddItem(ItemType type, int quantity = 1)
    {
        string name = GetItemName(type);
        string description = GetItemDescription(type);

        if (_items.ContainsKey(type))
        {
            _items[type].Quantity += quantity;
        }
        else
        {
            _items[type] = new Item(name, description, type, quantity);
        }
    }

    public bool UseItem(ItemType type)
    {
        if (_items.ContainsKey(type) && _items[type].Quantity > 0)
        {
            _items[type].Quantity--;
            if (_items[type].Quantity == 0)
            {
                _items.Remove(type);
            }
            return true;
        }
        return false;
    }

    public int GetItemCount(ItemType type)
    {
        return _items.ContainsKey(type) ? _items[type].Quantity : 0;
    }

    public Dictionary<ItemType, Item> GetAllItems()
    {
        return new Dictionary<ItemType, Item>(_items);
    }

    private string GetItemName(ItemType type)
    {
        return type switch
        {
            ItemType.Potion => "Potion",
            ItemType.SuperPotion => "Super Potion",
            ItemType.HyperPotion => "Hyper Potion",
            ItemType.FullHeal => "Full Heal",
            ItemType.Antidote => "Antidote",
            ItemType.Awakening => "Awakening",
            ItemType.BurnHeal => "Burn Heal",
            ItemType.IceHeal => "Ice Heal",
            ItemType.PokeBall => "Poké Ball",
            _ => "Unknown Item"
        };
    }

    private string GetItemDescription(ItemType type)
    {
        return type switch
        {
            ItemType.Potion => "Récupère 20 PV",
            ItemType.SuperPotion => "Récupère 50 PV",
            ItemType.HyperPotion => "Récupère 100 PV",
            ItemType.FullHeal => "Récupère tous les PV",
            ItemType.Antidote => "Soigne le poison",
            ItemType.Awakening => "Soigne le sommeil",
            ItemType.BurnHeal => "Soigne la brûlure",
            ItemType.IceHeal => "Soigne la paralysie",
            ItemType.PokeBall => "Attrape les Pokémon sauvages",
            _ => "Unknown"
        };
    }
}
