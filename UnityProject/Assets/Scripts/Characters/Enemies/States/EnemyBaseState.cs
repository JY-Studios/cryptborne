namespace Characters.Enemies.States
{
    public abstract class EnemyBaseState
    {
        public abstract void Enter(EnemyStateManager enemy);
        public abstract void Update(EnemyStateManager enemy);
        public abstract void Exit(EnemyStateManager enemy);
    }
}