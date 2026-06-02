using UnityEngine;
using System.Collections.Generic;

public class BossMimicBehavior : IEnemyBehavior
{
    public Enemy Self { get; set;}

    public Ally CopiedAlly { get; private set; }
    ISkill[] copiedSkills;

    private const int copyCooldown = 2;
    private int turnCount = 0;

    public string DecideAction(Enemy actor, List<Ally> allies, List<Enemy> enemies)
    {
        // if turncount is copycooldown, end silence and reset counter (fall through to turncount == 0 case)
        if (turnCount == copyCooldown)
        {
            if (CopiedAlly != null)
            {
                CopiedAlly.IsForceSilenced = false;
            }
            turnCount = 0;
        }
        // if turncount is 0, copy skills of a random alive ally, force silence them, and use a random skill on a random target
        if (turnCount == 0)
        {   
            // try to find target
            List<Ally> validTargets = allies.FindAll(a => a.IsAlive);
            if (validTargets.Count == 0) return $"{Self.Name} has no targets to copy!";

            // copy target skills and silence
            Ally target = validTargets[Random.Range(0, validTargets.Count)];
            CopiedAlly = target;
            copiedSkills = target.Skills;
            target.IsForceSilenced = true; // force silence the target for the rest of the fight

            // use random skill
            ISkill skillToUse = copiedSkills[Random.Range(0, copiedSkills.Length)];
            Ally skillTarget = validTargets[Random.Range(0, validTargets.Count)];
            skillToUse.Execute(Self, skillTarget);

            turnCount++;
            actor.OnSkilluse();
            return $"[-] {Self.Name} copies {target.Name}'s skills and uses {skillToUse.Name} on {skillTarget.Name}!";
        }
        // otherwise, use a random copied skill on a random target and increase counter
        else
        {
            if (copiedSkills == null) return $"{Self.Name} has no skills to use!";

            List<Ally> validTargets = allies.FindAll(a => a.IsAlive);
            if (validTargets.Count == 0) return $"{Self.Name} has no targets to attack!";

            ISkill skillToUse = copiedSkills[Random.Range(0, copiedSkills.Length)];
            Ally skillTarget = validTargets[Random.Range(0, validTargets.Count)];
            skillToUse.Execute(Self, skillTarget);

            turnCount++;
            actor.OnSkilluse();
            return $"[-] {Self.Name} uses copied skill {skillToUse.Name} on {skillTarget.Name}!";
        }
    }
}