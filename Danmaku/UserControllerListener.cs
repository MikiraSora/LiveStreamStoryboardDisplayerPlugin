using ReOsuStoryBoardPlayer.Parser;
using ReOsuStoryBoardPlayer.Player;
using ReOsuStoryBoardPlayer.Utils;
using Sync.MessageFilter;
using Sync.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace LiveStreamStoryboardDisplayerPlugin.Danmaku
{
    public class UserControllerListener : IFilter, ISourceDanmaku
    {
        Logger logger = new Logger<UserControllerListener>();

        public UserControllerListener()
        {

        }

        public void onMsg(ref IMessageBase msg)
        {
            msg.Cancel=true; //block all

            if (string.IsNullOrWhiteSpace(msg.Message.RawText))
                return;

            var cmd = msg.Message.RawText.Split(' ').FirstOrDefault().ToLower();

            switch (cmd)
            {
                case "play":
                    MusicPlayerManager.ActivityPlayer.Play();
                    break;

                case "pause":
                    MusicPlayerManager.ActivityPlayer.Pause();
                    break;

                case "replay":
                    MusicPlayerManager.ActivityPlayer.Jump(0);
                    MusicPlayerManager.ActivityPlayer.Play();
                    break;

                default:
                    ProcessComplexCommand(msg.Message.RawText);
                    break;
            }
        }

        private bool TryGetBeatmapSetID(string content,out int setid)
        {
            setid = -1;

            if (!int.TryParse(content, out setid))
            {
                var match = Regex.Match(content, @"beatmapsets/(\w+)");

                if (match.Success)
                    setid=int.Parse(match.Groups[1].Value);
                else
                {
                    match=Regex.Match(content, @"s/(\w+)");
                    if (match.Success)
                        setid=int.Parse(match.Groups[1].Value);
                }
            }

            return setid!=-1;
        }

        Queue<int> queue=new Queue<int>();

        private void ProcessComplexCommand(string command_line)
        {
            if (TryGetBeatmapSetID(command_line, out var setid))
                queue.Enqueue(setid);
        }

        private bool SwitchStoryboard(string path)
        {
            try
            {
                var info = BeatmapFolderInfo.Parse(path);
                StatusOutput.ChangeText($"正在切换{Path.GetFileName(info.osb_file_path)}...");
                StoryboardPlayerHelper.PlayStoryboard(info);
                StatusOutput.ChangeText($"");
            }
            catch (Exception e)
            {
                logger.LogError($"无法切换SB:原因 {e.Message}");
                return false;
            }

            return true;
        }

        private async void SwitchStoryboard(int beatmap_setid)
        {
            var path = Directory.EnumerateDirectories(Setting.Instance.StoryboardFolders, $"{beatmap_setid}*").FirstOrDefault();

            if (string.IsNullOrWhiteSpace(path))
            {
                try
                {
                    await DownloadBeatmap(beatmap_setid);
                    path=Directory.EnumerateDirectories(Setting.Instance.StoryboardFolders, $"{beatmap_setid}*").FirstOrDefault();
                }
                catch (Exception e)
                {
                    StatusOutput.ChangeText($"下载{beatmap_setid}失败！{e.Message}",5000);
                }
            }

            if (!SwitchStoryboard(path))
            {
                RequestSwitch();
            }
        }

        private Task DownloadBeatmap(int beatmap_setid)
        {
            return Task.Run(() => {
                WebClient client = new WebClient();
                var file_name = $"{beatmap_setid}.osz";
                StatusOutput.ChangeText($"正在下载{beatmap_setid}...");
                
                client.DownloadFile(new Uri($"https://mikirasora.moe/api/osu/dl_map?beatmap_setid={beatmap_setid}&api=98D037CD"), file_name);

                using (ZipArchive archive = ZipFile.Open(file_name, ZipArchiveMode.Update))
                {
                    var output_path = Path.Combine(Setting.Instance.StoryboardFolders,$"{beatmap_setid}");
                    Directory.CreateDirectory(output_path);

                    archive.ExtractToDirectory(output_path);
                }
            });
        }

        public void RegisterFinishEvent()
        {
            ((MusicPlayer)MusicPlayerManager.ActivityPlayer).FinishedPlay+=RequestSwitch;
        } 

        private bool RequestSwitch()
        {


            if (queue.Count==0)
            {
                var dirs = Directory.EnumerateDirectories(Setting.Instance.StoryboardFolders);
                var next_path = dirs.ElementAt(new Random().Next(dirs.Count()));
                SwitchStoryboard(next_path);
            }
            else
            {
                var setid = queue.Dequeue();
                SwitchStoryboard(setid);
            }

            return false;
        }
    }
}
