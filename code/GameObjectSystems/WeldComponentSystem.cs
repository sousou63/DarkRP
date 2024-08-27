// using Sandbox.Utility;

// namespace Scenebox;

// public sealed class WeldComponentSystem : GameObjectSystem
// {
//     public WeldComponentSystem( Scene scene ) : base( scene )
//     {
//         Listen( Stage.UpdateBones, 15, UpdateWelds, "UpdateWelds" );
//     }

//     void UpdateWelds()
//     {
//         WeldComponent[] welds = Scene.GetAllComponents<WeldComponent>().ToArray();

//         Parallel.ForEach( welds, weld =>
//         {
//             weld.UpdateWeld();
//         } );
//     }
// }