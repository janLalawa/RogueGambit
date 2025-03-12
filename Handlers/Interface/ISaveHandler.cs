namespace RogueGambit.Handlers.Interface;

public interface ISaveHandler
{
    bool SaveCurrentGameState(GameState gameState);
    bool SaveGameToFile(string filePath);
}