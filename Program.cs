using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace CSV2FHIR
{
    class Program
    {
        /// <summary>
        /// This program is using the Firely .NET Core SDK https://docs.fire.ly/projects/Firely-NET-SDK/ for FHIR R4 and the NReco CSV Helper classes to:
        ///     1- Read 1000 synthetic patient information from the excel sheet in the 'Files' folder
        ///     2- Convert the CSV to a FHIR Format (Patient Resources)
        ///     3- Connect to, and ingest the 1000 patients in, the Azure FHIR Service
        /// </summary>
        static string fhirServiceURL = "TODO - ADD THE FHIR SERVICE FULL URL";
        static string access_token = "TODO - ADD THE ACCESS TOKEN FROM POSTMAN";
        static string csvFileLocatoin = Path.GetFullPath(Path.Combine(System.AppContext.BaseDirectory, @"..\..\..\")) + @"Files\MasterPatientList.csv";
        static List<CSVPatient> csvPatients = new List<CSVPatient>();
        static List<Patient> fhirPatients = new List<Patient>();
        static Patient fhirPatient = new Patient();
        static FhirClient fhirClient;
        
        static void Main(string[] args)
        {
            try
            {
                // Open the CSV File, and populate the List of CSVPatients
                csvPatients = CSVPatientsFromCSV();

                // Generate a List of FHIRPatients from the List of CSVPatients
                fhirPatients = FHIRPatientsFromCSV();

                // Connect to the FHIR Service 
                fhirClient = ConnectToFHIRService();

                // Create the Patient Resrouces in the FHIR Service
                CreatePatientResources();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static List<CSVPatient> CSVPatientsFromCSV()
        {
            using (var streamRdr = new System.IO.StreamReader(csvFileLocatoin))
            {
                var csvReader = new NReco.Csv.CsvReader(streamRdr, ",");

                while (csvReader.Read())
                {
                    if (csvReader[0].ToString() != "Id")
                    {
                        // populate the CSVPatients
                        csvPatients.Add(new CSVPatient
                        {
                            Id = csvReader[0],
                            DateOfBirth = DateTime.Parse(csvReader[1].ToString()),
                            SSN = csvReader[2],
                            FirstName = csvReader[3],
                            MiddleName = csvReader[5],
                            LastName = csvReader[4],
                            MaterialStatus = csvReader[6],
                            Race = csvReader[7],
                            Gender = csvReader[8],
                            Address = csvReader[9],
                            City = csvReader[10],
                            ZipCode = int.Parse(csvReader[11]),
                            State = csvReader[12],
                            Country = csvReader[13],
                            PhoneNumber = csvReader[14],
                            EMail = csvReader[15]
                        });

                    }

                }
            }
            return csvPatients;
        }

        private static List<Patient> FHIRPatientsFromCSV()
        {
            foreach (CSVPatient csvPatient in csvPatients)
            {
                fhirPatient = new Patient
                {
                    Name = new System.Collections.Generic.List<HumanName>()
                        {
                            new HumanName()
                            {
                                Use = HumanName.NameUse.Official,
                                Family = csvPatient.LastName,
                                Given = new List<string> { csvPatient.FirstName, csvPatient.MiddleName }
                            }
                        },
                    Gender = csvPatient.Gender == "M" ? AdministrativeGender.Male : (csvPatient.Gender == "F" ? AdministrativeGender.Female : AdministrativeGender.Other),
                    BirthDate = csvPatient.DateOfBirth.ToString("yyyy-MM-dd"),
                    Active = true,
                    Id = csvPatient.Id,
                    MaritalStatus = csvPatient.MaterialStatus == "M" ? new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-MaritalStatus", "M") : (csvPatient.MaterialStatus == "S" ? new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-MaritalStatus", "S") : new CodeableConcept("http://terminology.hl7.org/CodeSystem/v3-MaritalStatus", "UNK")),
                    Identifier = new List<Identifier>()
                        {
                            new Identifier()
                            {
                                System = "http://hl7.org/fhir/sid/us-ssn",
                                Value = csvPatient.SSN
                            }
                        },
                    Address = new List<Address>()
                        {
                            new Address()
                            {
                                Line = new string[] {csvPatient.Address },
                                City = csvPatient.City,
                                State = csvPatient.State,
                                PostalCode = csvPatient.ZipCode.ToString(),
                                Country = csvPatient.Country
                            }
                        },
                    Telecom = new List<ContactPoint>()
                        {
                            new ContactPoint()
                            {
                                System = ContactPoint.ContactPointSystem.Phone,
                                Value = csvPatient.PhoneNumber
                            },
                            new ContactPoint()
                            {
                                System = ContactPoint.ContactPointSystem.Email,
                                Value = csvPatient.EMail
                            }
                        }
                };
                fhirPatients.Add(fhirPatient);
            }
            return fhirPatients;
        }

        private static FhirClient ConnectToFHIRService()
        {
            var handler = new AuthorizationMessageHandler();
            var bearerToken = access_token;
            var settings = new FhirClientSettings
            {
                Timeout = 120000,
                PreferredFormat = ResourceFormat.Json,
                VerifyFhirVersion = true,
                PreferredReturn = Prefer.ReturnMinimal,
                UseFormatParameter = true
            };
            handler.Authorization = new AuthenticationHeaderValue("Bearer", bearerToken);
            var client = new FhirClient(fhirServiceURL, settings, handler);
            return client;
        }

        private static void CreatePatientResources()
        {
            foreach (Patient fhirPatient in fhirPatients)
            {
                var created_patient = fhirClient.Create<Patient>(fhirPatient);
            }
        }

        private class AuthorizationMessageHandler : HttpClientHandler
        {
            public System.Net.Http.Headers.AuthenticationHeaderValue Authorization { get; set; }
            protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
            {
                if (Authorization != null)
                    request.Headers.Authorization = Authorization;
                return await base.SendAsync(request, cancellationToken);
            }
        }
    }

    
}
