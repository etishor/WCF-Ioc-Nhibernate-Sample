using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Client
{
    class Program
    {
        static void Main(string[] args)
        {
            Guid id;
            using (Sample.SampleServiceClient client = new Sample.SampleServiceClient())
            {
                id = client.DoWork();
            }

            using (Sample.SampleServiceClient client = new Sample.SampleServiceClient())
            {
                client.Change(id, "new name");
            }
        }
    }
}
