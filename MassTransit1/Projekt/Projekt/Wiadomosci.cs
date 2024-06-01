using MassTransit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wiadomosci;
using MassTransit;
using static MassTransit.MessageHeaders;
using System.Runtime.Remoting.Contexts;

namespace Wiadomosci
{
    public interface IWiadomosc1
    {
        int temperatura { get; set; }
    }

    public interface IWiadomosc2
    {
        string wiadomosc2 { get; set; }
    }
}