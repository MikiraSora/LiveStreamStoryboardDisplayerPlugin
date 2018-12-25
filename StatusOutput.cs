using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LiveStreamStoryboardDisplayerPlugin
{
    public static class StatusOutput
    {
        static MemoryMappedFile file;

        static int id=0;

        static StatusOutput()
        {
            file=MemoryMappedFile.CreateOrOpen("player_status", 4096, MemoryMappedFileAccess.ReadWrite);
        }

        public static int ChangeText(string text)
        {
            lock (file)
            {
                var x=id++;

                using (StreamWriter stream = new StreamWriter(file.CreateViewStream()))
                {
                    stream.Write(text);
                    stream.Write('\0');
                }

                return x;
            }
        }
        
        public static void ChangeText(string text,int keep_time)
        {
            var x = ChangeText(text);

            Task.Delay(keep_time).ContinueWith(obj => {
                if (x==id)
                    ChangeText(string.Empty);
            });
        }
    }
}
