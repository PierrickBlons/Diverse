using System;

namespace Diverse
{
    public class Generators
    {
        public Func<string> AdjectiveGenerator { get; set; }
        public Func<string> NameGenerator { get; set; }
        public Func<string, string> LastNameGenerator { get; set; }
    }
}