using LiveStreamStoryboardDisplayerPlugin;
using LiveStreamStoryboardDisplayerPlugin.Danmaku;
using ReOsuStoryBoardPlayer.Kernel;
using Sync.Plugins;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LiveStreamStoryboardDisplayer
{
    public class LiveStreamStoryboardDisplayerPlugin : Plugin
    {
        PluginConfigurationManager config_manager;
        UserControllerListener listener = new UserControllerListener();
        Thread thread;

        public LiveStreamStoryboardDisplayerPlugin() : base("LiveStreamStoryboardDisplayer", "MikiraSora")
        {
            config_manager=new PluginConfigurationManager(this);
            config_manager.AddItem(Setting.Instance);

            EventBus.BindEvent<PluginEvents.InitFilterEvent>(OnInitFilter);
            EventBus.BindEvent<PluginEvents.LoadCompleteEvent>(OnLoad);
        }

        private void OnLoad(PluginEvents.LoadCompleteEvent @event)
        {
            thread=new Thread(() =>
            {
                ExecutorSync.PostTask(() => listener.RegisterFinishEvent());
                ReOsuStoryBoardPlayer.MainProgram.Main(((string)Setting.Instance.StoryboardPlayerOptions).Split(' '));
            });

            thread.Start();
        }

        private void OnInitFilter(PluginEvents.InitFilterEvent @event)
        {
            @event.Filters.AddFilter(listener);
        }

        ~LiveStreamStoryboardDisplayerPlugin()
        {

        }
    }
}
