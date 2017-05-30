using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartyInvites.HelloWorldDomain.Services
{
    public class HelloWorld : IHelloWorld
    {
        public string GetHelloWorld()
        {
            return "Hello world!";
        }
    }
}
