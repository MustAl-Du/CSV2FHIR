# CSV2FHIR
This program is using the Firely .NET Core SDK https://docs.fire.ly/projects/Firely-NET-SDK/ for FHIR R4 and the NReco CSV Helper classes to:
 1. Read 1000 synthetic patient information from the excel sheet in the 'Files' folder
 2. Convert the CSV to a FHIR Format (Patient Resources)
 3. Connect to, and ingest the 1000 patients in, the Azure FHIR Service
