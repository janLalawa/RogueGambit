namespace RogueGambit.Handlers.Interface;

public interface ISaveHandler
{
    public GameState GameState { get; set; }

    bool SaveCurrentGameState(GameState gameState);
    bool SaveGameToFile(string filePath);
    string SaveGame(string[] args);
}