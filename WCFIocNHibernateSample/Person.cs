using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WCFIocNHibernateSample
{
    public class Person
    {
        public Person()
        { 
        }

        public virtual Guid Id { get; set; }
        public virtual string Name { get; set; }
    }
}
