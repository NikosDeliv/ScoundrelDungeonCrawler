using System;
using System.Collections.Generic;
using System.Linq;

class ScoundrelGame
{
    static Random rng = new Random();
    static int playerHealth = 20;
    static List<Card> deck = new List<Card>();
    static Card carriedCard = null;
    static Card equippedWeapon = null;
    static int deferredRoomCount = 0;
    const int maxDeferredRooms = 2;

    static void Main()
    {
        Console.WriteLine("Welcome to the Scoundrel Dungeon!");
        InitializeDeck();
        ShuffleDeck();

        while (deck.Count >= 3 && playerHealth > 0)
        {
            PlayRoom();
        }

        Console.WriteLine(playerHealth > 0 ? "Congratulations! You cleared the dungeon and escaped!" : "It seems you couldn't escape after all...");
    }

    static void InitializeDeck()
    {
        string[] suits = { "♠", "♣", "♦", "♥" };

        for (int value = 2; value <= 14; value++)
        {
            foreach (string suit in suits)
            {
                if ((suit == "♥" || suit == "♦") && (value >= 11 || value == 14))
                    continue; // Remove red face cards & aces

                deck.Add(new Card(suit, value));
            }
        }
    }

    static void ShuffleDeck()
    {
        deck = deck.OrderBy(_ => rng.Next()).ToList();
    }

    static void PlayRoom()
    {
        Console.WriteLine("\n--- Dungeon Room ---");

        List<Card> roomCards = new List<Card>();

        if (carriedCard != null)
        {
            roomCards.Add(carriedCard);
            carriedCard = null;
        }

        while (roomCards.Count < 4 && deck.Count > 0)
        {
            roomCards.Add(deck[0]);
            deck.RemoveAt(0);
        }

        for (int i = 0; i < roomCards.Count; i++)
        {
            Console.WriteLine($"{i + 1}) {roomCards[i]} - {roomCards[i].GetCardType()}");
        }

        DisplayPlayerStatus();

        if (deferredRoomCount < maxDeferredRooms)
        {
            Console.Write("Do you want to defer this room? (y/n): ");
            string input;
            while (true)
            {
                input = Console.ReadLine()?.ToLower();
                if (input == "y" || input == "n")
                    break;
                Console.Write("Invalid input! Please enter 'y' or 'n': ");
            }

            if (input == "y")
            {
                deferredRoomCount++;
                deck.AddRange(roomCards);
                ShuffleDeck();
                return;
            }
        }
        else
        {
            Console.WriteLine("\nYou've skipped too many rooms! You must face this one.");
        }

        deferredRoomCount = 0;
        List<Card> resolvedCards = new List<Card>();

        while (resolvedCards.Count < 3 && playerHealth > 0)
        {
            Console.Write("\nChoose a card to resolve (1-4): ");
            if (int.TryParse(Console.ReadLine(), out int choice) && choice >= 1 && choice <= 4)
            {
                Card chosenCard = roomCards[choice - 1];
                if (resolvedCards.Contains(chosenCard))
                {
                    Console.WriteLine("You already resolved that card!");
                    continue;
                }

                ResolveCard(chosenCard);
                resolvedCards.Add(chosenCard);
            }
            else
            {
                Console.WriteLine("Invalid choice! Try again.");
            }
        }

        carriedCard = roomCards.Except(resolvedCards).First();
        Console.WriteLine($"The last card ({carriedCard}) carries to the next room.");
    }

    static void DisplayPlayerStatus()
    {
        Console.WriteLine($"\n--- Player Status ---");
        Console.WriteLine($"Health: {playerHealth}");
        if (equippedWeapon != null)
        {
            Console.WriteLine($"Weapon: {equippedWeapon} (Attack: {equippedWeapon.Value})");
        }
        else
        {
            Console.WriteLine("Weapon: None");
        }
    }

    static void ResolveCard(Card card)
    {
        if (card.Suit == "♦") // Weapon
        {
            equippedWeapon = card;
            Console.WriteLine($"You equipped a weapon: {card} (Attack: {card.Value})");
        }
        else if (card.Suit == "♥") // Potion
        {
            int healAmount = card.Value;
            int maxHeal = 20 - playerHealth;
            int actualHeal = Math.Min(healAmount, maxHeal);
            playerHealth += actualHeal;
            Console.WriteLine($"You drank a potion! Restored {actualHeal} HP. Current HP: {playerHealth}");
        }
        else if (card.Suit == "♠" || card.Suit == "♣") // Monster
        {
            HandleCombat(card);
        }
    }

    static void HandleCombat(Card monster)
    {
        if (equippedWeapon != null)
        {
            if (monster.Value < equippedWeapon.Value)
            {
                Console.WriteLine($"You attacked {monster} with {equippedWeapon} and won!");
            }
            else
            {
                Console.WriteLine($"Your {equippedWeapon} is too weak! You must fight barehanded.");
                playerHealth -= monster.Value;
            }
        }
        else
        {
            Console.WriteLine($"You fight {monster} barehanded!");
            playerHealth -= monster.Value;
        }

        Console.WriteLine($"Current HP: {playerHealth}");
    }
}

class Card
{
    public string Suit { get; }
    public int Value { get; }

    public Card(string suit, int value)
    {
        Suit = suit;
        Value = value;
    }

    public override string ToString()
    {
        string displayValue = Value switch
        {
            11 => "J",
            12 => "Q",
            13 => "K",
            14 => "A",
            _ => Value.ToString()
        };
        return $"{Suit} {displayValue}";
    }

    public string GetCardType()
    {
        return Suit switch
        {
            "♠" or "♣" => "Monster",
            "♦" => "Weapon",
            "♥" => "Potion",
            _ => "Unknown"
        };
    }
}