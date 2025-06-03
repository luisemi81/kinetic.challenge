using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kinetic.Common.DTO
{
    public class ProductMessage
    {
        public int ProductId { get; set; }
        public required string EventType { get; set; } // "create", "update", "delete"
        public DateTime Timestamp { get; set; }
        public required string Payload { get; set; }
    }
}
