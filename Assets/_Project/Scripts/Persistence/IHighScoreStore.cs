namespace ChromeDino
{
    public interface IHighScoreStore
    {
        int Load();
        void Save(int highScore);
    }
}
