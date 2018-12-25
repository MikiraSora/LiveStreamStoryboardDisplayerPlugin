using Sync.Tools;
using Sync.Tools.ConfigurationAttribute;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveStreamStoryboardDisplayerPlugin
{
    public class Setting : IConfigurable
    {
        public ConfigurationElement StoryboardFolders { get; set; } = "./";

        public ConfigurationElement StoryboardPlayerOptions { get; set; }

        public ConfigurationElement Width { get; set; } = "1600";

        public ConfigurationElement Height { get; set; } = "900";

        public void onConfigurationLoad()
        {

        }

        public void onConfigurationReload()
        {

        }

        public void onConfigurationSave()
        {

        }
        public static Setting Instance { get; } = new Setting();
    }
}
