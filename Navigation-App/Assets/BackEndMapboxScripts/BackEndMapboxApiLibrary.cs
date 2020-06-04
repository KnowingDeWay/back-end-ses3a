using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System;

namespace Navigation_App.Assets.BackEndMapboxScripts
{
    public class BackEndMapboxApiLibrary
    {
        public BackEndLibrary() {

        }

        public async Task<string> GetRoute(int profileNumber, double sourceX, double sourceY, double initDestX, double initDestY, params double[] destinationCoordinates) {
            int interval = 0;
            Coordinates sourceCoordinates = new Coordinates() {
                Latitude = sourceX,
                Longitude = sourceY
            };
            List<Coordinates> destinationCoordinateList = new List<Coordinates>();
            destinationCoordinateList.Add(new Coordinates() {
                Latitude = initDestX,
                Longitude = initDestY
            });
            foreach(var destCoord in destinationCoordinates) {
                destinationCoordinateList.Add(new Coordinates() {
                    Latitude = destinationCoordinates[interval],
                    Longitude = destinationCoordinates[interval + 1]
                });
                interval = interval + 2;
            }
            string coordinateString = BuildCoordinatesString(sourceCoordinates, destinationCoordinateList);
            string profile = TranslateProfileNumber(profileNumber);
            string getRouteResponse = await GetRouteTask(profile, coordinateString);
            return getRouteResponse;
        }

        private string TranslateProfileNumber(int profileNumber) {
            switch(profileNumber) {
                case 0: return "driving-traffic";
                case 1: return "driving";
                case 2: return "walking";
                case 3: return "cycling";
                default: throw new InvalidProfileNumberException("The profile number passed in was not between 0 and 3");
            }
        }

        private string BuildCoordinatesString(Coordinates sourceCoords, List<Coordinates> coordinatePairs) {
            string coordinateString = "";
            coordinateString = (coordinatePairs.Count == 0) ? 
            coordinateString + $"{sourceCoords.Longitude},{sourceCoords.Latitude}"
            : coordinateString + $"{sourceCoords.Longitude},{sourceCoords.Latitude};";
            for (int i = 0; i < coordinatePairs.Count; i++) {
                Coordinates coordinates = coordinatePairs[i];
                coordinateString = (i != coordinatePairs.Count - 1) 
                ? coordinateString + $"{coordinates.Longitude},{coordinates.Latitude};" 
                : coordinateString = coordinateString + $"{coordinates.Longitude},{coordinates.Latitude}";
            }
            return coordinateString;
        }

        private async Task<string> GetRouteTask(string profile, string coords) {
            HttpClient routingClient = new HttpClient();
            var routingClientResponse = await routingClient.GetAsync($"{SystemConfiguration.GENERAL_BASE_API_URL}{SystemConfiguration.DIRECTIONS_API_BASE_URL}{profile}/{coords}?{SystemConfiguration.TOKEN_REQUEST_PARAM}{SystemConfiguration.MAIN_TOKEN}");
            if (routingClientResponse.StatusCode == HttpStatusCode.OK) {
                var content = await routingClientResponse.Content.ReadAsStringAsync();
                dynamic routingResponse = JsonConvert.DeserializeObject(content);
                return Convert.ToString(routingResponse);
            }
            return "";
        }
    }
}