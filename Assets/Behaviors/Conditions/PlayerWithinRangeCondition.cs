using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "PlayerWithinRange", story: "Agent is in proximity to [Player]", category: "Conditions", id: "9467ff55b5c865284001aeb9a1542aef")]
public partial class PlayerWithinRangeCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Player;

    public override bool IsTrue()
    {
        return true;
    }

    public override void OnStart()
    {
    }

    public override void OnEnd()
    {
    }
}
