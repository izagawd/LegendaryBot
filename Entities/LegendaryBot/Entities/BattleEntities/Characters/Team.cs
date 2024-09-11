using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using DatabaseManagement;
using Entities.LegendaryBot.BattleSimulatorStuff;
using Entities.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

#pragma warning disable CS8764 // Nullability of return type doesn't match overridden member (possibly because of nullability attributes).

namespace Entities.LegendaryBot.Entities.BattleEntities.Characters;

public abstract class Team : IList<Character>
{
    [NotMapped] public BattleSimulator? CurrentBattle { get; set; }

    [NotMapped] public abstract int MaxCharacters { get; set; }

    public abstract IEnumerator<Character> GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract void Add(Character item);
    public abstract void Clear();
    public abstract bool Contains(Character item);
    public abstract void CopyTo(Character[] array, int arrayIndex);
    public abstract bool Remove(Character item);
    public abstract int Count { get; }
    public abstract bool IsReadOnly { get; }
    public abstract int IndexOf(Character item);
    public abstract void Insert(int index, Character item);
    public abstract void RemoveAt(int index);
    public abstract Character this[int index] { get; set; }
}

public abstract class Team<TTeamMembership> : Team where TTeamMembership : TeamMembership, new()
{
    public List<TTeamMembership> TeamMemberships { get; set; } = [];


    public bool IsFull => TeamMemberships.Count >= MaxCharacters;

    public override int Count => TeamMemberships.Count;
    public override bool IsReadOnly => false;


    public override Character? this[int slot]
    {
        get => TeamMemberships.FirstOrDefault(i => i.Slot == slot)?.Character;
        set
        {
            if (slot < 1 || slot > MaxCharacters)
                throw new Exception("slot/index exceeds max characters limit");
            var gotten = TeamMemberships.FirstOrDefault(i => i.Slot == slot);

            if (gotten is not null) TeamMemberships.Remove(gotten);
            if (value is not null)
            {
                var gottenSelf = TeamMemberships.FirstOrDefault(i => ReferenceEquals(i.Character, value));
                if (gottenSelf is null)
                {
                    gottenSelf = new TTeamMembership
                    {
                        Character = value
                    };
                    TeamMemberships.Add(gottenSelf);
                }

                gottenSelf.Slot = slot;
            }
        }
    }


    public override IEnumerator<Character> GetEnumerator()
    {
        foreach (var i in TeamMemberships.OrderBy(i => i.Slot)) yield return i.Character;
    }

    public Team LoadTeamStats()
    {
        foreach (var i in TeamMemberships.Select(i => i.Character)) i.LoadStats();

        return this;
    }

    public override void Add(Character item)
    {
        for (var i = 1; i <= MaxCharacters; i++)
            if (TeamMemberships.All(j => j.Slot != i))
            {
                TeamMemberships.Add(new TTeamMembership
                {
                    Character = item,
                    Slot = i
                });
                return;
            }

        throw new Exception("List is full");
    }

    public override void Clear()
    {
        TeamMemberships.Clear();
    }

    public override bool Contains(Character item)
    {
        return TeamMemberships.Any(i => i.Character.Equals(item));
    }

    public override void CopyTo(Character[] array, int arrayIndex)
    {
        var characters = this.ToArray();
        characters.CopyTo(array, arrayIndex);
    }

    public override bool Remove(Character item)
    {
        var membership = TeamMemberships.FirstOrDefault(m => m.Character.Equals(item));
        if (membership is not null)
        {
            TeamMemberships.Remove(membership);
            return true;
        }

        return false;
    }

    public override int IndexOf(Character item)
    {
        return TeamMemberships.FirstOrDefault(i => i.Character.Equals(item))?.Slot ?? -1;
    }

    public override void Insert(int index, Character item)
    {
        this[index] = item;
    }

    public override void RemoveAt(int slot)
    {
        var gotten = TeamMemberships.FirstOrDefault(i => i.Slot == slot);
        if (gotten is not null) TeamMemberships.Remove(gotten);
    }
}