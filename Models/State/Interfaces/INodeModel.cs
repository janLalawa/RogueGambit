namespace RogueGambit.Models.State.Interfaces;

public interface INodeModel
{
    void UpdateNode(bool create = false);
    void ReadNode();
    void DestroyNode(bool instant = false);
}