using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using TCPAuto.TgaTrainingComp;

namespace TCPAuto
{
    class Program
    {
        static void Main(string[] args)
        {
            TrainingComponentServiceClient proxy = new TrainingComponentServiceClient("TrainingComponentServiceBasicHttpEndpoint");
            if (proxy.ClientCredentials != null)
            {
                proxy.ClientCredentials.UserName.UserName = "WebService.Read";
                proxy.ClientCredentials.UserName.Password = "Asdf098";
            }
            var serverTime = proxy.GetServerTime();
            TrainingComponentInformationRequested trainingComponentInformationRequested = new TrainingComponentInformationRequested
            {
                ShowUnitGrid = true,
                ShowReleases = true,
            };
            TrainingComponentInformationRequested trainingComponentInformationRequestedTwo = new TrainingComponentInformationRequested
            {
                ShowUnitGrid = true,
                ShowFiles = true,
                ShowReleases = true,
            };
            TrainingComponentDetailsRequest request = new TrainingComponentDetailsRequest()
            {
                Code = "ICT50715",
                InformationRequest = trainingComponentInformationRequested,
            };
            
            //getting details for qualification
            var requestResult = proxy.GetDetails(request) as TrainingComponent;
           
            //create request objects for unit codes
            TrainingComponentDetailsRequest rr = new TrainingComponentDetailsRequest() 
            {
                InformationRequest = trainingComponentInformationRequestedTwo
            };
            TrainingComponent rrs = new TrainingComponent();



            //linked list to store xml links for units 
            List<String> AssessmentLinks = new List<string>();
            List<String> ElementLinks = new List<string>();

            //fixes the '\' in links to '/'
            string fixSlash;

            for (int i = 0; i < requestResult.Releases[0].UnitGrid.Count(); i++)
            {
                //gets unit code from main request(qualification code) array
                rr.Code = requestResult.Releases[0].UnitGrid[i].Code;

                //gets details of unit
                rrs = proxy.GetDetails(rr);


                //checks for files with xml extension
                for (int ii = 0; ii < rrs.Releases[0].Files.Count(); ii++)
                {
                    //if it does, it adds it to the list
                    if (rrs.Releases[0].Files[ii].RelativePath.Contains("xml"))
                    {

                        //replaces back slash with front slash
                        fixSlash = rrs.Releases[0].Files[ii].RelativePath.Replace("\\", "/");
                        fixSlash = "http://training.gov.au/TrainingComponentFiles/" + fixSlash;

                        if (fixSlash.Contains("Assessment"))
                        {
                            AssessmentLinks.Add(fixSlash);
                        }

                        else
                        {
                            ElementLinks.Add(fixSlash);
                        }
                    }

                    //if not just goes through the loop to check the rest of the list
                    else
                    {

                    }

                }

                Console.WriteLine(AssessmentLinks[i]);
                Console.WriteLine(ElementLinks[i]);

            }


            XDocument doc = new XDocument(new XElement("Units"));


            for (int a = 0; a < requestResult.Releases[0].UnitGrid.Count(); a++)
            {

                XElement ex = new XElement("Unit",
                                               new XElement("UnitCode", requestResult.Releases[0].UnitGrid[a].Code),
                                               new XElement("IsEssential", requestResult.Releases[0].UnitGrid[a].IsEssential.ToString()),
                                               new XElement("UnitName", requestResult.Releases[0].UnitGrid[a].Title),
                                               new XElement("AssesmentRequirments", AssessmentLinks[a]),
                                               new XElement("Elements", ElementLinks[a]));



                doc.Element("Units").Add(ex);


            }

            doc.Save("Z:\\pls.xml");


            Console.ReadKey();
        }
    }
}
