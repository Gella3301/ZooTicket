using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ZooTicket
{



    public class Ticket
    {
        public string Id { get; set; }
        public string Type { get; set; } // "Взрослый", "Детский"
        public decimal Price { get; set; }
        public DateTime SaleDate { get; set; }
        public string Status { get; set; } // "Действителен", "Использован"
        public string SoldBy { get; set; }
    }
}
