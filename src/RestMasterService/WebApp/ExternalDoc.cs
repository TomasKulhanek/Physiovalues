using System;

namespace RestMasterService.WebApp
{
    /*due to cross site scripting protection from web browser, it's not allowed to get content from external web sites from javascript. This
     * web service gets document content from Wikipedia and Wikiskripta web sites and returns the html content */
    
    [Route("/ExternalDoc")]
    public class ExternalDocDTO
    {
        public string externalUrl { get; set; }
        public string wiki { get; set; }
        public string wikiskripta { get; set; }
    }
    
    public class ExternalDoc :Service
    {
        private string WIKIPEDIA = "http://en.wikipedia.org/wiki/";
        private string WIKISKRIPTA = "http://www.wikiskripta.eu/index.php/";
        public object Get(ExternalDocDTO request)
        {
            try
            {
                var wc = new System.Net.WebClient();
                wc.Encoding = System.Text.Encoding.UTF8;
                string htmlCode = "";
                
                if (!string.IsNullOrEmpty(request.wiki)) htmlCode = wc.DownloadString(WIKIPEDIA + request.wiki);
                else if (!string.IsNullOrEmpty(request.wikiskripta))
                    htmlCode = wc.DownloadString(WIKISKRIPTA + request.wikiskripta);
                else if (!string.IsNullOrEmpty(request.externalUrl)) 
                    htmlCode = wc.DownloadString(request.externalUrl);
                return htmlCode;
            } catch (Exception e)
            {
                return ("<p>error when loading from " + request.externalUrl + " " + e.Message+"</p>");
            }


        }
    }
}
/*
namespace HumModWebSimulator.SimAppScreen
{
    [Route("/SimAppScreenMetas")]
    public class SimAppScreenMetaDTO : IReturn<List<SimAppScreenMetaDTO>>
    {
        public long Id { get; set; }
        public string Name { get; set; }
    }

    //only GET 
    public class SimAppScreenMetasService : Service
    {
        public SimAppScreenRepository Repository { get; set; }  //Injected by IOC

        public object Get(SimAppScreenMetaDTO request)
        {
            return (request.Id == 0)
                       ? Repository.GetAllSimAppScreensMeta()
                       : Repository.GetMetaById(request.Id);
        }
    }
}*/