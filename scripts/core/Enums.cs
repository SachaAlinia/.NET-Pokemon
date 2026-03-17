namespace Game.Core
{
    public enum ECharacterAnimation
    {
        idle_down,
        idle_up,
        idle_left,
        idle_right,
        turn_down,
        turn_up,
        turn_left,
        turn_right,
        walk_down,
        walk_up,
        walk_left,
        walk_right
    }

    #region levels
    public enum LevelName
    {
        map1,
        map1_house_1,
        map1_house_2,
        map1_lab,
        map1_exit,
    }

    public enum LevelGroup
    {
        SPAWNPOINTS,
        SCENETRIGGERS,
    }
    #endregion

}