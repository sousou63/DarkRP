using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Editor;

namespace Tux
{
    [EditorApp("Linux Fix", "cake", "Fix Linux Game View")]
    public class TuxMain : Window
    {
        public TuxMain()
        {
            Log.Info("Hello World!");
            BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var GameFrameType = Assembly.GetAssembly(typeof(Widget)).GetType("Editor.GameFrame");
            var GameFrame = GameFrameType.GetProperty("Singleton").GetValue(null, null) as Widget;
            var Canvas = GameFrameType.GetField("EngineCanvas", flags).GetValue(GameFrame) as Widget;
            var EngineView = GameFrameType.GetField("EngineView", flags).GetValue(GameFrame) as Widget;
            GameFrame.IsWindow = true;
            GameFrame.IsFramelessWindow = false;
            GameFrame.ShowWithoutActivating = true;
            GameFrame.Parent = null;
            GameFrame.Visible = true;

            //Just close the window since its useless (for now...)
            this.Close();
        }
    }
}