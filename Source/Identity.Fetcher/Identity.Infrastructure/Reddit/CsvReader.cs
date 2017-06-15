using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Identity.Infrastructure.Reddit
{
    public class Post
    {
        public string Title { get; set; }

        public string SubReddit { get; set; }
    }

    public class CsvReader
    {
        void Processing(int i)
        {
            if (i % 1000 == 0)
            {
                if (i % 200000 == 0)
                {
                    Console.WriteLine(".");
                }
                else
                {
                    Console.Write(".");
                }
            }
        }

        public IEnumerable<Post> Fetch(string filename)
        {
            using (StreamReader sr = new StreamReader(filename))
            {
                {
                    int i = 0;

                    while (sr.Peek() >= 0)
                    {
                        i++;
                        Processing(i);
                        var line = sr.ReadLine().Split(';');
                        if (line.Length == 2)
                            yield return new Post { Title = line[0], SubReddit = line[1] };
                    }
                }
            }

        }


    }
}
