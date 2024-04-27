using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BRIE.Classes.Roads.Collection;
using BRIE.Classes.Roads.Sources;
using BRIE.Classes.Statics;
using Newtonsoft.Json;

namespace BRIE.Classes.RoadsSources
{
    public class OsmJson : RoadsSource
    {
        public string version { get; set; }
        public string generator { get; set; }
        public string copyright { get; set; }
        public string attribution { get; set; }
        public string license { get; set; }
        public Bounds bounds { get; set; }
        public List<Element> elements { get; set; }

        public override void ToRoadsCollection()
        {

            //traffic_signals
            //crossing
            //motorway_junction
            //stop
            //turning_circle
            //bus_stop
            //primary_link
            //residential
            //primary
            //tertiary
            //pedestrian
            //motorway_link
            //secondary
            //motorway
            //tertiary_link
            //service
            //secondary_link
            //footway
            //steps
            //living_street

            RoadsCollection.All.Clear();
            var ways = elements.Where(e => e.tags?.highway == "bus_stop").ToList();
            //var tags = elements.Select(e => e.tags).DistinctBy(t => t?.highway?.ToString()).ToList();
            ways.ForEach(way =>
            {
                Road road = new Road();
                ObservableCollection<Node> ns = new ObservableCollection<Node>();
                foreach (var node in way.nodes)
                {
                    var nodeElement = elements.Where(e => e.id == node).First();
                    Point coords = new Point(nodeElement.lat, nodeElement.lon);
                    Node Node = new Node(coords, 0, 2, road);
                    ns.Add(Node);
                }
                road.Nodes = ns;
                RoadsCollection.All.Add(road);
            });


        }

        public class Bounds
        {
            public float minlat { get; set; }
            public float minlon { get; set; }
            public float maxlat { get; set; }
            public float maxlon { get; set; }
        }

        public class Element
        {

            public string type { get; set; }
            public long id { get; set; }
            public float lat { get; set; }
            public float lon { get; set; }
            public DateTime timestamp { get; set; }
            public int version { get; set; }
            public int changeset { get; set; }
            public string user { get; set; }
            public int uid { get; set; }
            public Tags tags { get; set; }
            public long[] nodes { get; set; }
            public Member[] members { get; set; }
        }

        public class Tags
        {
            public string source { get; set; }
            public string IBGEGEOCODIGO { get; set; }
            public string name { get; set; }
            public string place { get; set; }
            public string population { get; set; }
            public string populationdate { get; set; }
            public string sourcepopulation { get; set; }
            public string wikidata { get; set; }
            public string wikipedia { get; set; }
            public string highway { get; set; }
            public string crossing { get; set; }
            public string sloped_curb { get; set; }
            public string traffic_signals { get; set; }
            public string traffic_signalsdirection { get; set; }
            public string amenity { get; set; }
            public string brand { get; set; }
            public string wheelchair { get; set; }
            public string _ref { get; set; }
            public string cuisine { get; set; }
            public string noexit { get; set; }
            public string man_made { get; set; }
            public string towertype { get; set; }
            public string artwork_type { get; set; }
            public string inscription { get; set; }
            public string tourism { get; set; }
            public string addrcity { get; set; }
            public string addrstreet { get; set; }
            public string shop { get; set; }
            public string traffic_signalssound { get; set; }
            public string fee { get; set; }
            public string addrhousenumber { get; set; }
            public string addrpostcode { get; set; }
            public string addrsuburb { get; set; }
            public string brandwikidata { get; set; }
            public string brandwikipedia { get; set; }
            public string fueldiesel { get; set; }
            public string fuelethanol { get; set; }
            public string fuelgasoline { get; set; }
            public string _operator { get; set; }
            public string self_service { get; set; }
            public string historic { get; set; }
            public string healthcare { get; set; }
            public string building { get; set; }
            public string denomination { get; set; }
            public string religion { get; set; }
            public string notbrandwikidata { get; set; }
            public string barrier { get; set; }
            public string public_transport { get; set; }
            public string nameen { get; set; }
            public string namept { get; set; }
            public string opening_hours { get; set; }
            public string phone { get; set; }
            public string website { get; set; }
            public string bus { get; set; }
            public string shelter { get; set; }
            public string surface { get; set; }
            public string traffic_calming { get; set; }
            public string leisure { get; set; }
            public string internet_access { get; set; }
            public string access { get; set; }
            public string email { get; set; }
            public string office { get; set; }
            public string network { get; set; }
            public string social_facility { get; set; }
            public string communicationmobile_phone { get; set; }
            public string direction { get; set; }
            public string architect { get; set; }
            public string height { get; set; }
            public string towerconstruction { get; set; }
            public string min_age { get; set; }
            public string paymentcash { get; set; }
            public string paymentcredit_cards { get; set; }
            public string paymentdebit_cards { get; set; }
            public string healthcarespeciality { get; set; }
            public string lanes { get; set; }
            public string oneway { get; set; }
            public string alt_name { get; set; }
            public string layer { get; set; }
            public string maxspeed { get; set; }
            public string lit { get; set; }
            public string motor_vehicle { get; set; }
            public string landuse { get; set; }
            public string IBGECD_ADMINIS { get; set; }
            public string junction { get; set; }
            public string waterway { get; set; }
            public string note { get; set; }
            public string nameetymologywikidata { get; set; }
            public string sport { get; set; }
            public string bridge { get; set; }
            public string destinationref { get; set; }
            public string destination { get; set; }
            public string bridgename { get; set; }
            public string service { get; set; }
            public string tunnel { get; set; }
            public string footway { get; set; }
            public string supervised { get; set; }
            public string emergency { get; set; }
            public string parking { get; set; }
            public string bicycle { get; set; }
            public string horse { get; set; }
            public string capacity { get; set; }
            public string area { get; set; }
            public string buildinglevels { get; set; }
            public string social_facilityfor { get; set; }
            public string atm { get; set; }
            public string studio { get; set; }
            public string smoking { get; set; }
            public string natural { get; set; }
            public string water { get; set; }
            public string intermittent { get; set; }
            public string width { get; set; }
            public string drive_through { get; set; }
            public string takeaway { get; set; }
            public string fueloctane_87 { get; set; }
            public string residential { get; set; }
            public string telecom { get; set; }
            public string fuelgasoline_87 { get; set; }
            public string operatorwikidata { get; set; }
            public string admin_level { get; set; }
            public string boundary { get; set; }
            public string type { get; set; }
            public string restriction { get; set; }
            public string route { get; set; }
        }

        public class Member
        {
            public string type { get; set; }
            public long _ref { get; set; }
            public string role { get; set; }
        }

    }
}