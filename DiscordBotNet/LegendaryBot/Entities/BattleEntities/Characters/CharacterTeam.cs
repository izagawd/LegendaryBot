using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters.CharacterPartials;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class CharacterTeam : ISet<Character>
{

    /// <summary>
    /// If the owner of the team is a real person, this should be their discord id
    /// </summary>
    public ulong? TryGetDiscordId => (this as PlayerTeam)?.UserData.DiscordId;

    public bool IsPlayerTeam => TryGetDiscordId is not null;

    /// <summary>
    /// increases exp of every character in the team and returns useful text
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public string IncreaseExp(int exp)
    {
        exp /= Count;
        var text = "";
        foreach (var i in this)
        {
            text += i.IncreaseExp(exp) + "\n";
        }

        return text;
    }

    [NotMapped]
    public BattleSimulator CurrentBattle { get; set; }

    public HashSet<Character> Characters { get; set; } = [];
    public IEnumerator<Character> GetEnumerator()
    {
        return Characters.GetEnumerator();
    }
 


    public CharacterTeam LoadTeamStats()
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            i.LoadStats();
        }

        return this;

    }


    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }



        /// <param name="character"></param>

        /// <returns></returns>

    public virtual bool Add(Character character)
    {
        character.Team = this;
        return Characters.Add(character);
    }

    public void AddRange(IEnumerable<Character> characters)
    {
        
        
        foreach (var i in characters)
        {
            Add(i);
        }
    }
    public void ExceptWith(IEnumerable<Character> other)
    {
        Characters.ExceptWith(other);
    }

    public void IntersectWith(IEnumerable<Character> other)
    {
        Characters.IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<Character> other)
    {
        return Characters.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<Character> other)
    {
        return Characters.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<Character> other)
    {
        return Characters.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<Character> other)
    {
        return Characters.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<Character> other)
    {
        return Characters.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<Character> other)
    {
        return Characters.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<Character> other)
    {
        Characters.SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<Character> other)
    {
        Characters.UnionWith(other);
    }


    void ICollection<Character>.Add(Character item)
    {
        Add(item);
    }

    public void Clear()
    {
        Characters.Clear();
    }

    public CharacterTeam(params Character[] characters)
    {

        Characters = characters.ToHashSet();
        foreach (var i in Characters)
        {
            i.Team = this;
        }
    }


    public CharacterTeam(IEnumerable<Character> characters) : this(characters.ToArray())
    {
        
    }


    public bool Contains(Character character)
    {
        return Characters.Contains(character);
    }

    public void CopyTo(Character[] array, int arrayIndex)
    {
        Characters.CopyTo(array,arrayIndex);
    }

    public bool Remove(Character character)
    {
        return Characters.Remove(character);
    }

    public int Count => Characters.Count;
    public bool IsReadOnly { get; }
}