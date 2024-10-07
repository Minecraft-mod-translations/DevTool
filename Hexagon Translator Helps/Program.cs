using Newtonsoft.Json;
using System;
using System.Net;
using System.Runtime.CompilerServices;

namespace MyApp // Note: actual namespace depends on the project name.
{
    internal class Program
    {
        static void Main(string[] args)
        {
            List<Task<Tuple<string, Root2>>> tasks = new List<Task<Tuple<string, Root2>>>();
            using (WebClient wc = new WebClient())
            {
                string json = wc.DownloadString("https://raw.githubusercontent.com/Minecraft-mod-translations/Cloud/refs/heads/main/list.json");
                Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(json);
                foreach (string hash in myDeserializedClass.lists)
                {
                    Task<Tuple<string, Root2>> t = new Task<Tuple<string, Root2>>(() =>
                    {
                        using (WebClient wc = new WebClient())
                        {
                            string json = wc.DownloadString($"https://raw.githubusercontent.com/Minecraft-mod-translations/Cloud/refs/heads/main/files/" + hash + ".json");
                            Root2 myDeserializedClass = JsonConvert.DeserializeObject<Root2>(json);
                            return new Tuple<string, Root2>(hash, myDeserializedClass);
                        }
                    });
                    tasks.Add(t);
                    t.Start();
                }
            }

            start:
            foreach (Task<Tuple<string, Root2>> root2 in tasks)
            {
                root2.Wait();
            }

            int index = 0;
            foreach (Task<Tuple<string, Root2>> root2 in tasks)
            {
                Root2 root = root2.Result.Item2;
                Console.WriteLine(index + ". " + root.name + " | " + root.version);
                Console.WriteLine(root2.Result.Item1);
                Console.WriteLine();
                index++;
            }
            Console.Write("Selected: ");
            string selected = Console.ReadLine();
            using (WebClient wc = new WebClient())
            {
                int index2 = 0;
                foreach (Task<Tuple<string, Root2>> root2 in tasks)
                {
                    Root2 root = root2.Result.Item2;
                    if (index2 == int.Parse(selected))
                    {
                        Console.WriteLine("Selected: " + root.name);
                        File.Create("selected.jar").Close();
                        File.WriteAllBytes("selected.jar", StringToByteArray(root.file));
                            break;
                    }
                    index2++;
                }
            }
            Console.Clear();
            goto start;
        }

        public static byte[] StringToByteArray(string hex)
        {
            return Enumerable.Range(0, hex.Length)
                             .Where(x => x % 2 == 0)
                             .Select(x => Convert.ToByte(hex.Substring(x, 2), 16))
                             .ToArray();
        }
    }
    public class Root
    {
        public List<string> lists { get; set; }
    }

    public class Root2
    {
        public string name { get; set; }
        public string version { get; set; }
        public string file { get; set; }
    }
}