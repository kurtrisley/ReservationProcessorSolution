using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMqUtils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ReservationProcessor
{
    public class ReservationListener : RabbitListener
    {
        private readonly ILogger<ReservationListener> Logger;
        private readonly ReservationHttpService Service;

        public ReservationListener(ILogger<ReservationListener> logger, 
            IOptionsMonitor<RabbitOptions> options,
            ReservationHttpService service) : base(options)
        {
            Logger = logger;
            this.Service = service;
            base.QueueName = "reservations";
            base.ExchangeName = "";
        }

        public override async Task<bool> Process(string message)
        {
            // 1. deserialize from the json into a .net object
            var request = JsonSerializer.Deserialize<ReservationMessage>(message);
            // 2. maybe log it so we can see it.
            Logger.LogInformation($"Got a reservation for {request.For}");
            // 3. do the business stuff. process the reservation. 
            // (reservations with an even nuber of books get approved, otherwise, they are denied)
            var isOk = request.Books.Split(',').Count() % 2 == 0;
            // 4. tell the api about it.
            if(isOk)
            {
                return await Service.MarkReservationAccepted(request);
            }
            else
            {
                return await Service.MarkReservationDenied(request);
            }
        }
    }


    public class ReservationMessage
    {
        public int Id { get; set; }
        public string For { get; set; }
        public string Books { get; set; }
        public string Status { get; set; }
        public DateTime ReservationCreated { get; set; }
    }


}
