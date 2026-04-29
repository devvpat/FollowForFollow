// Factory for getting role definitions

public enum AllyRole { Warrior, Mage, Rogue, Cleric, Default }

public static class RoleFactory
{
    public static IRoleDefinition GetRole(AllyRole role)
    {
        return role switch
        {
            AllyRole.Warrior => new WarriorRole(),
            AllyRole.Mage    => new MageRole(),
            AllyRole.Rogue   => new RogueRole(),
            AllyRole.Cleric  => new ClericRole(),
            AllyRole.Default => new DefaultRole(),
            _                => new DefaultRole()
        };
    }
}