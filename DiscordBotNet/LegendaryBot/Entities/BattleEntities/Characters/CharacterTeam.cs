using System.Collections;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;
using DiscordBotNet.LegendaryBot.BattleSimulatorStuff;
using DSharpPlus.Entities;

namespace DiscordBotNet.LegendaryBot.Entities.BattleEntities.Characters;

public class CharacterTeam : ISet<CharacterPartials.Character>
{

    /// <summary>
    /// If the owner of the team is a real person, this should be their discord id
    /// </summary>
    public long? TryGetUserDataId => (this as PlayerTeam)?.UserDataId;

    public bool IsPlayerTeam => TryGetUserDataId is not null;

    /// <summary>
    /// increases exp of every character in the team and returns useful text
    /// </summary>
    /// <param name="exp"></param>
    /// <returns></returns>
    public string IncreaseExp(long exp)
    {
        var text = "";
        foreach (var i in this)
        {
            text += i.IncreaseExp(exp) + "\n";
        }

        return text;
    }

    [NotMapped]
    public BattleSimulator CurrentBattle { get; set; }

    public HashSet<CharacterPartials.Character> Characters { get; set; } = [];
    public IEnumerator<CharacterPartials.Character> GetEnumerator()
    {
        return Characters.GetEnumerator();
    }
 

    public async Task<CharacterTeam> LoadTeamGearWithPlayerDataAsync(DiscordUser? user = null)
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            if (i is Player player) await player.LoadPlayerDataAsync(user);
            i.LoadGear();
        }
    
        return this;
    }
    public CharacterTeam LoadTeam()
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            i.LoadGear();
        }

        return this;

    }

    public CharacterTeam LoadTeamGearWithPlayerData(ClaimsPrincipal user)
    {
        foreach (var i in Characters)
        {
            i.Team = this;
            if (i is Player player)
            {
                player.LoadPlayerData(user);
            }
            i.LoadGear();
        }


        return this;
    }
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }



        /// <param name="character"></param>

        /// <returns></returns>

    public virtual bool Add(CharacterPartials.Character character)
    {
        character.Team = this;
        return Characters.Add(character);
    }

    public void AddRange(IEnumerable<CharacterPartials.Character> characters)
    {
        
        foreach (var i in Characters)
        {
            if(i is null) continue;
            Add(i);
        }
    }
    public void ExceptWith(IEnumerable<CharacterPartials.Character> other)
    {
        Characters.ExceptWith(other);
    }

    public void IntersectWith(IEnumerable<CharacterPartials.Character> other)
    {
        Characters.IntersectWith(other);
    }

    public bool IsProperSubsetOf(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.IsProperSubsetOf(other);
    }

    public bool IsProperSupersetOf(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.IsProperSupersetOf(other);
    }

    public bool IsSubsetOf(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.IsSubsetOf(other);
    }

    public bool IsSupersetOf(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.IsSupersetOf(other);
    }

    public bool Overlaps(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.Overlaps(other);
    }

    public bool SetEquals(IEnumerable<CharacterPartials.Character> other)
    {
        return Characters.SetEquals(other);
    }

    public void SymmetricExceptWith(IEnumerable<CharacterPartials.Character> other)
    {
        Characters.SymmetricExceptWith(other);
    }

    public void UnionWith(IEnumerable<CharacterPartials.Character> other)
    {
        Characters.UnionWith(other);
    }


    void ICollection<CharacterPartials.Character>.Add(CharacterPartials.Character item)
    {
        Add(item);
    }

    public void Clear()
    {
        Characters.Clear();
    }

    public CharacterTeam(params CharacterPartials.Character[] characters)
    {

        Characters = characters.ToHashSet();
    }





    public bool Contains(CharacterPartials.Character character)
    {
        return Characters.Contains(character);
    }

    public void CopyTo(CharacterPartials.Character[] array, int arrayIndex)
    {
        Characters.CopyTo(array,arrayIndex);
    }

    public bool Remove(CharacterPartials.Character character)
    {
        return Characters.Remove(character);
    }

    public int Count => Characters.Count;
    public bool IsReadOnly { get; }
}