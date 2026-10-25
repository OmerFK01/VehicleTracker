namespace VehicleTracker.Models
{
    public record VehicleUpdate
    {
       

        public string Id { get; set; }
        public string Route { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        public double Speed { get; set; }
        public DateTime TimeStamp { get; set; }
        public VehicleUpdate(string id, string route, double lat, double lng, double speed, DateTime timeStamp)
        {
            this.Id = id;
            this.Route = route;
            this.Lat = lat;
            this.Lng = lng;
            this.Speed = speed;
            this.TimeStamp = timeStamp;
        }

    }


}
