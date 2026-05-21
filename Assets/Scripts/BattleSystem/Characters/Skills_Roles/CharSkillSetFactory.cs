
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
                new KaraageSkills.Yabai(),
                new KaraageSkills.WingClipper(),
                new KaraageSkills.FeatherStitch(),
                new KaraageSkills.ClipFarm(),
            },

            CharacterSkillSet.JohnDreamblade => new ISkill[]
            {
                new JohnDreambladeSkills.AllKill(),
                new JohnDreambladeSkills.BiasWrecker(),
                new JohnDreambladeSkills.Aegyo(),
                new JohnDreambladeSkills.VacantVoid(),
            },

            CharacterSkillSet.ApolloPhoebe => new ISkill[]
            {
                new ApolloPhoebeSkills.WorldCuttingSlash(),
                new ApolloPhoebeSkills.Riposte(),
                new ApolloPhoebeSkills.Instigation(),
                new ApolloPhoebeSkills.Blackout(),
            },

            _ => throw new System.ArgumentException($"Invalid skill set: {set}")
        };
    }
}