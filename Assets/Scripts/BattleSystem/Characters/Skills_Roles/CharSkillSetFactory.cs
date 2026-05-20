
using System.Security;

public enum CharacterSkillSet { Bookwyrm, Karaage, JohnDreamblade, ApolloPhoebe }

public static class CharSkillSetFactory
{
    public static ISkill[] GetSkills(CharacterSkillSet set)
    {
        return set switch
        {
            CharacterSkillSet.Bookwyrm => new ISkill[]
            {
                new BookwyrmSkills.LoreDump(),
                new BookwyrmSkills.Demonization(),
                new BookwyrmSkills.FrameManipulation(),
                new BookwyrmSkills.MouseTrap(),
            },

            CharacterSkillSet.Karaage => new ISkill[]
            {
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
            },

            CharacterSkillSet.JohnDreamblade => new ISkill[]
            {
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
            },

            CharacterSkillSet.ApolloPhoebe => new ISkill[]
            {
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
                new BookwyrmSkills.TrueTest(),
            },

            _ => throw new System.ArgumentException($"Invalid skill set: {set}")
        };
    }
}