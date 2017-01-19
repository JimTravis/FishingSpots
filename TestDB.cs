using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FishingSpots.Model;
using FishingSpots.ViewModel;

namespace FishingSpots
{
    class TestDB
    {
        String[] species = new string[] { "perch", "trout", "bass", "tuna", "salmon" };
        String[] colors = new string [] { "red", "green", "pink", "silver", "gold" };
        String[] baits = new string [] { "worm", "spinnerbait", "texas", "carolina", "drop shot"};

        int _cVisits = 0;
        int _cSpots = 0;
        int _spotIndex = 0;
        int _visitIndex = 0;
        Random rand;

        public TestDB(int cSpots, int cVisits)
        {
            _cSpots = cSpots;
            _cVisits = cVisits;
        }

        public int CVisits
        {
            get{return _cVisits;}
            set{_cVisits = value;}
        }

        public int CSpots
        {
            get { return _cSpots; }
            set { _cSpots = value; }
        }

        public Boolean PopulateDB()
        {
            if(CVisits == 0 || CSpots == 0)
                return false;
            rand = new Random();

            for (int i = 0; i < CSpots; i++)
            {
                App.ViewModel.AddSpot(RandomSpot());
                int ID = (from spots in App.ViewModel.AllSpots
                          select spots.SpotItemId).Max();

                for (int j = 0; j < CVisits; j++)
                {
                    App.ViewModel.AddVisit(RandomVisit(ID));
                }
            }

            return true;
        }
        
        private VisitTable RandomVisit(int spotID)
        {
            VisitTable visit = new VisitTable();
            visit.Bait = baits[rand.Next(5)];
            visit.BaitColor = colors[rand.Next(5)];
            visit.Species = species[rand.Next(5)];
            visit.Caught = 2;
            visit.Depth = 15;
            visit.Rating = rand.Next(6);
            visit.Temp = 70;
            visit.Tide = "non-tidal";
            visit.VisitDateTime = DateTime.Now.AddDays(-rand.NextDouble()*60);
            visit.WaterTemp = 72;
            visit.Weather = "clear";
            visit.Note = "Another random visit on " + visit.VisitDateTime.Date;
            visit.SpotID = spotID;
            
            return visit;
        }

        private SpotTable RandomSpot()
        {
            SpotTable spot = new SpotTable();
            spot.SpotName = "Random Spot " + _spotIndex++;
            spot.Latitude = (float)(rand.NextDouble() * 60);
            spot.Longitude = (float)(-rand.NextDouble() * 180);
            spot.Description = spot.SpotName + " " + spot.Latitude.ToString() + " " + spot.Longitude.ToString();
            return spot;
        }


    }
}
