using UnityEngine;

/*
 * The static batching manager is used to combine all the static objects in the scene into a single mesh.
 * This is done to improve performance by reducing the number of draw calls.
 * Todo: Add support for dynamic objects. They should not be batched.
 */
public class StaticBatchingManager
{
        
    private readonly GameObject _rootNode;
        
    public StaticBatchingManager(GameObject rootNode)
    {
        _rootNode = rootNode;
    }
        
    public void Combine()
    {
        StaticBatchingUtility.Combine(_rootNode);
    }
}