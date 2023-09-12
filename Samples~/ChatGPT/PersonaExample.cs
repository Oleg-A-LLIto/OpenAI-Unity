using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace OpenAI 
{
    public class PersonaExample
    {
        public string prompt = "Act as a patient named Robin. You age is 64. You are in a hospital waiting room and reply to the questions. Don't break character. Your pain level used to be 2. Don't ever mention that you are an AI model."; 

        public string content_pain =  "{\"pain\":\"7\"}";
        public string content_wheelchair =  "{\"patient_in_wheelchair\":\"true\"}";

        public List<FunctionDescription> functions = new List<FunctionDescription>
            {
                new FunctionDescription
                {
                    Name = "report_pain_level",
                    Description = "Patient's pain as a number between 1 and 10.",
                    Parameters = new Parameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Property>
                        {
                            {
                                "Pain", new Property
                                {
                                    Type = "number",
                                    Description = "A number from one to ten"
                                }
                            },
                            {
                                "Reponse", new Property
                                {
                                    Type = "string",
                                    Description = "The in-character response. How the patient responds to the question."
                                }
                            }
                        },
                        Required = new List<string> { "pain", "reponse" }
                    }
                },
                new FunctionDescription
                {
                    Name = "enter_wheelchair",
                    Description = "The patient was commanded to enter the wheelchair",
                    Parameters = new Parameters
                    {
                        Type = "object",
                        Properties = new Dictionary<string, Property>
                        {
                            {
                                "Reponse", new Property
                                {
                                    Type = "string",
                                    Description = "The in-character response. How the patient responds to the question."
                                }
                            }
                        },
                        Required = new List<string> { "reponse" }
                    }
                }
            };
    }
}