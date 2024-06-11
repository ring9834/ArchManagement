using Microsoft.AspNetCore.Mvc;

namespace ArchiveFileManagementNs.Controllers
{
    public class TabsNcollapse2Controller : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public string getTestData()
        {
            string j1 = "[{ \"title\": \"Animalia\", \"expanded\": true, \"folder\": true, \"children\": [{ \"title\": \"Chordate\", \"folder\": true, \"children\": [{ \"title\": \"Mammal\", \"children\": [{ \"title\": \"Primate\", \"children\": [{\"title\": \"Primate\", \"children\": []},{\"title\": \"Carnivora\", \"children\": []}]},{\"title\": \"Carnivora\", \"children\": [{ \"title\": \"Felidae\", \"lazy\": true}]}]}]},{\"title\": \"Arthropoda\", \"expanded\": true, \"folder\": true, \"children\": [{ \"title\": \"Insect\", \"children\": [{ \"title\": \"Diptera\", \"lazy\": true}]}]}]}]";
            //return Json(j1);
            //return Newtonsoft.Json.JsonConvert.SerializeObject(j1);
            //Response.ContentType = "application/json";
            return j1;
        }

        public string getTestData2()
        {
            string j2 = "[{\"title\": \"Sub item\",\"lazy\": true }, {\"title\": \"Sub folder\", \"folder\": true, \"lazy\": true } ]";
            //return Json(j2);
            //Response.ContentType = "application/json";
            return j2;
        }
    }
}
